/**
 * Global error handler middleware
 */
const errorHandler = (err, req, res, next) => {
    console.error('[Error]', err);

    // Validation errors
    if (err.name === 'ValidationError') {
        return res.status(400).json({
            error: 'Validation Error',
            details: err.message
        });
    }

    // Database errors
    if (err.code && err.code.startsWith('23')) {
        // PostgreSQL constraint violations
        return res.status(400).json({
            error: 'Database constraint violation',
            details: err.detail || err.message
        });
    }

    // JWT errors
    if (err.name === 'JsonWebTokenError' || err.name === 'TokenExpiredError') {
        return res.status(401).json({
            error: 'Authentication error',
            details: err.message
        });
    }

    // Custom API errors
    if (err.statusCode) {
        return res.status(err.statusCode).json({
            error: err.message,
            details: err.details
        });
    }

    // Default to 500 internal server error
    res.status(500).json({
        error: 'Internal server error',
        details: process.env.NODE_ENV === 'development' ? err.message : undefined
    });
};

/**
 * Custom API Error class
 */
class APIError extends Error {
    constructor(message, statusCode = 400, details = null) {
        super(message);
        this.statusCode = statusCode;
        this.details = details;
        this.name = 'APIError';
    }
}

module.exports = {
    errorHandler,
    APIError
};
