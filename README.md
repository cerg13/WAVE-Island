# 🌴 WAVE Island

[![Unity](https://img.shields.io/badge/Unity-2022.3%2B-black?logo=unity)](https://unity.com/)
[![Node.js](https://img.shields.io/badge/Node.js-18%2B-green?logo=node.js)](https://nodejs.org/)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-15%2B-blue?logo=postgresql)](https://www.postgresql.org/)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

**WAVE Island** — мобильная игра в жанре Cozy Farm Sim + Alchemy Crafting с интеграцией реального ресторана через iiko API.

> Выращивай тропические ингредиенты, создавай коктейли и блюда, собирай духов кальяна, делай реальные заказы в ресторане WAVE и получай бонусы в игре!

## 🎮 Описание проекта

### Концепция

WAVE Island — это уютная ферма-симулятор на мистическом тропическом острове в стиле Tulum. Игроки выращивают ингредиенты, создают коктейли и блюда по реальному меню ресторана, собирают коллекцию духов кальяна и получают награды за заказы в реальном заведении.

### Основной игровой цикл

```
ВЫРАЩИВАЙ → СОБИРАЙ → КОМБИНИРУЙ → ОТКРЫВАЙ РЕЦЕПТЫ → ПОЛУЧАЙ БОНУСЫ
     ↑                                                        ↓
     └────────────── ЗАКАЗЫВАЙ В WAVE ─────────────────────────┘
```

### Ключевые особенности

- 🌿 **Система сада** — Выращивание 50+ тропических растений в реальном времени
- 🍹 **70 рецептов коктейлей** — От классических до signature WAVE
- 🍽️ **48 блюд** — Полное меню ресторана
- 💨 **18 духов кальяна** — Коллекционные помощники с уникальными способностями
- 🎯 **32 достижения** — Прогрессия и награды
- 🏆 **Таблицы лидеров** — Соревнование с друзьями
- 📱 **iiko интеграция** — Реальные заказы = игровой прогресс
- 🌍 **8 языков** — Русский, English, Español и другие

## 📋 Содержание

- [Требования](#требования)
- [Установка](#установка)
- [Структура проекта](#структура-проекта)
- [Backend API](#backend-api)
- [База данных](#база-данных)
- [Разработка](#разработка)
- [Деплой](#деплой)
- [Документация](#документация)

## 🔧 Требования

### Unity Client

- **Unity:** 2022.3 LTS или выше
- **Платформы:** iOS 13+, Android 8.0+ (API 26+)
- **Packages:**
  - TextMeshPro
  - Unity UI
  - Unity Notifications (iOS/Android)
  - Newtonsoft JSON

### Backend

- **Node.js:** 18+ LTS
- **PostgreSQL:** 15+
- **Redis:** 7+ (опционально, для кэширования)
- **npm:** 9+

### Сервисы

- **iiko API:** Токен доступа к iiko Cloud API v2
- **Analytics:** (опционально) Unity Analytics или Firebase

## 📦 Установка

### 1. Клонирование репозитория

```bash
git clone https://github.com/your-org/WAVE-Island.git
cd WAVE-Island
```

### 2. Unity Client

1. Откройте проект в Unity Hub
2. Выберите Unity 2022.3 LTS
3. Подождите импорта packages
4. Откройте сцену `Assets/Scenes/Bootstrap.unity`

### 3. Backend Setup

```bash
cd Backend

# Установка зависимостей
npm install

# Настройка окружения
cp .env.example .env
# Отредактируйте .env файл с вашими параметрами

# Запуск в dev режиме
npm run dev
```

### 4. База данных

```bash
cd Database

# Создайте базу данных
createdb wave_island

# Примените схему
psql wave_island < schema.sql
```

### 5. Конфигурация

#### Backend `.env`

```env
# Server
PORT=3000
NODE_ENV=development

# Database
DB_HOST=localhost
DB_PORT=5432
DB_NAME=wave_island
DB_USER=postgres
DB_PASSWORD=your_password

# JWT
JWT_SECRET=your_secret_key_here
JWT_EXPIRES_IN=7d

# iiko API
IIKO_API_URL=https://api-ru.iiko.services
IIKO_API_LOGIN=your_login
IIKO_ORG_ID=your_organization_id
```

#### Unity Settings

В Unity Editor:
1. `Edit > Project Settings > Player`
2. Установите Bundle Identifier: `com.wave.island`
3. Установите версию приложения
4. Настройте иконки и splash screen

## 📁 Структура проекта

```
WAVE-Island/
├── Assets/
│   ├── Scenes/              # Unity сцены
│   ├── Scripts/             # C# скрипты
│   │   ├── Core/           # Основные системы
│   │   ├── Managers/       # Менеджеры игры
│   │   ├── Garden/         # Система сада
│   │   ├── Alchemy/        # Система крафта
│   │   ├── Spirits/        # Система духов
│   │   ├── UI/             # UI контроллеры
│   │   └── ...
│   ├── Resources/
│   │   ├── GameData/       # JSON данные
│   │   └── Localization/   # Переводы
│   └── Prefabs/            # Префабы
│
├── Backend/
│   ├── src/
│   │   ├── routes/         # API маршруты
│   │   ├── controllers/    # Контроллеры
│   │   ├── middleware/     # Middleware
│   │   └── services/       # Сервисы
│   └── package.json
│
├── Database/
│   └── schema.sql          # PostgreSQL схема
│
├── PROGRESS.md             # Прогресс разработки
└── WAVE_Island_Full_Implementation_Plan.md  # Полный план
```

## 🚀 Backend API

### Endpoints

#### Authentication
- `POST /api/auth/register` — Регистрация
- `POST /api/auth/login` — Вход
- `POST /api/auth/logout` — Выход

#### Player
- `GET /api/player/profile` — Профиль игрока
- `PUT /api/player/profile` — Обновление профиля
- `GET /api/player/inventory` — Инвентарь

#### Garden
- `GET /api/garden/plots` — Грядки игрока
- `POST /api/garden/plant` — Посадить семя
- `POST /api/garden/harvest` — Собрать урожай

#### Alchemy
- `POST /api/alchemy/craft` — Создать рецепт
- `GET /api/alchemy/recipes` — Открытые рецепты
- `POST /api/alchemy/discover` — Открыть рецепт

#### Spirits
- `GET /api/spirits/collection` — Коллекция духов
- `POST /api/spirits/summon` — Призыв духа
- `POST /api/spirits/activate` — Активировать духа

#### Shop
- `GET /api/shop/items` — Товары магазина
- `POST /api/shop/purchase` — Покупка

#### iiko Integration
- `POST /api/iiko/link-phone` — Привязка телефона
- `GET /api/iiko/orders` — История заказов
- `POST /api/iiko/webhook` — Webhook от iiko
- `POST /api/iiko/verify-order` — Верификация заказа

### Аутентификация

Все защищённые эндпоинты требуют JWT токен:

```http
Authorization: Bearer <your_jwt_token>
```

## 🗄️ База данных

### Основные таблицы

- `players` — Данные игроков
- `player_inventory` — Инвентарь
- `garden_plots` — Грядки
- `plants` — Типы растений
- `ingredients` — Ингредиенты
- `recipes` — Рецепты
- `player_recipes` — Открытые рецепты
- `spirits` — Духи
- `player_spirits` — Коллекция духов
- `achievements` — Достижения
- `player_achievements` — Достижения игроков
- `iiko_orders` — Заказы из iiko

### Миграции

```bash
# Применить схему
npm run migrate

# Загрузить начальные данные
npm run seed
```

## 💻 Разработка

### Unity

```bash
# Открыть проект в Unity
unity-hub --projectPath /path/to/WAVE-Island

# Запустить тесты
# Unity Editor > Window > General > Test Runner
```

### Backend

```bash
cd Backend

# Dev режим с hot reload
npm run dev

# Запуск тестов
npm test

# Линтинг
npm run lint
```

### Локализация

Файлы локализации находятся в `Assets/Resources/Localization/`:

- `ru.json` — Русский
- `en.json` — English
- `es.json` — Español
- и другие...

Добавление нового ключа:

```json
{
  "common.welcome": "Добро пожаловать на WAVE Island!",
  "garden.plant_seed": "Посадить семя"
}
```

## 🚢 Деплой

### Backend Production

```bash
# Установка зависимостей
npm ci --production

# Запуск с PM2
pm2 start src/index.js --name wave-island-api

# Или с Docker
docker build -t wave-island-backend .
docker run -p 3000:3000 wave-island-backend
```

### Unity Build

#### iOS

1. `File > Build Settings > iOS`
2. `Player Settings`:
   - Bundle Identifier: `com.wave.island`
   - Signing: Auto
3. `Build`
4. Откройте Xcode проект
5. Archive и Upload to App Store

#### Android

1. `File > Build Settings > Android`
2. `Player Settings`:
   - Package Name: `com.wave.island`
   - Keystore: создайте или используйте существующий
3. `Build > Build App Bundle (AAB)`
4. Upload в Google Play Console

## 📚 Документация

- [Полный план реализации](WAVE_Island_Full_Implementation_Plan.md)
- [Прогресс разработки](PROGRESS.md)
- [API Documentation](Backend/API.md) *(TODO)*
- [Game Design Document](docs/GDD.md) *(TODO)*

## 🎯 Roadmap

### ✅ Phase 1: MVP (Завершено)
- Базовый сад
- Система крафта
- 20 рецептов
- Tutorial

### ✅ Phase 2: Content (Завершено)
- 70 рецептов
- Система духов
- iiko интеграция
- Достижения

### ✅ Phase 3: Social (Завершено)
- Таблицы лидеров
- Ежедневные задания
- События
- Настройки

### ✅ Phase 4: Polish (Завершено)
- Оптимизация
- Локализация
- Аналитика
- Звук

### 🔄 Phase 5: Release (В процессе)
- UI Polish
- Build settings
- Store assets
- Beta testing
- Launch

## 🤝 Команда

- **Game Design:** [Your Name]
- **Development:** [Your Name]
- **Art:** Asset Store / AI
- **Backend:** Node.js + PostgreSQL
- **Integration:** iiko API v2

## 📄 Лицензия

MIT License - см. [LICENSE](LICENSE) файл

## 🔗 Ссылки

- [Unity Documentation](https://docs.unity3d.com/)
- [iiko API Documentation](https://api-ru.iiko.services/api/docs)
- [PostgreSQL Documentation](https://www.postgresql.org/docs/)
- [Node.js Documentation](https://nodejs.org/docs/)

## 📞 Поддержка

- **Email:** support@wave-island.com
- **Discord:** [WAVE Island Community]
- **Issues:** [GitHub Issues](https://github.com/your-org/WAVE-Island/issues)

---

Made with ❤️ for WAVE Restaurant
