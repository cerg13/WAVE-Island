const express = require('express');
const router = express.Router();
const playerController = require('../controllers/playerController');
const { authenticate } = require('../middleware/auth');

// All routes require authentication
router.use(authenticate);

/**
 * @route   GET /api/player/profile
 * @desc    Get player profile and stats
 * @access  Private
 */
router.get('/profile', playerController.getProfile);

/**
 * @route   PUT /api/player/profile
 * @desc    Update player profile
 * @access  Private
 */
router.put('/profile', playerController.updateProfile);

/**
 * @route   GET /api/player/inventory
 * @desc    Get player inventory
 * @access  Private
 */
router.get('/inventory', playerController.getInventory);

/**
 * @route   POST /api/player/save
 * @desc    Save game progress
 * @access  Private
 */
router.post('/save', playerController.saveProgress);

/**
 * @route   GET /api/player/load
 * @desc    Load game progress
 * @access  Private
 */
router.get('/load', playerController.loadProgress);

/**
 * @route   GET /api/player/achievements
 * @desc    Get player achievements
 * @access  Private
 */
router.get('/achievements', playerController.getAchievements);

/**
 * @route   POST /api/player/daily-bonus
 * @desc    Claim daily login bonus
 * @access  Private
 */
router.post('/daily-bonus', playerController.claimDailyBonus);

module.exports = router;
