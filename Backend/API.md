# WAVE Island API Documentation

REST API для игры WAVE Island.

Base URL: `http://localhost:3000/api` (development)

## Аутентификация

Большинство endpoints требуют JWT токен в заголовке:

```http
Authorization: Bearer <your_jwt_token>
```

Токен получается при регистрации или входе.

## Endpoints

### 🔐 Authentication

#### POST /api/auth/register

Регистрация нового игрока.

**Request:**
```json
{
  "username": "player123",
  "email": "player@example.com",
  "password": "securePassword123"
}
```

**Response (201):**
```json
{
  "success": true,
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "player": {
      "id": "uuid",
      "username": "player123",
      "email": "player@example.com",
      "level": 1,
      "coins": 100,
      "gems": 50
    }
  }
}
```

#### POST /api/auth/login

Вход в аккаунт.

**Request:**
```json
{
  "email": "player@example.com",
  "password": "securePassword123"
}
```

**Response (200):**
```json
{
  "success": true,
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "player": {
      "id": "uuid",
      "username": "player123",
      "level": 5,
      "coins": 1250,
      "gems": 120
    }
  }
}
```

---

### 👤 Player

#### GET /api/player/profile

Получить профиль игрока.

**Headers:** `Authorization: Bearer <token>`

**Response (200):**
```json
{
  "success": true,
  "data": {
    "id": "uuid",
    "username": "player123",
    "email": "player@example.com",
    "level": 5,
    "experience": 2450,
    "coins": 1250,
    "gems": 120,
    "energy": 85,
    "max_energy": 100,
    "created_at": "2024-01-15T10:30:00Z",
    "last_login": "2024-03-20T15:45:00Z"
  }
}
```

#### PUT /api/player/profile

Обновить профиль игрока.

**Headers:** `Authorization: Bearer <token>`

**Request:**
```json
{
  "username": "newUsername",
  "avatar_url": "https://cdn.wave-island.com/avatars/1.png"
}
```

**Response (200):**
```json
{
  "success": true,
  "message": "Profile updated successfully"
}
```

#### GET /api/player/inventory

Получить инвентарь игрока.

**Headers:** `Authorization: Bearer <token>`

**Response (200):**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "item_id": 1,
        "name": "Lime",
        "type": "ingredient",
        "rarity": "common",
        "quantity": 15
      },
      {
        "item_id": 2,
        "name": "Mint",
        "type": "ingredient",
        "rarity": "common",
        "quantity": 8
      }
    ],
    "total_items": 23
  }
}
```

---

### 🌿 Garden

#### GET /api/garden/plots

Получить все грядки игрока.

**Headers:** `Authorization: Bearer <token>`

**Response (200):**
```json
{
  "success": true,
  "data": {
    "plots": [
      {
        "id": 1,
        "plant_id": 3,
        "plant_name": "Mint",
        "state": "growing",
        "planted_at": "2024-03-20T12:00:00Z",
        "ready_at": "2024-03-20T12:15:00Z",
        "progress": 0.67
      },
      {
        "id": 2,
        "plant_id": null,
        "state": "empty"
      }
    ],
    "max_plots": 8
  }
}
```

#### POST /api/garden/plant

Посадить семя на грядку.

**Headers:** `Authorization: Bearer <token>`

**Request:**
```json
{
  "plot_id": 2,
  "plant_id": 5
}
```

**Response (200):**
```json
{
  "success": true,
  "message": "Planted successfully",
  "data": {
    "plot_id": 2,
    "plant_id": 5,
    "ready_at": "2024-03-20T14:00:00Z"
  }
}
```

#### POST /api/garden/harvest

Собрать урожай с грядки.

**Headers:** `Authorization: Bearer <token>`

**Request:**
```json
{
  "plot_id": 1
}
```

**Response (200):**
```json
{
  "success": true,
  "message": "Harvested successfully",
  "data": {
    "rewards": [
      {
        "item_id": 3,
        "name": "Mint",
        "quantity": 3
      }
    ],
    "bonus_seed": true,
    "experience": 10,
    "coins": 5
  }
}
```

---

### 🔮 Alchemy

#### POST /api/alchemy/craft

Создать рецепт из ингредиентов.

**Headers:** `Authorization: Bearer <token>`

**Request:**
```json
{
  "ingredients": [1, 3, 5, 12]
}
```

**Response (200):**
```json
{
  "success": true,
  "data": {
    "recipe_id": 7,
    "recipe_name": "Mojito",
    "is_new_discovery": false,
    "rewards": {
      "experience": 25,
      "coins": 50
    }
  }
}
```

**Response (404) - рецепт не найден:**
```json
{
  "success": false,
  "error": "Recipe not found",
  "hint": "Try adding citrus fruit"
}
```

#### GET /api/alchemy/recipes

Получить все открытые рецепты игрока.

**Headers:** `Authorization: Bearer <token>`

**Response (200):**
```json
{
  "success": true,
  "data": {
    "recipes": [
      {
        "id": 1,
        "name": "Mojito",
        "category": "classic_cocktail",
        "difficulty": "easy",
        "ingredients": [
          {"id": 1, "name": "White Rum"},
          {"id": 3, "name": "Lime"},
          {"id": 5, "name": "Mint"}
        ],
        "times_crafted": 15,
        "discovered_at": "2024-01-20T10:00:00Z"
      }
    ],
    "total_discovered": 23,
    "total_recipes": 70
  }
}
```

---

### 💨 Spirits

#### GET /api/spirits/collection

Получить коллекцию духов игрока.

**Headers:** `Authorization: Bearer <token>`

**Response (200):**
```json
{
  "success": true,
  "data": {
    "spirits": [
      {
        "id": 1,
        "name": "Ember",
        "element": "fire",
        "rarity": "rare",
        "level": 3,
        "experience": 450,
        "bonuses": [
          {
            "type": "craft_speed",
            "value": 15
          }
        ],
        "is_active": true,
        "obtained_at": "2024-02-01T08:00:00Z"
      }
    ],
    "total_owned": 5,
    "total_spirits": 18,
    "active_spirit": 1
  }
}
```

#### POST /api/spirits/summon

Призвать нового духа (gacha).

**Headers:** `Authorization: Bearer <token>`

**Request:**
```json
{
  "summon_type": "single"
}
```

**Response (200):**
```json
{
  "success": true,
  "data": {
    "spirit": {
      "id": 8,
      "name": "Aqua",
      "element": "water",
      "rarity": "uncommon",
      "is_new": true
    },
    "pity_counter": 23,
    "cost": {
      "gems": 100
    }
  }
}
```

---

### 🏪 Shop

#### GET /api/shop/items

Получить товары магазина.

**Headers:** `Authorization: Bearer <token>`

**Query params:**
- `category` (optional): `ingredients`, `seeds`, `daily_specials`

**Response (200):**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": 1,
        "name": "Lime Seed",
        "type": "seed",
        "price": 10,
        "currency": "coins",
        "stock": "unlimited"
      },
      {
        "id": 15,
        "name": "Special Offer Bundle",
        "type": "daily_special",
        "price": 50,
        "currency": "gems",
        "discount": 30,
        "expires_at": "2024-03-21T00:00:00Z"
      }
    ],
    "daily_refresh": "2024-03-21T00:00:00Z"
  }
}
```

#### POST /api/shop/purchase

Купить товар в магазине.

**Headers:** `Authorization: Bearer <token>`

**Request:**
```json
{
  "item_id": 1,
  "quantity": 5
}
```

**Response (200):**
```json
{
  "success": true,
  "message": "Purchase successful",
  "data": {
    "item": {
      "id": 1,
      "name": "Lime Seed",
      "quantity": 5
    },
    "cost": {
      "coins": 50
    },
    "player_balance": {
      "coins": 1200,
      "gems": 120
    }
  }
}
```

---

### 🍽️ iiko Integration

#### POST /api/iiko/link-phone

Привязать номер телефона к аккаунту для iiko интеграции.

**Headers:** `Authorization: Bearer <token>`

**Request:**
```json
{
  "phone": "+7 900 123-45-67"
}
```

**Response (200):**
```json
{
  "success": true,
  "message": "Phone linked successfully",
  "data": {
    "imported_orders": 3,
    "rewards": {
      "coins": 150,
      "gems": 30
    }
  }
}
```

#### GET /api/iiko/orders

Получить историю заказов из iiko.

**Headers:** `Authorization: Bearer <token>`

**Response (200):**
```json
{
  "success": true,
  "data": {
    "orders": [
      {
        "id": "iiko-order-uuid",
        "order_number": "1234",
        "total": 2500.00,
        "created_at": "2024-03-15T18:30:00Z",
        "verified": true,
        "rewards_claimed": true,
        "items": [
          {
            "name": "Mojito Classic",
            "quantity": 2,
            "matched_recipe_id": 1
          }
        ]
      }
    ],
    "total_orders": 5,
    "total_spent": 12500.00
  }
}
```

#### POST /api/iiko/webhook

Webhook для получения уведомлений от iiko (только для iiko сервера).

**Request:**
```json
{
  "eventType": "OrderClosed",
  "orderId": "uuid",
  "phone": "+79001234567",
  "total": 2500.00,
  "items": [...]
}
```

**Response (200):**
```json
{
  "success": true,
  "message": "Order processed"
}
```

---

## Коды ошибок

| Код | Описание |
|-----|----------|
| 200 | OK |
| 201 | Created |
| 400 | Bad Request — неверные параметры |
| 401 | Unauthorized — требуется аутентификация |
| 403 | Forbidden — нет доступа |
| 404 | Not Found — ресурс не найден |
| 409 | Conflict — конфликт данных |
| 422 | Unprocessable Entity — валидация не прошла |
| 429 | Too Many Requests — rate limit |
| 500 | Internal Server Error |

## Rate Limiting

- **По умолчанию:** 100 запросов в минуту на IP
- **Auth endpoints:** 5 попыток в 15 минут

При превышении лимита:
```json
{
  "success": false,
  "error": "Too many requests",
  "retry_after": 60
}
```

## Webhooks

### iiko Webhook

URL: `https://your-domain.com/api/iiko/webhook`

Отправляется при закрытии заказа в iiko.

## Changelog

### v1.0.0 (2024-03-20)
- Initial API release
- All core endpoints implemented
- iiko integration active
