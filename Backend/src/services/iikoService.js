const axios = require('axios');
const crypto = require('crypto');

const IIKO_API_BASE = process.env.IIKO_API_BASE_URL || 'https://api-eu.iiko.services';
const ORGANIZATION_ID = process.env.IIKO_ORGANIZATION_ID;
const API_LOGIN = process.env.IIKO_API_LOGIN;
const WEBHOOK_SECRET = process.env.IIKO_WEBHOOK_SECRET;

let cachedToken = null;
let tokenExpiresAt = null;
let cachedMenu = null;
let menuCachedAt = null;
const MENU_CACHE_DURATION = 60 * 60 * 1000; // 1 hour

/**
 * Get iiko API token with caching
 */
const getToken = async () => {
    if (cachedToken && tokenExpiresAt && Date.now() < tokenExpiresAt) {
        return cachedToken;
    }

    try {
        const response = await axios.post(`${IIKO_API_BASE}/api/1/access_token`, {
            apiLogin: API_LOGIN
        });

        cachedToken = response.data.token;
        tokenExpiresAt = Date.now() + 55 * 60 * 1000; // 55 minutes

        console.log('[iiko] Token refreshed successfully');
        return cachedToken;
    } catch (error) {
        console.error('[iiko] Token refresh failed:', error.message);
        throw new Error('Failed to authenticate with iiko API');
    }
};

/**
 * Make authenticated API request
 */
const apiRequest = async (endpoint, data = {}) => {
    const token = await getToken();

    try {
        const response = await axios.post(
            `${IIKO_API_BASE}${endpoint}`,
            { organizationId: ORGANIZATION_ID, ...data },
            {
                headers: {
                    'Authorization': `Bearer ${token}`,
                    'Content-Type': 'application/json'
                },
                timeout: 30000
            }
        );
        return response.data;
    } catch (error) {
        console.error(`[iiko] API request failed (${endpoint}):`, error.message);
        throw error;
    }
};

/**
 * Get full menu from iiko
 */
const getMenu = async (forceRefresh = false) => {
    if (!forceRefresh && cachedMenu && menuCachedAt && Date.now() - menuCachedAt < MENU_CACHE_DURATION) {
        return cachedMenu;
    }

    const data = await apiRequest('/api/1/nomenclature');

    cachedMenu = {
        groups: data.groups || [],
        products: data.products || [],
        sizes: data.sizes || [],
        revision: data.revision,
        fetchedAt: new Date().toISOString()
    };
    menuCachedAt = Date.now();

    console.log(`[iiko] Menu cached: ${cachedMenu.products.length} products`);
    return cachedMenu;
};

/**
 * Get order by ID
 */
const getOrderById = async (orderId) => {
    const data = await apiRequest('/api/1/order/by_id', {
        orderIds: [orderId]
    });
    return data.orders?.[0] || null;
};

/**
 * Get orders by phone number
 */
const getOrdersByPhone = async (phone, fromDate, toDate) => {
    const data = await apiRequest('/api/1/order/by_phone', {
        phone: normalizePhone(phone),
        dateFrom: fromDate || new Date(Date.now() - 30 * 24 * 60 * 60 * 1000).toISOString(),
        dateTo: toDate || new Date().toISOString()
    });
    return data.orders || [];
};

/**
 * Get customer info by phone
 */
const getCustomerByPhone = async (phone) => {
    try {
        const data = await apiRequest('/api/1/loyalty/iiko/customer/info', {
            phone: normalizePhone(phone),
            type: 'phone'
        });
        return data;
    } catch (error) {
        return null;
    }
};

/**
 * Normalize phone number
 */
const normalizePhone = (phone) => {
    // Remove all non-digits
    let cleaned = phone.replace(/\D/g, '');

    // Handle Russian numbers
    if (cleaned.startsWith('8') && cleaned.length === 11) {
        cleaned = '7' + cleaned.substring(1);
    }

    // Add + prefix
    if (!cleaned.startsWith('+')) {
        cleaned = '+' + cleaned;
    }

    return cleaned;
};

/**
 * Verify webhook signature
 */
const verifyWebhookSignature = (payload, signature) => {
    if (!WEBHOOK_SECRET) {
        console.warn('[iiko] Webhook secret not configured');
        return true; // Skip verification if not configured
    }

    const expectedSignature = crypto
        .createHmac('sha256', WEBHOOK_SECRET)
        .update(JSON.stringify(payload))
        .digest('hex');

    return crypto.timingSafeEqual(
        Buffer.from(signature || ''),
        Buffer.from(expectedSignature)
    );
};

/**
 * Recipe mapping (iiko product ID -> game recipe ID)
 */
const RECIPE_MAPPING = {
    // Cocktails - these would be actual iiko product IDs
    'iiko_mojito_id': 'mojito',
    'iiko_margarita_id': 'margarita',
    'iiko_daiquiri_id': 'daiquiri',
    'iiko_cuba_libre_id': 'cuba_libre',
    'iiko_moscow_mule_id': 'moscow_mule',
    'iiko_pina_colada_id': 'pina_colada',
    'iiko_caipirinha_id': 'caipirinha',
    'iiko_tequila_sunrise_id': 'tequila_sunrise',
    'iiko_gin_tonic_id': 'gin_tonic',
    'iiko_negroni_id': 'negroni',
    'iiko_paloma_id': 'paloma',
    'iiko_dark_n_stormy_id': 'dark_n_stormy',
    'iiko_jungle_bird_id': 'jungle_bird',

    // Food
    'iiko_guacamole_id': 'guacamole',
    'iiko_fish_tacos_id': 'fish_tacos',
    'iiko_ceviche_id': 'ceviche',
    'iiko_churros_id': 'churros'
};

/**
 * Map iiko product to game recipe
 */
const mapProductToRecipe = (iikoProductId) => {
    return RECIPE_MAPPING[iikoProductId] || null;
};

/**
 * Process order for game rewards
 */
const processOrderForRewards = (order) => {
    const rewards = {
        recipesToUnlock: [],
        coinsEarned: 0,
        expEarned: 0,
        gemsEarned: 0,
        spiritProgress: 0,
        orderValue: order.sum || 0,
        itemsCount: 0
    };

    if (!order || !order.items) {
        return rewards;
    }

    for (const item of order.items) {
        const gameRecipeId = mapProductToRecipe(item.productId);
        rewards.itemsCount++;

        if (gameRecipeId) {
            rewards.recipesToUnlock.push(gameRecipeId);
            rewards.coinsEarned += 50;
            rewards.expEarned += 30;
        }

        // Base rewards per item
        rewards.coinsEarned += 10;
        rewards.expEarned += 5;
    }

    // Bonus based on order value
    if (order.sum) {
        rewards.coinsEarned += Math.floor(order.sum / 10); // 10% as coins
        rewards.gemsEarned += Math.floor(order.sum / 100); // 1% as gems
    }

    // Spirit progress (towards iiko-exclusive spirits)
    rewards.spiritProgress = 1; // Each order counts as 1 progress

    return rewards;
};

/**
 * Get bonus milestones
 */
const getBonusMilestones = () => {
    return [
        {
            id: 'first_order',
            name: 'First Order',
            nameRu: 'Первый заказ',
            description: 'Place your first order at WAVE',
            descriptionRu: 'Сделайте первый заказ в WAVE',
            ordersRequired: 1,
            rewards: { gems: 50, coins: 200 }
        },
        {
            id: 'loyal_customer',
            name: 'Loyal Customer',
            nameRu: 'Постоянный клиент',
            description: 'Place 5 orders',
            descriptionRu: 'Сделайте 5 заказов',
            ordersRequired: 5,
            rewards: { spiritId: 'shisha_sage', coins: 500 }
        },
        {
            id: 'vip_status',
            name: 'VIP Status',
            nameRu: 'VIP статус',
            description: 'Place 10 orders',
            descriptionRu: 'Сделайте 10 заказов',
            ordersRequired: 10,
            rewards: { spiritId: 'wave_master', gems: 200, coins: 1000 }
        },
        {
            id: 'whale',
            name: 'Island Legend',
            nameRu: 'Легенда острова',
            description: 'Place 25 orders',
            descriptionRu: 'Сделайте 25 заказов',
            ordersRequired: 25,
            rewards: { gems: 500, coins: 2500, exclusive: true }
        }
    ];
};

/**
 * Check which milestones a player has achieved
 */
const checkMilestones = (totalOrders, claimedMilestones = []) => {
    const milestones = getBonusMilestones();
    const available = [];
    const claimed = [];
    const progress = [];

    for (const milestone of milestones) {
        if (claimedMilestones.includes(milestone.id)) {
            claimed.push(milestone);
        } else if (totalOrders >= milestone.ordersRequired) {
            available.push(milestone);
        } else {
            progress.push({
                ...milestone,
                currentProgress: totalOrders,
                progressPercent: Math.floor((totalOrders / milestone.ordersRequired) * 100)
            });
        }
    }

    return { available, claimed, progress };
};

module.exports = {
    getToken,
    getMenu,
    getOrderById,
    getOrdersByPhone,
    getCustomerByPhone,
    normalizePhone,
    verifyWebhookSignature,
    mapProductToRecipe,
    processOrderForRewards,
    getBonusMilestones,
    checkMilestones,
    RECIPE_MAPPING
};
