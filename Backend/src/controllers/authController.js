const bcrypt = require('bcryptjs');
const { v4: uuidv4 } = require('uuid');
const { generateToken } = require('../middleware/auth');
const { APIError } = require('../middleware/errorHandler');
// const db = require('../services/database');

/**
 * Register new player
 */
const register = async (req, res, next) => {
    try {
        const { email, password, playerName } = req.body;

        if (!email || !password) {
            throw new APIError('Email and password are required', 400);
        }

        // TODO: Check if email already exists in database

        // Hash password
        const passwordHash = await bcrypt.hash(password, 10);

        // Create player
        const playerId = uuidv4();
        const player = {
            id: playerId,
            email,
            playerName: playerName || 'Island Explorer',
            level: 1,
            coins: 500,
            gems: 10,
            createdAt: new Date().toISOString()
        };

        // TODO: Save to database

        // Generate token
        const token = generateToken({
            userId: player.id,
            playerId: player.id,
            email: player.email
        });

        res.status(201).json({
            message: 'Registration successful',
            token,
            player: {
                id: player.id,
                email: player.email,
                playerName: player.playerName,
                level: player.level
            }
        });
    } catch (error) {
        next(error);
    }
};

/**
 * Login player
 */
const login = async (req, res, next) => {
    try {
        const { email, password } = req.body;

        if (!email || !password) {
            throw new APIError('Email and password are required', 400);
        }

        // TODO: Fetch player from database
        // const player = await db.getPlayerByEmail(email);

        // Stub for testing
        const player = null;

        if (!player) {
            throw new APIError('Invalid email or password', 401);
        }

        // Verify password
        const isValid = await bcrypt.compare(password, player.passwordHash);
        if (!isValid) {
            throw new APIError('Invalid email or password', 401);
        }

        // Generate token
        const token = generateToken({
            userId: player.id,
            playerId: player.id,
            email: player.email
        });

        res.json({
            message: 'Login successful',
            token,
            player: {
                id: player.id,
                email: player.email,
                playerName: player.playerName,
                level: player.level
            }
        });
    } catch (error) {
        next(error);
    }
};

/**
 * Create guest account
 */
const createGuest = async (req, res, next) => {
    try {
        const { deviceId } = req.body;

        const playerId = uuidv4();
        const guestPlayer = {
            id: playerId,
            isGuest: true,
            deviceId,
            playerName: `Guest_${playerId.substring(0, 6)}`,
            level: 1,
            coins: 500,
            gems: 10,
            createdAt: new Date().toISOString()
        };

        // TODO: Save to database

        const token = generateToken({
            userId: guestPlayer.id,
            playerId: guestPlayer.id,
            isGuest: true
        });

        res.status(201).json({
            message: 'Guest account created',
            token,
            player: {
                id: guestPlayer.id,
                playerName: guestPlayer.playerName,
                isGuest: true,
                level: guestPlayer.level
            }
        });
    } catch (error) {
        next(error);
    }
};

/**
 * Refresh JWT token
 */
const refreshToken = async (req, res, next) => {
    try {
        const { id, playerId, email } = req.user;

        const token = generateToken({
            userId: id,
            playerId,
            email
        });

        res.json({
            message: 'Token refreshed',
            token
        });
    } catch (error) {
        next(error);
    }
};

/**
 * Link iiko account
 */
const linkIiko = async (req, res, next) => {
    try {
        const { iikoPhone, iikoUserId } = req.body;
        const playerId = req.user.playerId;

        if (!iikoPhone && !iikoUserId) {
            throw new APIError('iiko phone or userId required', 400);
        }

        // TODO: Verify with iiko API and update database

        res.json({
            message: 'iiko account linked successfully',
            iikoLinked: true
        });
    } catch (error) {
        next(error);
    }
};

/**
 * Get current user info
 */
const getCurrentUser = async (req, res, next) => {
    try {
        const playerId = req.user.playerId;

        // TODO: Fetch from database

        res.json({
            id: playerId,
            // ... other player data
        });
    } catch (error) {
        next(error);
    }
};

module.exports = {
    register,
    login,
    createGuest,
    refreshToken,
    linkIiko,
    getCurrentUser
};
