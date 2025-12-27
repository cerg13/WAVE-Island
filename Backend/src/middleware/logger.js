/**
 * Request logging middleware
 */
const requestLogger = (req, res, next) => {
    const start = Date.now();

    res.on('finish', () => {
        const duration = Date.now() - start;
        const logLevel = res.statusCode >= 400 ? 'WARN' : 'INFO';

        console.log(
            `[${logLevel}] ${req.method} ${req.originalUrl} - ${res.statusCode} - ${duration}ms`
        );
    });

    next();
};

module.exports = {
    requestLogger
};
