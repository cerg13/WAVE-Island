require('dotenv').config();

const express = require('express');
const cors = require('cors');
const helmet = require('helmet');
const rateLimit = require('express-rate-limit');

const authRoutes = require('./routes/auth');
const playerRoutes = require('./routes/player');
const gardenRoutes = require('./routes/garden');
const alchemyRoutes = require('./routes/alchemy');
const spiritsRoutes = require('./routes/spirits');
const shopRoutes = require('./routes/shop');
const iikoRoutes = require('./routes/iiko');

const { errorHandler } = require('./middleware/errorHandler');
const { requestLogger } = require('./middleware/logger');

const app = express();
const PORT = process.env.PORT || 3000;

// Security middleware
app.use(helmet());
app.use(cors({
    origin: process.env.CORS_ORIGIN || '*',
    methods: ['GET', 'POST', 'PUT', 'DELETE'],
    allowedHeaders: ['Content-Type', 'Authorization']
}));

// Rate limiting
const limiter = rateLimit({
    windowMs: 15 * 60 * 1000, // 15 minutes
    max: 100, // limit each IP to 100 requests per windowMs
    message: { error: 'Too many requests, please try again later.' }
});
app.use('/api/', limiter);

// Body parsing
app.use(express.json({ limit: '10mb' }));
app.use(express.urlencoded({ extended: true }));

// Logging
app.use(requestLogger);

// Health check
app.get('/health', (req, res) => {
    res.json({
        status: 'ok',
        version: process.env.GAME_VERSION || '1.0.0',
        timestamp: new Date().toISOString()
    });
});

// API Routes
app.use('/api/auth', authRoutes);
app.use('/api/player', playerRoutes);
app.use('/api/garden', gardenRoutes);
app.use('/api/alchemy', alchemyRoutes);
app.use('/api/spirits', spiritsRoutes);
app.use('/api/shop', shopRoutes);
app.use('/api/iiko', iikoRoutes);

// 404 handler
app.use((req, res) => {
    res.status(404).json({ error: 'Route not found' });
});

// Error handling
app.use(errorHandler);

// Start server
app.listen(PORT, () => {
    console.log(`[Server] WAVE Island Backend running on port ${PORT}`);
    console.log(`[Server] Environment: ${process.env.NODE_ENV || 'development'}`);
});

module.exports = app;
