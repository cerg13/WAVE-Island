# Contributing to WAVE Island

Спасибо за интерес к проекту WAVE Island! Мы рады любому вкладу.

## Как внести вклад

### Сообщение о багах

Если вы нашли баг, пожалуйста:

1. Проверьте, не создан ли уже issue для этого бага
2. Создайте новый issue с подробным описанием:
   - Шаги для воспроизведения
   - Ожидаемое поведение
   - Фактическое поведение
   - Скриншоты (если применимо)
   - Версия Unity / Node.js
   - Платформа (iOS/Android/Editor)

### Предложения новых фич

1. Создайте issue с тегом `feature request`
2. Опишите:
   - Проблему, которую решает фича
   - Предлагаемое решение
   - Альтернативы, которые вы рассмотрели

### Pull Requests

1. Fork репозитория
2. Создайте feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit изменений (`git commit -m 'Add some AmazingFeature'`)
4. Push в branch (`git push origin feature/AmazingFeature`)
5. Откройте Pull Request

#### Требования к PR:

- Код должен следовать style guide проекта
- Добавьте тесты для новой функциональности
- Обновите документацию при необходимости
- Убедитесь, что все тесты проходят
- Опишите изменения в PR description

## Style Guide

### C# (Unity)

Следуем [Microsoft C# Coding Conventions](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)

```csharp
// Классы: PascalCase
public class PlayerController : MonoBehaviour
{
    // Поля: camelCase с _
    private int _playerHealth;

    // Properties: PascalCase
    public int MaxHealth { get; set; }

    // Methods: PascalCase
    public void TakeDamage(int damage)
    {
        // ...
    }
}
```

### JavaScript (Backend)

Следуем [Airbnb JavaScript Style Guide](https://github.com/airbnb/javascript)

```javascript
// Константы: UPPER_SNAKE_CASE
const MAX_RETRIES = 3;

// Функции: camelCase
function getUserData(userId) {
  // ...
}

// Классы: PascalCase
class UserService {
  constructor() {
    // ...
  }
}
```

### SQL

```sql
-- Таблицы: snake_case
CREATE TABLE player_inventory (
    -- Колонки: snake_case
    player_id UUID,
    item_id INTEGER
);
```

## Процесс разработки

### Ветки

- `main` — production-ready код
- `develop` — основная ветка разработки
- `feature/*` — новые фичи
- `bugfix/*` — исправления багов
- `hotfix/*` — срочные исправления

### Коммиты

Используем [Conventional Commits](https://www.conventionalcommits.org/):

```
feat: добавлена система достижений
fix: исправлен баг с сохранением сада
docs: обновлен README
style: форматирование кода
refactor: рефакторинг GardenManager
test: добавлены тесты для AlchemySystem
chore: обновлены зависимости
```

## Тестирование

### Unity

```bash
# Запустить тесты в Unity Editor
Window > General > Test Runner
```

### Backend

```bash
cd Backend
npm test
```

## Вопросы?

Не стесняйтесь задавать вопросы в:
- GitHub Discussions
- Discord сервере
- Email: dev@wave-island.com

Спасибо за ваш вклад! 🌴
