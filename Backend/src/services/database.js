const { Pool } = require('pg');

// Create PostgreSQL connection pool
const pool = new Pool({
    connectionString: process.env.DATABASE_URL,
    ssl: process.env.NODE_ENV === 'production' ? { rejectUnauthorized: false } : false,
    max: 20,
    idleTimeoutMillis: 30000,
    connectionTimeoutMillis: 2000,
});

// Test connection
pool.on('connect', () => {
    console.log('[Database] Connected to PostgreSQL');
});

pool.on('error', (err) => {
    console.error('[Database] Unexpected error:', err);
});

/**
 * Execute a query
 */
const query = async (text, params) => {
    const start = Date.now();
    const result = await pool.query(text, params);
    const duration = Date.now() - start;

    if (process.env.NODE_ENV === 'development') {
        console.log('[Database] Query:', { text: text.substring(0, 50), duration, rows: result.rowCount });
    }

    return result;
};

/**
 * Get a client from pool for transactions
 */
const getClient = () => pool.connect();

// =====================================================
// PLAYER OPERATIONS
// =====================================================

const createPlayer = async (data) => {
    const { email, passwordHash, playerName, isGuest, deviceId } = data;

    const result = await query(
        `INSERT INTO players (email, password_hash, player_name, is_guest, device_id)
         VALUES ($1, $2, $3, $4, $5)
         RETURNING *`,
        [email, passwordHash, playerName || 'Island Explorer', isGuest || false, deviceId]
    );

    return result.rows[0];
};

const getPlayerById = async (id) => {
    const result = await query('SELECT * FROM players WHERE id = $1', [id]);
    return result.rows[0];
};

const getPlayerByEmail = async (email) => {
    const result = await query('SELECT * FROM players WHERE email = $1', [email]);
    return result.rows[0];
};

const updatePlayer = async (id, updates) => {
    const fields = Object.keys(updates);
    const values = Object.values(updates);

    const setClause = fields.map((f, i) => `${f} = $${i + 2}`).join(', ');

    const result = await query(
        `UPDATE players SET ${setClause} WHERE id = $1 RETURNING *`,
        [id, ...values]
    );

    return result.rows[0];
};

// =====================================================
// INVENTORY OPERATIONS
// =====================================================

const getInventory = async (playerId) => {
    const result = await query(
        'SELECT * FROM inventory WHERE player_id = $1',
        [playerId]
    );
    return result.rows;
};

const addToInventory = async (playerId, itemId, itemType, quantity = 1) => {
    const result = await query(
        `INSERT INTO inventory (player_id, item_id, item_type, quantity)
         VALUES ($1, $2, $3, $4)
         ON CONFLICT (player_id, item_id)
         DO UPDATE SET quantity = inventory.quantity + $4, updated_at = CURRENT_TIMESTAMP
         RETURNING *`,
        [playerId, itemId, itemType, quantity]
    );
    return result.rows[0];
};

const removeFromInventory = async (playerId, itemId, quantity = 1) => {
    // First check if enough quantity exists
    const check = await query(
        'SELECT quantity FROM inventory WHERE player_id = $1 AND item_id = $2',
        [playerId, itemId]
    );

    if (!check.rows[0] || check.rows[0].quantity < quantity) {
        return null; // Not enough items
    }

    if (check.rows[0].quantity === quantity) {
        // Remove entirely
        await query(
            'DELETE FROM inventory WHERE player_id = $1 AND item_id = $2',
            [playerId, itemId]
        );
    } else {
        // Decrease quantity
        await query(
            `UPDATE inventory SET quantity = quantity - $3, updated_at = CURRENT_TIMESTAMP
             WHERE player_id = $1 AND item_id = $2`,
            [playerId, itemId, quantity]
        );
    }

    return true;
};

// =====================================================
// GARDEN OPERATIONS
// =====================================================

const getGardenPlots = async (playerId) => {
    const result = await query(
        'SELECT * FROM garden_plots WHERE player_id = $1 ORDER BY plot_index',
        [playerId]
    );
    return result.rows;
};

const initializeGarden = async (playerId, plotCount = 4) => {
    const values = [];
    const placeholders = [];

    for (let i = 0; i < plotCount; i++) {
        placeholders.push(`($1, $${i + 2}, true, 'empty')`);
        values.push(i);
    }

    await query(
        `INSERT INTO garden_plots (player_id, plot_index, is_unlocked, state)
         VALUES ${placeholders.join(', ')}
         ON CONFLICT (player_id, plot_index) DO NOTHING`,
        [playerId, ...values]
    );
};

const updatePlot = async (playerId, plotIndex, updates) => {
    const fields = Object.keys(updates);
    const values = Object.values(updates);

    const setClause = fields.map((f, i) => `${f} = $${i + 3}`).join(', ');

    const result = await query(
        `UPDATE garden_plots SET ${setClause}, updated_at = CURRENT_TIMESTAMP
         WHERE player_id = $1 AND plot_index = $2
         RETURNING *`,
        [playerId, plotIndex, ...values]
    );

    return result.rows[0];
};

// =====================================================
// RECIPE OPERATIONS
// =====================================================

const getDiscoveredRecipes = async (playerId) => {
    const result = await query(
        `SELECT dr.*, rc.display_name, rc.category, rc.difficulty
         FROM discovered_recipes dr
         JOIN recipes_catalog rc ON dr.recipe_id = rc.id
         WHERE dr.player_id = $1`,
        [playerId]
    );
    return result.rows;
};

const discoverRecipe = async (playerId, recipeId, method = 'craft') => {
    const result = await query(
        `INSERT INTO discovered_recipes (player_id, recipe_id, discovery_method)
         VALUES ($1, $2, $3)
         ON CONFLICT (player_id, recipe_id)
         DO UPDATE SET times_crafted = discovered_recipes.times_crafted + 1
         RETURNING *`,
        [playerId, recipeId, method]
    );
    return result.rows[0];
};

// =====================================================
// SPIRIT OPERATIONS
// =====================================================

const getOwnedSpirits = async (playerId) => {
    const result = await query(
        `SELECT os.*, sc.display_name, sc.element, sc.rarity, sc.abilities
         FROM owned_spirits os
         JOIN spirits_catalog sc ON os.spirit_id = sc.id
         WHERE os.player_id = $1`,
        [playerId]
    );
    return result.rows;
};

const addSpirit = async (playerId, spiritId, method = 'gacha') => {
    const result = await query(
        `INSERT INTO owned_spirits (player_id, spirit_id, obtain_method)
         VALUES ($1, $2, $3)
         ON CONFLICT (player_id, spirit_id)
         DO UPDATE SET duplicates = owned_spirits.duplicates + 1,
                       level = LEAST(owned_spirits.level + 1, 10)
         RETURNING *`,
        [playerId, spiritId, method]
    );
    return result.rows[0];
};

const setActiveSpirit = async (playerId, spiritId) => {
    // Deactivate all
    await query(
        'UPDATE owned_spirits SET is_active = false WHERE player_id = $1',
        [playerId]
    );

    // Activate selected
    const result = await query(
        `UPDATE owned_spirits SET is_active = true
         WHERE player_id = $1 AND spirit_id = $2
         RETURNING *`,
        [playerId, spiritId]
    );

    return result.rows[0];
};

module.exports = {
    query,
    getClient,

    // Players
    createPlayer,
    getPlayerById,
    getPlayerByEmail,
    updatePlayer,

    // Inventory
    getInventory,
    addToInventory,
    removeFromInventory,

    // Garden
    getGardenPlots,
    initializeGarden,
    updatePlot,

    // Recipes
    getDiscoveredRecipes,
    discoverRecipe,

    // Spirits
    getOwnedSpirits,
    addSpirit,
    setActiveSpirit
};
