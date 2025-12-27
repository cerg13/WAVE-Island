const express = require('express');
const router = express.Router();
const shopController = require('../controllers/shopController');
const { authenticate } = require('../middleware/auth');

router.use(authenticate);

/**
 * @route   GET /api/shop
 * @desc    Get shop items
 * @access  Private
 */
router.get('/', shopController.getShopItems);

/**
 * @route   POST /api/shop/buy
 * @desc    Buy item from shop
 * @access  Private
 */
router.post('/buy', shopController.buyItem);

/**
 * @route   POST /api/shop/sell
 * @desc    Sell item to shop
 * @access  Private
 */
router.post('/sell', shopController.sellItem);

/**
 * @route   GET /api/shop/daily-deals
 * @desc    Get daily special deals
 * @access  Private
 */
router.get('/daily-deals', shopController.getDailyDeals);

module.exports = router;
