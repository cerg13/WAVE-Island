const express = require('express');
const router = express.Router();
const authController = require('../controllers/authController');
const { authenticate } = require('../middleware/auth');

/**
 * @route   POST /api/auth/register
 * @desc    Register new player
 * @access  Public
 */
router.post('/register', authController.register);

/**
 * @route   POST /api/auth/login
 * @desc    Login player
 * @access  Public
 */
router.post('/login', authController.login);

/**
 * @route   POST /api/auth/guest
 * @desc    Create guest account
 * @access  Public
 */
router.post('/guest', authController.createGuest);

/**
 * @route   POST /api/auth/refresh
 * @desc    Refresh JWT token
 * @access  Private
 */
router.post('/refresh', authenticate, authController.refreshToken);

/**
 * @route   POST /api/auth/link-iiko
 * @desc    Link iiko account
 * @access  Private
 */
router.post('/link-iiko', authenticate, authController.linkIiko);

/**
 * @route   GET /api/auth/me
 * @desc    Get current user info
 * @access  Private
 */
router.get('/me', authenticate, authController.getCurrentUser);

module.exports = router;
