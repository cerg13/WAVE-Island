const { APIError } = require('../middleware/errorHandler');

/**
 * Get garden state
 */
const getGarden = async (req, res, next) => {
    try {
        const playerId = req.user.playerId;

        // TODO: Fetch from database

        res.json({
            gardenLevel: 1,
            maxPlots: 4,
            plots: [
                { index: 0, state: 'empty', plantId: null },
                { index: 1, state: 'empty', plantId: null },
                { index: 2, state: 'empty', plantId: null },
                { index: 3, state: 'empty', plantId: null }
            ]
        });
    } catch (error) {
        next(error);
    }
};

/**
 * Plant a seed
 */
const plantSeed = async (req, res, next) => {
    try {
        const playerId = req.user.playerId;
        const { plotIndex, plantId } = req.body;

        if (plotIndex === undefined || !plantId) {
            throw new APIError('plotIndex and plantId are required', 400);
        }

        // TODO: Validate plot availability
        // TODO: Check if player has seed
        // TODO: Update database

        res.json({
            message: 'Seed planted',
            plotIndex,
            plantId,
            plantedAt: new Date().toISOString()
        });
    } catch (error) {
        next(error);
    }
};

/**
 * Water a plant
 */
const waterPlant = async (req, res, next) => {
    try {
        const playerId = req.user.playerId;
        const { plotIndex } = req.body;

        if (plotIndex === undefined) {
            throw new APIError('plotIndex is required', 400);
        }

        // TODO: Update database

        res.json({
            message: 'Plant watered',
            plotIndex,
            wateredAt: new Date().toISOString()
        });
    } catch (error) {
        next(error);
    }
};

/**
 * Harvest a plant
 */
const harvestPlant = async (req, res, next) => {
    try {
        const playerId = req.user.playerId;
        const { plotIndex } = req.body;

        if (plotIndex === undefined) {
            throw new APIError('plotIndex is required', 400);
        }

        // TODO: Validate plant is ready
        // TODO: Calculate harvest amount
        // TODO: Update inventory and clear plot

        const harvest = {
            ingredientId: 'mint',
            quantity: 3,
            bonusSeed: true,
            experience: 10
        };

        res.json({
            message: 'Harvest successful',
            plotIndex,
            ...harvest
        });
    } catch (error) {
        next(error);
    }
};

/**
 * Harvest all ready plants
 */
const harvestAll = async (req, res, next) => {
    try {
        const playerId = req.user.playerId;

        // TODO: Find all ready plots and harvest

        res.json({
            message: 'All plants harvested',
            totalHarvested: 0,
            rewards: []
        });
    } catch (error) {
        next(error);
    }
};

/**
 * Upgrade garden
 */
const upgradeGarden = async (req, res, next) => {
    try {
        const playerId = req.user.playerId;

        // TODO: Check current level and cost
        // TODO: Deduct coins
        // TODO: Upgrade and unlock plots

        res.json({
            message: 'Garden upgraded',
            newLevel: 2,
            newMaxPlots: 6,
            cost: 500
        });
    } catch (error) {
        next(error);
    }
};

/**
 * Get plants catalog
 */
const getPlantsCatalog = async (req, res, next) => {
    try {
        // Return all available plants
        res.json({
            plants: [
                {
                    id: 'mint',
                    name: 'Mint',
                    growthMinutes: 15,
                    unlockLevel: 1,
                    seedPrice: 10
                },
                {
                    id: 'basil',
                    name: 'Basil',
                    growthMinutes: 20,
                    unlockLevel: 1,
                    seedPrice: 15
                },
                {
                    id: 'lime',
                    name: 'Lime',
                    growthMinutes: 60,
                    unlockLevel: 1,
                    seedPrice: 25
                }
                // ... more plants
            ]
        });
    } catch (error) {
        next(error);
    }
};

module.exports = {
    getGarden,
    plantSeed,
    waterPlant,
    harvestPlant,
    harvestAll,
    upgradeGarden,
    getPlantsCatalog
};
