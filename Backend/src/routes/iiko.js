const express = require('express');
const router = express.Router();
const iikoController = require('../controllers/iikoController');
const { authenticate } = require('../middleware/auth');

/**
 * @route   POST /api/iiko/webhook
 * @desc    Handle iiko order webhook
 * @access  Public (verified by signature)
 */
router.post('/webhook', iikoController.handleWebhook);

// Protected routes
router.use(authenticate);

/**
 * @route   GET /api/iiko/menu
 * @desc    Get synced menu from iiko
 * @access  Private
 */
router.get('/menu', iikoController.getMenu);

/**
 * @route   GET /api/iiko/orders
 * @desc    Get player's order history
 * @access  Private
 */
router.get('/orders', iikoController.getOrderHistory);

/**
 * @route   POST /api/iiko/link-phone
 * @desc    Link phone number to account
 * @access  Private
 */
router.post('/link-phone', iikoController.linkPhone);

/**
 * @route   POST /api/iiko/verify-order
 * @desc    Verify order and grant rewards
 * @access  Private
 */
router.post('/verify-order', iikoController.verifyOrder);

/**
 * @route   GET /api/iiko/bonuses
 * @desc    Get available iiko bonuses
 * @access  Private
 */
router.get('/bonuses', iikoController.getBonuses);

/**
 * @route   POST /api/iiko/claim-milestone
 * @desc    Claim a milestone bonus
 * @access  Private
 */
router.post('/claim-milestone', iikoController.claimMilestone);

module.exports = router;
