const { APIError } = require('../middleware/errorHandler');
const iikoService = require('../services/iikoService');
const db = require('../services/database');

/**
 * Handle iiko webhook (order completed)
 */
const handleWebhook = async (req, res, next) => {
    try {
        const signature = req.headers['x-iiko-signature'];
        const payload = req.body;

        // Verify signature
        if (!iikoService.verifyWebhookSignature(payload, signature)) {
            console.warn('[iiko Webhook] Invalid signature');
            return res.status(401).json({ error: 'Invalid signature' });
        }

        const { orderId, organizationId, eventType, eventInfo } = payload;

        console.log(`[iiko Webhook] Event: ${eventType}, Order: ${orderId}`);

        // Only process completed orders
        if (eventType !== 'OrderClosed' && eventType !== 'DeliveryOrderClosed') {
            return res.json({ received: true, processed: false });
        }

        // Get full order details
        const order = await iikoService.getOrderById(orderId);
        if (!order) {
            console.warn('[iiko Webhook] Order not found:', orderId);
            return res.json({ received: true, processed: false });
        }

        // Find player by phone
        const phone = order.phone || eventInfo?.phone;
        if (!phone) {
            console.warn('[iiko Webhook] No phone in order');
            return res.json({ received: true, processed: false });
        }

        const normalizedPhone = iikoService.normalizePhone(phone);

        // Find player in database
        const playerResult = await db.query(
            'SELECT id, player_id FROM players WHERE iiko_phone = $1',
            [normalizedPhone]
        );

        if (playerResult.rows.length === 0) {
            console.log('[iiko Webhook] No player linked to phone:', normalizedPhone);
            return res.json({ received: true, processed: false, reason: 'No linked player' });
        }

        const player = playerResult.rows[0];

        // Check if order already processed
        const existingOrder = await db.query(
            'SELECT id FROM iiko_orders WHERE iiko_order_id = $1',
            [orderId]
        );

        if (existingOrder.rows.length > 0) {
            console.log('[iiko Webhook] Order already processed:', orderId);
            return res.json({ received: true, processed: false, reason: 'Already processed' });
        }

        // Calculate rewards
        const rewards = iikoService.processOrderForRewards(order);

        // Save order
        await db.query(
            `INSERT INTO iiko_orders (player_id, iiko_order_id, order_data, rewards_granted, created_at)
             VALUES ($1, $2, $3, $4, NOW())`,
            [player.player_id, orderId, JSON.stringify(order), JSON.stringify(rewards)]
        );

        // Update player stats
        await db.query(
            `UPDATE players SET
                iiko_total_orders = iiko_total_orders + 1,
                iiko_total_spent = iiko_total_spent + $2,
                coins = coins + $3,
                experience = experience + $4,
                gems = gems + $5,
                last_iiko_order_at = NOW()
             WHERE player_id = $1`,
            [player.player_id, rewards.orderValue, rewards.coinsEarned, rewards.expEarned, rewards.gemsEarned]
        );

        // Unlock recipes
        for (const recipeId of rewards.recipesToUnlock) {
            await db.query(
                `INSERT INTO player_recipes (player_id, recipe_id, unlocked_via, unlocked_at)
                 VALUES ($1, $2, 'iiko_order', NOW())
                 ON CONFLICT (player_id, recipe_id) DO NOTHING`,
                [player.player_id, recipeId]
            );
        }

        console.log(`[iiko Webhook] Rewards granted to ${player.player_id}:`, rewards);

        res.json({
            received: true,
            processed: true,
            playerId: player.player_id,
            rewards
        });

    } catch (error) {
        console.error('[iiko Webhook] Error:', error);
        // Always return 200 to iiko to prevent retries
        res.json({ received: true, error: error.message });
    }
};

/**
 * Get synced menu with game mappings
 */
const getMenu = async (req, res, next) => {
    try {
        const forceRefresh = req.query.refresh === 'true';
        const menu = await iikoService.getMenu(forceRefresh);

        // Map products to game recipes
        const mappedProducts = menu.products.map(product => ({
            iikoId: product.id,
            name: product.name,
            description: product.description,
            price: product.sizePrices?.[0]?.price?.currentPrice || 0,
            category: product.parentGroup,
            gameRecipeId: iikoService.mapProductToRecipe(product.id),
            hasGameRecipe: !!iikoService.mapProductToRecipe(product.id)
        }));

        // Separate mapped and unmapped
        const withRecipes = mappedProducts.filter(p => p.hasGameRecipe);
        const withoutRecipes = mappedProducts.filter(p => !p.hasGameRecipe);

        res.json({
            menu: {
                mapped: withRecipes,
                unmapped: withoutRecipes,
                totalProducts: menu.products.length,
                mappedCount: withRecipes.length
            },
            groups: menu.groups,
            revision: menu.revision,
            lastSyncedAt: menu.fetchedAt
        });

    } catch (error) {
        next(error);
    }
};

/**
 * Get player's order history
 */
const getOrderHistory = async (req, res, next) => {
    try {
        const playerId = req.user.playerId;
        const limit = parseInt(req.query.limit) || 20;
        const offset = parseInt(req.query.offset) || 0;

        // Get from database
        const ordersResult = await db.query(
            `SELECT iiko_order_id, order_data, rewards_granted, created_at
             FROM iiko_orders
             WHERE player_id = $1
             ORDER BY created_at DESC
             LIMIT $2 OFFSET $3`,
            [playerId, limit, offset]
        );

        // Get totals
        const statsResult = await db.query(
            `SELECT iiko_total_orders, iiko_total_spent
             FROM players WHERE player_id = $1`,
            [playerId]
        );

        const stats = statsResult.rows[0] || { iiko_total_orders: 0, iiko_total_spent: 0 };

        res.json({
            orders: ordersResult.rows.map(row => ({
                orderId: row.iiko_order_id,
                orderData: row.order_data,
                rewards: row.rewards_granted,
                processedAt: row.created_at
            })),
            stats: {
                totalOrders: stats.iiko_total_orders,
                totalSpent: stats.iiko_total_spent
            },
            pagination: {
                limit,
                offset,
                hasMore: ordersResult.rows.length === limit
            }
        });

    } catch (error) {
        next(error);
    }
};

/**
 * Link phone number to account
 */
const linkPhone = async (req, res, next) => {
    try {
        const playerId = req.user.playerId;
        const { phone } = req.body;

        if (!phone) {
            throw new APIError('Phone number required', 400);
        }

        const normalizedPhone = iikoService.normalizePhone(phone);

        // Check if phone already linked to another account
        const existing = await db.query(
            'SELECT player_id FROM players WHERE iiko_phone = $1 AND player_id != $2',
            [normalizedPhone, playerId]
        );

        if (existing.rows.length > 0) {
            throw new APIError('Phone already linked to another account', 409);
        }

        // Update player
        await db.query(
            'UPDATE players SET iiko_phone = $1 WHERE player_id = $2',
            [normalizedPhone, playerId]
        );

        // Check for existing orders
        const orders = await iikoService.getOrdersByPhone(normalizedPhone);
        let importedOrders = 0;
        let totalRewards = { coins: 0, exp: 0, gems: 0, recipes: [] };

        for (const order of orders) {
            // Check if not already processed
            const existing = await db.query(
                'SELECT id FROM iiko_orders WHERE iiko_order_id = $1',
                [order.id]
            );

            if (existing.rows.length === 0) {
                const rewards = iikoService.processOrderForRewards(order);

                await db.query(
                    `INSERT INTO iiko_orders (player_id, iiko_order_id, order_data, rewards_granted, created_at)
                     VALUES ($1, $2, $3, $4, $5)`,
                    [playerId, order.id, JSON.stringify(order), JSON.stringify(rewards), order.completedAt || new Date()]
                );

                totalRewards.coins += rewards.coinsEarned;
                totalRewards.exp += rewards.expEarned;
                totalRewards.gems += rewards.gemsEarned;
                totalRewards.recipes.push(...rewards.recipesToUnlock);
                importedOrders++;
            }
        }

        // Apply rewards
        if (importedOrders > 0) {
            await db.query(
                `UPDATE players SET
                    iiko_total_orders = iiko_total_orders + $2,
                    coins = coins + $3,
                    experience = experience + $4,
                    gems = gems + $5
                 WHERE player_id = $1`,
                [playerId, importedOrders, totalRewards.coins, totalRewards.exp, totalRewards.gems]
            );

            // Unlock recipes
            for (const recipeId of [...new Set(totalRewards.recipes)]) {
                await db.query(
                    `INSERT INTO player_recipes (player_id, recipe_id, unlocked_via, unlocked_at)
                     VALUES ($1, $2, 'iiko_import', NOW())
                     ON CONFLICT (player_id, recipe_id) DO NOTHING`,
                    [playerId, recipeId]
                );
            }
        }

        res.json({
            success: true,
            phone: normalizedPhone,
            importedOrders,
            rewards: totalRewards
        });

    } catch (error) {
        next(error);
    }
};

/**
 * Verify order manually (alternative to webhook)
 */
const verifyOrder = async (req, res, next) => {
    try {
        const playerId = req.user.playerId;
        const { orderCode, phone } = req.body;

        // Get player's linked phone
        const playerResult = await db.query(
            'SELECT iiko_phone FROM players WHERE player_id = $1',
            [playerId]
        );

        const linkedPhone = playerResult.rows[0]?.iiko_phone;
        const searchPhone = phone || linkedPhone;

        if (!searchPhone) {
            throw new APIError('No phone number linked. Please link your phone first.', 400);
        }

        // Get recent orders
        const orders = await iikoService.getOrdersByPhone(
            searchPhone,
            new Date(Date.now() - 7 * 24 * 60 * 60 * 1000).toISOString(), // Last 7 days
            new Date().toISOString()
        );

        // Find unprocessed orders
        const unprocessedOrders = [];

        for (const order of orders) {
            const existing = await db.query(
                'SELECT id FROM iiko_orders WHERE iiko_order_id = $1',
                [order.id]
            );

            if (existing.rows.length === 0) {
                // If orderCode provided, filter by it
                if (orderCode && order.number !== orderCode) {
                    continue;
                }
                unprocessedOrders.push(order);
            }
        }

        if (unprocessedOrders.length === 0) {
            return res.json({
                verified: false,
                message: 'No new orders found',
                ordersChecked: orders.length
            });
        }

        // Process all unprocessed orders
        const processedRewards = { coins: 0, exp: 0, gems: 0, recipes: [] };
        const processedOrders = [];

        for (const order of unprocessedOrders) {
            const rewards = iikoService.processOrderForRewards(order);

            await db.query(
                `INSERT INTO iiko_orders (player_id, iiko_order_id, order_data, rewards_granted, created_at)
                 VALUES ($1, $2, $3, $4, NOW())`,
                [playerId, order.id, JSON.stringify(order), JSON.stringify(rewards)]
            );

            processedRewards.coins += rewards.coinsEarned;
            processedRewards.exp += rewards.expEarned;
            processedRewards.gems += rewards.gemsEarned;
            processedRewards.recipes.push(...rewards.recipesToUnlock);
            processedOrders.push({
                orderId: order.id,
                orderNumber: order.number,
                rewards
            });
        }

        // Update player
        await db.query(
            `UPDATE players SET
                iiko_total_orders = iiko_total_orders + $2,
                coins = coins + $3,
                experience = experience + $4,
                gems = gems + $5
             WHERE player_id = $1`,
            [playerId, processedOrders.length, processedRewards.coins, processedRewards.exp, processedRewards.gems]
        );

        res.json({
            verified: true,
            ordersProcessed: processedOrders.length,
            orders: processedOrders,
            totalRewards: processedRewards
        });

    } catch (error) {
        next(error);
    }
};

/**
 * Get available bonuses and milestones
 */
const getBonuses = async (req, res, next) => {
    try {
        const playerId = req.user.playerId;

        // Get player stats
        const statsResult = await db.query(
            `SELECT iiko_total_orders, iiko_claimed_milestones
             FROM players WHERE player_id = $1`,
            [playerId]
        );

        const stats = statsResult.rows[0] || { iiko_total_orders: 0, iiko_claimed_milestones: [] };
        const claimedMilestones = stats.iiko_claimed_milestones || [];

        // Check milestones
        const milestones = iikoService.checkMilestones(stats.iiko_total_orders, claimedMilestones);

        res.json({
            totalOrders: stats.iiko_total_orders,
            milestones: {
                available: milestones.available,
                claimed: milestones.claimed,
                progress: milestones.progress
            }
        });

    } catch (error) {
        next(error);
    }
};

/**
 * Claim a milestone bonus
 */
const claimMilestone = async (req, res, next) => {
    try {
        const playerId = req.user.playerId;
        const { milestoneId } = req.body;

        if (!milestoneId) {
            throw new APIError('Milestone ID required', 400);
        }

        // Get player stats
        const statsResult = await db.query(
            `SELECT iiko_total_orders, iiko_claimed_milestones
             FROM players WHERE player_id = $1`,
            [playerId]
        );

        const stats = statsResult.rows[0];
        if (!stats) {
            throw new APIError('Player not found', 404);
        }

        const claimedMilestones = stats.iiko_claimed_milestones || [];

        // Check if already claimed
        if (claimedMilestones.includes(milestoneId)) {
            throw new APIError('Milestone already claimed', 400);
        }

        // Find milestone
        const allMilestones = iikoService.getBonusMilestones();
        const milestone = allMilestones.find(m => m.id === milestoneId);

        if (!milestone) {
            throw new APIError('Milestone not found', 404);
        }

        // Check if eligible
        if (stats.iiko_total_orders < milestone.ordersRequired) {
            throw new APIError('Not enough orders to claim this milestone', 400);
        }

        // Grant rewards
        const rewards = milestone.rewards;
        const updateFields = ['iiko_claimed_milestones = array_append(iiko_claimed_milestones, $2)'];
        const updateValues = [playerId, milestoneId];
        let paramIndex = 3;

        if (rewards.coins) {
            updateFields.push(`coins = coins + $${paramIndex++}`);
            updateValues.push(rewards.coins);
        }
        if (rewards.gems) {
            updateFields.push(`gems = gems + $${paramIndex++}`);
            updateValues.push(rewards.gems);
        }

        await db.query(
            `UPDATE players SET ${updateFields.join(', ')} WHERE player_id = $1`,
            updateValues
        );

        // Unlock spirit if included
        if (rewards.spiritId) {
            await db.query(
                `INSERT INTO player_spirits (player_id, spirit_id, unlocked_via, unlocked_at)
                 VALUES ($1, $2, 'iiko_milestone', NOW())
                 ON CONFLICT (player_id, spirit_id) DO NOTHING`,
                [playerId, rewards.spiritId]
            );
        }

        res.json({
            success: true,
            milestone,
            rewards
        });

    } catch (error) {
        next(error);
    }
};

module.exports = {
    handleWebhook,
    getMenu,
    getOrderHistory,
    linkPhone,
    verifyOrder,
    getBonuses,
    claimMilestone
};
