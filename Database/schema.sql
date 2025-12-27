-- WAVE Island Database Schema
-- PostgreSQL 15+

-- Enable UUID extension
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- =====================================================
-- PLAYERS
-- =====================================================

CREATE TABLE players (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    email VARCHAR(255) UNIQUE,
    password_hash VARCHAR(255),
    player_name VARCHAR(50) NOT NULL DEFAULT 'Island Explorer',
    avatar_id VARCHAR(50),

    -- Status
    is_guest BOOLEAN DEFAULT FALSE,
    is_active BOOLEAN DEFAULT TRUE,
    device_id VARCHAR(255),

    -- Progression
    level INTEGER DEFAULT 1,
    experience INTEGER DEFAULT 0,
    coins INTEGER DEFAULT 500,
    gems INTEGER DEFAULT 10,

    -- Garden
    garden_level INTEGER DEFAULT 1,

    -- Stats
    total_recipes_discovered INTEGER DEFAULT 0,
    total_crafts INTEGER DEFAULT 0,
    total_harvests INTEGER DEFAULT 0,

    -- iiko Integration
    iiko_user_id VARCHAR(255),
    iiko_phone VARCHAR(20),
    total_iiko_orders INTEGER DEFAULT 0,
    last_iiko_order_at TIMESTAMP,

    -- Timestamps
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    last_login_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    last_daily_bonus_at DATE
);

CREATE INDEX idx_players_email ON players(email);
CREATE INDEX idx_players_iiko_phone ON players(iiko_phone);

-- =====================================================
-- INVENTORY
-- =====================================================

CREATE TABLE inventory (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    player_id UUID NOT NULL REFERENCES players(id) ON DELETE CASCADE,
    item_id VARCHAR(100) NOT NULL,
    item_type VARCHAR(50) NOT NULL, -- 'ingredient', 'seed', 'booster', etc.
    quantity INTEGER DEFAULT 1,

    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,

    UNIQUE(player_id, item_id)
);

CREATE INDEX idx_inventory_player ON inventory(player_id);
CREATE INDEX idx_inventory_item ON inventory(item_id);

-- =====================================================
-- GARDEN PLOTS
-- =====================================================

CREATE TABLE garden_plots (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    player_id UUID NOT NULL REFERENCES players(id) ON DELETE CASCADE,
    plot_index INTEGER NOT NULL,
    is_unlocked BOOLEAN DEFAULT TRUE,

    -- Current plant
    plant_id VARCHAR(100),
    planted_at TIMESTAMP,
    last_watered_at TIMESTAMP,
    state VARCHAR(20) DEFAULT 'empty', -- empty, planted, growing, ready, withering, dead

    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,

    UNIQUE(player_id, plot_index)
);

CREATE INDEX idx_garden_player ON garden_plots(player_id);

-- =====================================================
-- DISCOVERED RECIPES
-- =====================================================

CREATE TABLE discovered_recipes (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    player_id UUID NOT NULL REFERENCES players(id) ON DELETE CASCADE,
    recipe_id VARCHAR(100) NOT NULL,
    discovered_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    discovery_method VARCHAR(50), -- 'craft', 'iiko_order', 'achievement', 'event'
    times_crafted INTEGER DEFAULT 1,

    UNIQUE(player_id, recipe_id)
);

CREATE INDEX idx_recipes_player ON discovered_recipes(player_id);

-- =====================================================
-- OWNED SPIRITS
-- =====================================================

CREATE TABLE owned_spirits (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    player_id UUID NOT NULL REFERENCES players(id) ON DELETE CASCADE,
    spirit_id VARCHAR(100) NOT NULL,
    level INTEGER DEFAULT 1,
    experience INTEGER DEFAULT 0,
    duplicates INTEGER DEFAULT 0,
    is_active BOOLEAN DEFAULT FALSE,

    obtained_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    obtain_method VARCHAR(50), -- 'gacha', 'iiko', 'achievement', 'event'

    UNIQUE(player_id, spirit_id)
);

CREATE INDEX idx_spirits_player ON owned_spirits(player_id);

-- =====================================================
-- ACHIEVEMENTS
-- =====================================================

CREATE TABLE player_achievements (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    player_id UUID NOT NULL REFERENCES players(id) ON DELETE CASCADE,
    achievement_id VARCHAR(100) NOT NULL,
    progress INTEGER DEFAULT 0,
    is_completed BOOLEAN DEFAULT FALSE,
    completed_at TIMESTAMP,
    reward_claimed BOOLEAN DEFAULT FALSE,

    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,

    UNIQUE(player_id, achievement_id)
);

CREATE INDEX idx_achievements_player ON player_achievements(player_id);

-- =====================================================
-- IIKO ORDERS
-- =====================================================

CREATE TABLE iiko_orders (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    player_id UUID REFERENCES players(id) ON DELETE SET NULL,

    iiko_order_id VARCHAR(255) UNIQUE NOT NULL,
    iiko_organization_id VARCHAR(255),
    phone VARCHAR(20),

    -- Order details
    order_items JSONB,
    total_amount DECIMAL(10, 2),

    -- Game rewards granted
    recipes_unlocked TEXT[], -- Array of recipe IDs
    rewards_granted JSONB, -- {coins: X, gems: Y, items: [...]}

    -- Status
    processed BOOLEAN DEFAULT FALSE,
    processed_at TIMESTAMP,

    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX idx_orders_player ON iiko_orders(player_id);
CREATE INDEX idx_orders_iiko_id ON iiko_orders(iiko_order_id);
CREATE INDEX idx_orders_phone ON iiko_orders(phone);

-- =====================================================
-- DAILY BONUSES LOG
-- =====================================================

CREATE TABLE daily_bonus_log (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    player_id UUID NOT NULL REFERENCES players(id) ON DELETE CASCADE,
    bonus_date DATE NOT NULL,
    streak_day INTEGER DEFAULT 1,
    coins_awarded INTEGER,
    gems_awarded INTEGER,
    items_awarded JSONB,

    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,

    UNIQUE(player_id, bonus_date)
);

CREATE INDEX idx_bonus_player ON daily_bonus_log(player_id);

-- =====================================================
-- GAME DATA (Static catalogs - can be in code/config instead)
-- =====================================================

-- Plants catalog
CREATE TABLE plants_catalog (
    id VARCHAR(100) PRIMARY KEY,
    display_name VARCHAR(100) NOT NULL,
    display_name_ru VARCHAR(100),
    category VARCHAR(50),
    rarity VARCHAR(20),
    growth_time_minutes INTEGER,
    wither_time_hours INTEGER DEFAULT 24,
    min_harvest INTEGER DEFAULT 1,
    max_harvest INTEGER DEFAULT 3,
    seed_drop_chance DECIMAL(3, 2) DEFAULT 0.20,
    unlock_level INTEGER DEFAULT 1,
    seed_buy_price INTEGER,
    seed_sell_price INTEGER
);

-- Ingredients catalog
CREATE TABLE ingredients_catalog (
    id VARCHAR(100) PRIMARY KEY,
    display_name VARCHAR(100) NOT NULL,
    display_name_ru VARCHAR(100),
    category VARCHAR(50),
    rarity VARCHAR(20),
    buy_price INTEGER,
    sell_price INTEGER,
    can_be_grown BOOLEAN DEFAULT FALSE,
    can_be_purchased BOOLEAN DEFAULT TRUE,
    source_plant_id VARCHAR(100) REFERENCES plants_catalog(id)
);

-- Recipes catalog
CREATE TABLE recipes_catalog (
    id VARCHAR(100) PRIMARY KEY,
    display_name VARCHAR(100) NOT NULL,
    display_name_ru VARCHAR(100),
    category VARCHAR(50),
    difficulty VARCHAR(20),
    rarity VARCHAR(20),
    ingredients JSONB NOT NULL, -- [{id: 'mint', quantity: 2}, ...]
    craft_time_seconds INTEGER DEFAULT 5,
    coins_reward INTEGER DEFAULT 10,
    exp_reward INTEGER DEFAULT 10,
    unlock_level INTEGER DEFAULT 1,
    iiko_menu_item_id VARCHAR(255),
    hint_level_1 TEXT,
    hint_level_2 TEXT,
    hint_level_3 TEXT
);

-- Spirits catalog
CREATE TABLE spirits_catalog (
    id VARCHAR(100) PRIMARY KEY,
    display_name VARCHAR(100) NOT NULL,
    display_name_ru VARCHAR(100),
    element VARCHAR(50),
    rarity VARCHAR(20),
    abilities JSONB,
    max_level INTEGER DEFAULT 10,
    gacha_drop_rate DECIMAL(5, 4),
    iiko_exclusive BOOLEAN DEFAULT FALSE
);

-- =====================================================
-- LEADERBOARDS (can use Redis for real-time)
-- =====================================================

CREATE TABLE leaderboards (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    player_id UUID NOT NULL REFERENCES players(id) ON DELETE CASCADE,
    leaderboard_type VARCHAR(50) NOT NULL, -- 'weekly_crafts', 'total_recipes', etc.
    score INTEGER DEFAULT 0,
    period_start DATE,
    period_end DATE,

    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,

    UNIQUE(player_id, leaderboard_type, period_start)
);

CREATE INDEX idx_leaderboards_type ON leaderboards(leaderboard_type, score DESC);

-- =====================================================
-- FUNCTIONS
-- =====================================================

-- Update timestamp function
CREATE OR REPLACE FUNCTION update_updated_at()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_at = CURRENT_TIMESTAMP;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- Apply to tables
CREATE TRIGGER update_players_timestamp
    BEFORE UPDATE ON players
    FOR EACH ROW EXECUTE FUNCTION update_updated_at();

CREATE TRIGGER update_inventory_timestamp
    BEFORE UPDATE ON inventory
    FOR EACH ROW EXECUTE FUNCTION update_updated_at();

CREATE TRIGGER update_garden_timestamp
    BEFORE UPDATE ON garden_plots
    FOR EACH ROW EXECUTE FUNCTION update_updated_at();

-- =====================================================
-- INITIAL DATA
-- =====================================================

-- Insert some starter plants
INSERT INTO plants_catalog (id, display_name, display_name_ru, category, rarity, growth_time_minutes, min_harvest, max_harvest, seed_drop_chance, unlock_level, seed_buy_price) VALUES
('mint', 'Mint', 'Мята', 'herbs', 'common', 15, 2, 4, 0.30, 1, 10),
('basil', 'Basil', 'Базилик', 'herbs', 'common', 20, 2, 3, 0.25, 1, 15),
('rosemary', 'Rosemary', 'Розмарин', 'herbs', 'common', 25, 2, 3, 0.25, 2, 20),
('lime', 'Lime', 'Лайм', 'citrus', 'common', 60, 3, 5, 0.20, 1, 25),
('lemon', 'Lemon', 'Лимон', 'citrus', 'common', 60, 3, 5, 0.20, 2, 25),
('orange', 'Orange', 'Апельсин', 'citrus', 'common', 120, 2, 4, 0.15, 3, 35),
('mango', 'Mango', 'Манго', 'tropical', 'uncommon', 180, 1, 3, 0.15, 5, 50),
('pineapple', 'Pineapple', 'Ананас', 'tropical', 'uncommon', 240, 1, 2, 0.10, 6, 60),
('passion_fruit', 'Passion Fruit', 'Маракуйя', 'tropical', 'uncommon', 240, 1, 2, 0.10, 7, 65),
('coconut', 'Coconut', 'Кокос', 'tropical', 'rare', 360, 1, 2, 0.10, 8, 80),
('blue_agave', 'Blue Agave', 'Голубая агава', 'rare', 'rare', 720, 1, 1, 0.05, 10, 150),
('dragon_fruit', 'Dragon Fruit', 'Драконий фрукт', 'rare', 'rare', 720, 1, 1, 0.05, 12, 175);

-- Insert starter ingredients (spirits/mixers that can't be grown)
INSERT INTO ingredients_catalog (id, display_name, display_name_ru, category, rarity, buy_price, sell_price, can_be_grown, can_be_purchased) VALUES
('white_rum', 'White Rum', 'Белый ром', 'spirit', 'common', 50, 25, FALSE, TRUE),
('dark_rum', 'Dark Rum', 'Тёмный ром', 'spirit', 'common', 55, 28, FALSE, TRUE),
('tequila', 'Tequila', 'Текила', 'spirit', 'common', 60, 30, FALSE, TRUE),
('mezcal', 'Mezcal', 'Мескаль', 'spirit', 'uncommon', 80, 40, FALSE, TRUE),
('vodka', 'Vodka', 'Водка', 'spirit', 'common', 50, 25, FALSE, TRUE),
('gin', 'Gin', 'Джин', 'spirit', 'common', 55, 28, FALSE, TRUE),
('bourbon', 'Bourbon', 'Бурбон', 'spirit', 'common', 60, 30, FALSE, TRUE),
('sugar', 'Sugar', 'Сахар', 'sweetener', 'common', 5, 2, FALSE, TRUE),
('honey', 'Honey', 'Мёд', 'sweetener', 'common', 15, 7, FALSE, TRUE),
('agave_syrup', 'Agave Syrup', 'Сироп агавы', 'sweetener', 'uncommon', 25, 12, FALSE, TRUE),
('soda', 'Soda Water', 'Содовая', 'mixer', 'common', 5, 2, FALSE, TRUE),
('tonic', 'Tonic Water', 'Тоник', 'mixer', 'common', 8, 4, FALSE, TRUE),
('cola', 'Cola', 'Кола', 'mixer', 'common', 8, 4, FALSE, TRUE),
('ginger_beer', 'Ginger Beer', 'Имбирное пиво', 'mixer', 'common', 12, 6, FALSE, TRUE),
('cointreau', 'Cointreau', 'Куантро', 'liqueur', 'uncommon', 70, 35, FALSE, TRUE),
('campari', 'Campari', 'Кампари', 'liqueur', 'uncommon', 65, 32, FALSE, TRUE);

-- Insert starter recipes
INSERT INTO recipes_catalog (id, display_name, display_name_ru, category, difficulty, rarity, ingredients, coins_reward, exp_reward, unlock_level, hint_level_1, hint_level_2, hint_level_3) VALUES
('mojito', 'Mojito', 'Мохито', 'classic_cocktail', 'easy', 'common', '[{"id":"white_rum","qty":1},{"id":"lime","qty":1},{"id":"mint","qty":1},{"id":"sugar","qty":1},{"id":"soda","qty":1}]', 25, 15, 1, 'A refreshing cocktail with citrus and herbs', 'Uses rum, lime, and mint', 'White rum + lime + mint + sugar + soda'),
('margarita', 'Margarita', 'Маргарита', 'classic_cocktail', 'easy', 'common', '[{"id":"tequila","qty":1},{"id":"cointreau","qty":1},{"id":"lime","qty":1}]', 30, 20, 1, 'A classic Mexican cocktail', 'Uses tequila and citrus', 'Tequila + Cointreau + lime'),
('daiquiri', 'Daiquiri', 'Дайкири', 'classic_cocktail', 'easy', 'common', '[{"id":"white_rum","qty":1},{"id":"lime","qty":1},{"id":"sugar","qty":1}]', 20, 12, 1, 'Simple rum-based sour', 'Rum with citrus', 'White rum + lime + sugar'),
('cuba_libre', 'Cuba Libre', 'Куба Либре', 'classic_cocktail', 'easy', 'common', '[{"id":"white_rum","qty":1},{"id":"cola","qty":1},{"id":"lime","qty":1}]', 15, 10, 1, 'Rum and cola classic', 'Uses rum and cola', 'Rum + cola + lime'),
('moscow_mule', 'Moscow Mule', 'Московский мул', 'classic_cocktail', 'easy', 'common', '[{"id":"vodka","qty":1},{"id":"ginger_beer","qty":1},{"id":"lime","qty":1}]', 20, 12, 1, 'Vodka with a ginger kick', 'Uses vodka and ginger', 'Vodka + ginger beer + lime');

COMMIT;
