const express = require('express');
const router = express.Router();
const spiritsController = require('../controllers/spiritsController');
const { authenticate } = require('../middleware/auth');

router.use(authenticate);

/**
 * @route   GET /api/spirits
 * @desc    Get owned spirits
 * @access  Private
 */
router.get('/', spiritsController.getOwnedSpirits);

/**
 * @route   GET /api/spirits/catalog
 * @desc    Get all spirits catalog
 * @access  Private
 */
router.get('/catalog', spiritsController.getSpiritsCatalog);

/**
 * @route   POST /api/spirits/pull
 * @desc    Single gacha pull
 * @access  Private
 */
router.post('/pull', spiritsController.singlePull);

/**
 * @route   POST /api/spirits/pull-ten
 * @desc    Ten gacha pulls
 * @access  Private
 */
router.post('/pull-ten', spiritsController.tenPull);

/**
 * @route   POST /api/spirits/set-active
 * @desc    Set active companion spirit
 * @access  Private
 */
router.post('/set-active', spiritsController.setActiveSpirit);

/**
 * @route   POST /api/spirits/level-up
 * @desc    Level up a spirit
 * @access  Private
 */
router.post('/level-up', spiritsController.levelUpSpirit);

module.exports = router;
