const { APIError } = require('../middleware/errorHandler');

/**
 * Craft with ingredients
 */
const craft = async (req, res, next) => {
    try {
        const playerId = req.user.playerId;
        const { ingredients } = req.body;

        if (!ingredients || !Array.isArray(ingredients) || ingredients.length < 2) {
            throw new APIError('At least 2 ingredients required', 400);
        }

        // TODO: Validate player has ingredients
        // TODO: Find matching recipe
        // TODO: Consume ingredients and give rewards

        // Stub response
        const matchedRecipe = null; // TODO: Find in database

        if (matchedRecipe) {
            res.json({
                success: true,
                recipe: matchedRecipe,
                isNewDiscovery: true,
                rewards: {
                    coins: 50,
                    experience: 20
                }
            });
        } else {
            res.json({
                success: false,
                message: 'No matching recipe found',
                ingredientsConsumed: true
            });
        }
    } catch (error) {
        next(error);
    }
};

/**
 * Get discovered recipes
 */
const getDiscoveredRecipes = async (req, res, next) => {
    try {
        const playerId = req.user.playerId;

        // TODO: Fetch from database

        res.json({
            recipes: [],
            totalDiscovered: 0
        });
    } catch (error) {
        next(error);
    }
};

/**
 * Get all recipes (with hidden ones)
 */
const getAllRecipes = async (req, res, next) => {
    try {
        const playerId = req.user.playerId;

        // TODO: Fetch all recipes, mark undiscovered as hidden

        res.json({
            recipes: [
                {
                    id: 'mojito',
                    name: 'Mojito',
                    category: 'classic_cocktail',
                    discovered: false,
                    hint: 'Contains citrus and herb'
                }
                // ... more recipes
            ],
            totalRecipes: 118,
            discovered: 0
        });
    } catch (error) {
        next(error);
    }
};

/**
 * Get hint for recipe
 */
const getHint = async (req, res, next) => {
    try {
        const playerId = req.user.playerId;
        const { recipeId, hintLevel } = req.body;

        if (!recipeId) {
            throw new APIError('recipeId is required', 400);
        }

        const level = hintLevel || 1;
        const hintCosts = { 1: 100, 2: 200, 3: 300, 4: 400 };

        // TODO: Check if player has coins
        // TODO: Deduct and return hint

        res.json({
            recipeId,
            hintLevel: level,
            hint: 'This recipe contains citrus',
            cost: hintCosts[level]
        });
    } catch (error) {
        next(error);
    }
};

/**
 * Get ingredients catalog
 */
const getIngredientsCatalog = async (req, res, next) => {
    try {
        res.json({
            ingredients: [
                {
                    id: 'mint',
                    name: 'Mint',
                    category: 'herb',
                    rarity: 'common'
                },
                {
                    id: 'lime',
                    name: 'Lime',
                    category: 'citrus',
                    rarity: 'common'
                },
                {
                    id: 'white_rum',
                    name: 'White Rum',
                    category: 'spirit',
                    rarity: 'common'
                }
                // ... more ingredients
            ]
        });
    } catch (error) {
        next(error);
    }
};

module.exports = {
    craft,
    getDiscoveredRecipes,
    getAllRecipes,
    getHint,
    getIngredientsCatalog
};
