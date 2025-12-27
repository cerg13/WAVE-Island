const express = require('express');
const router = express.Router();
const alchemyController = require('../controllers/alchemyController');
const { authenticate } = require('../middleware/auth');

router.use(authenticate);

/**
 * @route   POST /api/alchemy/craft
 * @desc    Attempt to craft with ingredients
 * @access  Private
 */
router.post('/craft', alchemyController.craft);

/**
 * @route   GET /api/alchemy/recipes
 * @desc    Get discovered recipes
 * @access  Private
 */
router.get('/recipes', alchemyController.getDiscoveredRecipes);

/**
 * @route   GET /api/alchemy/recipes/all
 * @desc    Get all recipes (with hidden ones shown as ???)
 * @access  Private
 */
router.get('/recipes/all', alchemyController.getAllRecipes);

/**
 * @route   POST /api/alchemy/hint
 * @desc    Get/buy hint for a recipe
 * @access  Private
 */
router.post('/hint', alchemyController.getHint);

/**
 * @route   GET /api/alchemy/ingredients
 * @desc    Get ingredients catalog
 * @access  Private
 */
router.get('/ingredients', alchemyController.getIngredientsCatalog);

module.exports = router;
