const axios = require('axios');

const IIKO_API_BASE = process.env.IIKO_API_BASE_URL || 'https://api-eu.iiko.services';
let cachedToken = null;
let tokenExpiresAt = null;

/**
 * Get iiko API token
 * Tokens are valid for ~1 hour, so we cache them
 */
const getToken = async () => {
    // Check if we have a valid cached token
    if (cachedToken && tokenExpiresAt && Date.now() < tokenExpiresAt) {
        return cachedToken;
    }

    try {
        const response = await axios.post(`${IIKO_API_BASE}/api/1/access_token`, {
            apiLogin: process.env.IIKO_API_LOGIN
        });

        cachedToken = response.data.token;
        // Token valid for 1 hour, refresh at 55 minutes
        tokenExpiresAt = Date.now() + 55 * 60 * 1000;

        console.log('[iiko] Token refreshed');
        return cachedToken;
    } catch (error) {
        console.error('[iiko] Failed to get token:', error.message);
        throw error;
    }
};

/**
 * Get organization menu
 */
const getMenu = async (organizationId) => {
    const token = await getToken();

    try {
        const response = await axios.post(
            `${IIKO_API_BASE}/api/1/nomenclature`,
            {
                organizationId: organizationId || process.env.IIKO_ORGANIZATION_ID
            },
            {
                headers: {
                    'Authorization': `Bearer ${token}`,
                    'Content-Type': 'application/json'
                }
            }
        );

        return response.data;
    } catch (error) {
        console.error('[iiko] Failed to get menu:', error.message);
        throw error;
    }
};

/**
 * Get order by ID
 */
const getOrder = async (organizationId, orderId) => {
    const token = await getToken();

    try {
        const response = await axios.post(
            `${IIKO_API_BASE}/api/1/order/by_id`,
            {
                organizationId: organizationId || process.env.IIKO_ORGANIZATION_ID,
                orderIds: [orderId]
            },
            {
                headers: {
                    'Authorization': `Bearer ${token}`,
                    'Content-Type': 'application/json'
                }
            }
        );

        return response.data.orders?.[0] || null;
    } catch (error) {
        console.error('[iiko] Failed to get order:', error.message);
        throw error;
    }
};

/**
 * Get orders by phone number
 */
const getOrdersByPhone = async (organizationId, phone, fromDate, toDate) => {
    const token = await getToken();

    try {
        const response = await axios.post(
            `${IIKO_API_BASE}/api/1/order/by_phone`,
            {
                organizationId: organizationId || process.env.IIKO_ORGANIZATION_ID,
                phone: phone,
                dateFrom: fromDate || new Date(Date.now() - 30 * 24 * 60 * 60 * 1000).toISOString(),
                dateTo: toDate || new Date().toISOString()
            },
            {
                headers: {
                    'Authorization': `Bearer ${token}`,
                    'Content-Type': 'application/json'
                }
            }
        );

        return response.data.orders || [];
    } catch (error) {
        console.error('[iiko] Failed to get orders by phone:', error.message);
        throw error;
    }
};

/**
 * Map iiko menu item to game recipe
 */
const mapToGameRecipe = (iikoItem, recipesMapping) => {
    // recipesMapping is a Map of iikoItemId -> gameRecipeId
    return recipesMapping.get(iikoItem.id) || null;
};

/**
 * Process order and extract game rewards
 */
const processOrderForRewards = (order, recipesMapping) => {
    const rewards = {
        recipesToUnlock: [],
        bonusCoins: 0,
        bonusExp: 0
    };

    if (!order || !order.items) {
        return rewards;
    }

    for (const item of order.items) {
        const gameRecipeId = mapToGameRecipe(item, recipesMapping);
        if (gameRecipeId) {
            rewards.recipesToUnlock.push(gameRecipeId);
            rewards.bonusCoins += 50; // Bonus coins per item
            rewards.bonusExp += 30; // Bonus exp per item
        }
    }

    // Bonus for order total
    if (order.sum) {
        rewards.bonusCoins += Math.floor(order.sum / 10); // 10% of order as coins
    }

    return rewards;
};

module.exports = {
    getToken,
    getMenu,
    getOrder,
    getOrdersByPhone,
    mapToGameRecipe,
    processOrderForRewards
};
