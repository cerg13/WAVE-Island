const { APIError } = require('../middleware/errorHandler');

/**
 * Get shop items
 */
const getShopItems = async (req, res, next) => {
    try {
        res.json({
            categories: [
                {
                    id: 'seeds',
                    name: 'Seeds',
                    items: [
                        { id: 'seed_mint', name: 'Mint Seeds', price: 10, currency: 'coins' },
                        { id: 'seed_basil', name: 'Basil Seeds', price: 15, currency: 'coins' },
                        { id: 'seed_lime', name: 'Lime Seeds', price: 25, currency: 'coins' }
                    ]
                },
                {
                    id: 'spirits',
                    name: 'Spirit Spirits',
                    items: [
                        { id: 'white_rum', name: 'White Rum', price: 50, currency: 'coins' },
                        { id: 'tequila', name: 'Tequila', price: 60, currency: 'coins' },
                        { id: 'vodka', name: 'Vodka', price: 50, currency: 'coins' }
                    ]
                },
                {
                    id: 'boosters',
                    name: 'Boosters',
                    items: [
                        { id: 'fertilizer', name: 'Fertilizer', price: 30, currency: 'coins' },
                        { id: 'speed_boost', name: 'Speed Boost', price: 50, currency: 'gems' }
                    ]
                }
            ]
        });
    } catch (error) {
        next(error);
    }
};

/**
 * Buy item from shop
 */
const buyItem = async (req, res, next) => {
    try {
        const playerId = req.user.playerId;
        const { itemId, quantity = 1 } = req.body;

        if (!itemId) {
            throw new APIError('itemId is required', 400);
        }

        // TODO: Get item from catalog
        // TODO: Check player has enough currency
        // TODO: Deduct currency
        // TODO: Add item to inventory

        res.json({
            message: 'Purchase successful',
            itemId,
            quantity,
            totalCost: 10 * quantity
        });
    } catch (error) {
        next(error);
    }
};

/**
 * Sell item to shop
 */
const sellItem = async (req, res, next) => {
    try {
        const playerId = req.user.playerId;
        const { itemId, quantity = 1 } = req.body;

        if (!itemId) {
            throw new APIError('itemId is required', 400);
        }

        // TODO: Check player has item
        // TODO: Remove from inventory
        // TODO: Add coins

        res.json({
            message: 'Item sold',
            itemId,
            quantity,
            coinsReceived: 5 * quantity
        });
    } catch (error) {
        next(error);
    }
};

/**
 * Get daily deals
 */
const getDailyDeals = async (req, res, next) => {
    try {
        // TODO: Generate based on date/seed

        res.json({
            deals: [
                {
                    id: 'deal_1',
                    itemId: 'seed_mango',
                    name: 'Mango Seeds Pack',
                    originalPrice: 100,
                    discountedPrice: 70,
                    discount: 30,
                    quantity: 5
                }
            ],
            refreshIn: 3600 // seconds until refresh
        });
    } catch (error) {
        next(error);
    }
};

module.exports = {
    getShopItems,
    buyItem,
    sellItem,
    getDailyDeals
};
