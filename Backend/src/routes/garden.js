const express = require('express');
const router = express.Router();
const gardenController = require('../controllers/gardenController');
const { authenticate } = require('../middleware/auth');

router.use(authenticate);

/**
 * @route   GET /api/garden
 * @desc    Get garden state (all plots)
 * @access  Private
 */
router.get('/', gardenController.getGarden);

/**
 * @route   POST /api/garden/plant
 * @desc    Plant a seed in a plot
 * @access  Private
 */
router.post('/plant', gardenController.plantSeed);

/**
 * @route   POST /api/garden/water
 * @desc    Water a plant
 * @access  Private
 */
router.post('/water', gardenController.waterPlant);

/**
 * @route   POST /api/garden/harvest
 * @desc    Harvest a ready plant
 * @access  Private
 */
router.post('/harvest', gardenController.harvestPlant);

/**
 * @route   POST /api/garden/harvest-all
 * @desc    Harvest all ready plants
 * @access  Private
 */
router.post('/harvest-all', gardenController.harvestAll);

/**
 * @route   POST /api/garden/upgrade
 * @desc    Upgrade garden level (unlock more plots)
 * @access  Private
 */
router.post('/upgrade', gardenController.upgradeGarden);

/**
 * @route   GET /api/garden/plants
 * @desc    Get available plants catalog
 * @access  Private
 */
router.get('/plants', gardenController.getPlantsCatalog);

module.exports = router;
