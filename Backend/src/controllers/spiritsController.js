const { APIError } = require('../middleware/errorHandler');

const SINGLE_PULL_COST = 100;
const TEN_PULL_COST = 900;

/**
 * Get owned spirits
 */
const getOwnedSpirits = async (req, res, next) => {
    try {
        const playerId = req.user.playerId;

        // TODO: Fetch from database

        res.json({
            spirits: [],
            activeSpirit: null
        });
    } catch (error) {
        next(error);
    }
};

/**
 * Get spirits catalog
 */
const getSpiritsCatalog = async (req, res, next) => {
    try {
        res.json({
            spirits: [
                {
                    id: 'ember',
                    name: 'Ember',
                    element: 'fire',
                    rarity: 'common',
                    abilities: [
                        { name: 'Craft Speed', bonus: '+5%' }
                    ]
                },
                {
                    id: 'aqua',
                    name: 'Aqua',
                    element: 'water',
                    rarity: 'uncommon',
                    abilities: [
                        { name: 'Auto Water', bonus: 'Every 4h' }
                    ]
                }
                // ... more spirits
            ]
        });
    } catch (error) {
        next(error);
    }
};

/**
 * Single gacha pull
 */
const singlePull = async (req, res, next) => {
    try {
        const playerId = req.user.playerId;

        // TODO: Check gems
        // TODO: Deduct cost
        // TODO: Perform pull with rates
        // TODO: Add to collection

        const result = performGachaPull();

        res.json({
            cost: SINGLE_PULL_COST,
            result: result,
            isNew: true
        });
    } catch (error) {
        next(error);
    }
};

/**
 * Ten gacha pulls
 */
const tenPull = async (req, res, next) => {
    try {
        const playerId = req.user.playerId;

        // TODO: Check gems
        // TODO: Deduct cost
        // TODO: Perform 10 pulls

        const results = [];
        for (let i = 0; i < 10; i++) {
            results.push(performGachaPull());
        }

        res.json({
            cost: TEN_PULL_COST,
            results: results,
            newSpirits: results.filter(r => r.isNew).length
        });
    } catch (error) {
        next(error);
    }
};

/**
 * Set active spirit
 */
const setActiveSpirit = async (req, res, next) => {
    try {
        const playerId = req.user.playerId;
        const { spiritId } = req.body;

        if (!spiritId) {
            throw new APIError('spiritId is required', 400);
        }

        // TODO: Validate ownership
        // TODO: Update database

        res.json({
            message: 'Active spirit set',
            activeSpirit: spiritId
        });
    } catch (error) {
        next(error);
    }
};

/**
 * Level up spirit
 */
const levelUpSpirit = async (req, res, next) => {
    try {
        const playerId = req.user.playerId;
        const { spiritId } = req.body;

        if (!spiritId) {
            throw new APIError('spiritId is required', 400);
        }

        // TODO: Check requirements
        // TODO: Level up

        res.json({
            message: 'Spirit leveled up',
            spiritId,
            newLevel: 2
        });
    } catch (error) {
        next(error);
    }
};

/**
 * Helper: Perform gacha pull
 */
function performGachaPull() {
    const roll = Math.random();

    // Rates: Common 60%, Uncommon 25%, Rare 10%, Epic 4%, Legendary 1%
    let rarity;
    if (roll < 0.01) rarity = 'legendary';
    else if (roll < 0.05) rarity = 'epic';
    else if (roll < 0.15) rarity = 'rare';
    else if (roll < 0.40) rarity = 'uncommon';
    else rarity = 'common';

    // TODO: Pick random spirit of that rarity
    return {
        spiritId: 'ember',
        name: 'Ember',
        rarity: rarity,
        isNew: true
    };
}

module.exports = {
    getOwnedSpirits,
    getSpiritsCatalog,
    singlePull,
    tenPull,
    setActiveSpirit,
    levelUpSpirit
};
