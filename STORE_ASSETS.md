# Store Assets & Metadata

Требования к ассетам для публикации в App Store и Google Play.

## 📱 App Icons

### iOS App Store

| Size | Purpose | Required |
|------|---------|----------|
| 1024x1024 | App Store listing | ✅ Yes |
| 180x180 | iPhone (@3x) | ✅ Yes |
| 120x120 | iPhone (@2x) | ✅ Yes |
| 167x167 | iPad Pro (@2x) | ✅ Yes |
| 152x152 | iPad (@2x) | ✅ Yes |
| 76x76 | iPad (@1x) | ✅ Yes |

**Requirements:**
- PNG format
- RGB color space (no alpha/transparency)
- Square (1:1 ratio)
- No rounded corners (iOS adds them)

### Android Google Play

| Size | Density | Required |
|------|---------|----------|
| 512x512 | High-res icon | ✅ Yes |
| 192x192 | xxxhdpi | ✅ Yes |
| 144x144 | xxhdpi | ✅ Yes |
| 96x96 | xhdpi | ✅ Yes |
| 72x72 | hdpi | ✅ Yes |
| 48x48 | mdpi | ✅ Yes |

**Requirements:**
- PNG format (32-bit)
- Square (1:1 ratio)
- Transparent background OK
- No rounded corners

---

## 📸 Screenshots

### iOS App Store

**iPhone 6.7" (required):**
- 1290 x 2796 pixels
- Portrait orientation
- 3-10 screenshots

**iPhone 6.5" (required):**
- 1242 x 2688 pixels
- Portrait orientation
- 3-10 screenshots

**iPad Pro 12.9" (required):**
- 2048 x 2732 pixels
- Portrait orientation
- 3-10 screenshots

### Android Google Play

**Phone screenshots (required):**
- Min: 320px
- Max: 3840px
- Aspect ratio: 16:9 to 2:1
- 2-8 screenshots

**Tablet screenshots (optional):**
- Min: 1024px
- Max: 3840px
- 2-8 screenshots

**Requirements:**
- PNG or JPEG
- RGB color space
- No alpha channel

---

## 🎬 Preview Videos

### iOS App Store (Optional)

- **Duration:** 15-30 seconds
- **Format:** .mov, .m4v, .mp4
- **Resolution:** Same as screenshot sizes
- **Max file size:** 500MB

### Google Play (Optional)

- **YouTube link**
- **Duration:** 30 seconds - 2 minutes
- **Aspect ratio:** 16:9

---

## 📝 Metadata & Descriptions

### App Store (iOS)

#### App Name
- **Max length:** 30 characters
- **Suggestion:** "WAVE Island"

#### Subtitle
- **Max length:** 30 characters
- **Suggestion:** "Tropical Farming & Alchemy"

#### Description
- **Max length:** 4000 characters
- Include:
  - What is the game
  - Key features
  - iiko integration highlight
  - Social features

**Example:**
```
🌴 Добро пожаловать на WAVE Island! 🌴

Создай свой тропический рай на мистическом острове! Выращивай экзотические растения, готовь авторские коктейли и блюда, собирай духов кальяна и получай награды за посещение реального ресторана WAVE.

✨ ОСОБЕННОСТИ:

🌿 САД
• Выращивай 50+ тропических растений
• Собирай редкие ингредиенты
• Офлайн-прогресс: растения растут даже когда ты не в игре!

🍹 АЛХИМИЯ
• 70+ рецептов коктейлей и блюд
• Экспериментируй с ингредиентами
• Открывай секретные рецепты

💨 ДУХИ КАЛЬЯНА
• Собери 18 уникальных духов
• Каждый дух даёт особые бонусы
• Gacha-система с гарантиями

🏪 ИНТЕГРАЦИЯ С WAVE
• Делай заказы в реальном ресторане
• Получай награды в игре мгновенно
• Эксклюзивные рецепты за посещения

🏆 СОРЕВНУЙСЯ
• Таблицы лидеров
• Ежедневные задания
• 32 достижения
• Сезонные события

📱 ОСОБЕННОСТИ:
• Бесплатная игра с опциональными покупками
• Офлайн режим доступен
• Облачное сохранение
• 8 языков

Скачай сейчас и начни своё приключение на WAVE Island! 🌺
```

#### Keywords
- **Max length:** 100 characters (comma-separated)
- **Suggestion:** "farming,crafting,cooking,cocktails,alchemy,casual,cozy,tulum,tropical,restaurant"

#### Promotional Text (Optional)
- **Max length:** 170 characters
- Updates regularly without new submission

#### What's New
- **Max length:** 4000 characters
- Version update notes

### Google Play (Android)

#### Title
- **Max length:** 50 characters
- **Suggestion:** "WAVE Island: Tropical Farming & Alchemy"

#### Short Description
- **Max length:** 80 characters
- **Suggestion:** "Grow plants, craft cocktails, collect spirits. Get rewards at WAVE restaurant!"

#### Full Description
- **Max length:** 4000 characters
- Similar to iOS but can use HTML formatting

#### What's New
- **Max length:** 500 characters

---

## 🎨 Feature Graphic (Google Play)

- **Size:** 1024 x 500 pixels
- **Format:** PNG or JPEG
- **Required:** Yes
- **Purpose:** Top of store listing

**Design suggestions:**
- Game logo centered
- Key art background
- Clear, readable text
- No borders

---

## 🎯 Content Rating

### iOS Age Rating

Select appropriate ratings for:
- Alcohol, Tobacco, or Drug Use: **Infrequent/Mild** (cocktail crafting)
- Gambling: **None**
- Violence: **None**
- Sexual Content: **None**
- Horror/Fear Themes: **None**
- Mature/Suggestive Themes: **None**

**Result:** Rated **12+** (due to alcohol references)

### Android Content Rating

Complete [IARC questionnaire](https://www.globalratings.com/)

Expected rating: **PEGI 12** or **ESRB Teen**

---

## 📊 Localization

### Priority Languages

1. **Russian (ru)** - Primary
2. **English (en)** - Required
3. **Spanish (es)**
4. **German (de)**
5. **French (fr)**

### What to Localize

- App name
- Description
- Screenshots (text overlays)
- Keywords
- What's new

---

## 🔗 Additional Assets

### Privacy Policy URL
- **Required:** Yes
- **URL:** https://wave-island.com/privacy
- Or: Host `PRIVACY_POLICY.md` on website

### Terms of Service URL
- **Optional but recommended**
- **URL:** https://wave-island.com/terms

### Support URL
- **Required:** Yes
- **URL:** https://wave-island.com/support
- **Email:** support@wave-island.com

### Marketing URL (Optional)
- **URL:** https://wave-island.com

---

## ✅ Pre-Submission Checklist

### Assets
- [ ] App icon (all sizes)
- [ ] Screenshots (all device sizes)
- [ ] Feature graphic (Android)
- [ ] Preview video (optional)

### Metadata
- [ ] App name/title
- [ ] Description (all languages)
- [ ] Keywords (iOS)
- [ ] What's new
- [ ] Category selected
- [ ] Content rating completed

### Links
- [ ] Privacy policy URL live
- [ ] Support URL/email active
- [ ] Terms of service (if applicable)

### Legal
- [ ] Export compliance (iOS)
- [ ] Content rights declaration
- [ ] Age rating appropriate
- [ ] In-app purchase descriptions

---

## 📦 Asset Preparation Tools

### Design Tools
- **Figma** - UI mockups and screenshots
- **Canva** - Quick graphics
- **Photoshop** - Professional editing
- **Sketch** - macOS design tool

### Icon Generators
- [App Icon Generator](https://www.appicon.co/)
- [MakeAppIcon](https://makeappicon.com/)

### Screenshot Tools
- **Figma** - Device frames
- [Screenshot.rocks](https://screenshot.rocks/) - Beautiful device mockups
- [Previewed](https://previewed.app/) - Device mockups

### Video Tools
- **OBS Studio** - Screen recording
- **iMovie** - Basic editing
- **Final Cut Pro** - Professional editing

---

## 📱 Mock Screenshots Ideas

1. **Main Garden View**
   - Show growing plants
   - UI overlay with resources
   - Vibrant tropical colors

2. **Crafting Table**
   - Ingredients in slots
   - Recipe discovery animation
   - Success celebration

3. **Spirit Collection**
   - Gallery of spirits
   - Highlighted rare spirit
   - Stats and bonuses

4. **iiko Integration**
   - Restaurant menu sync
   - Order rewards screen
   - Exclusive content unlock

5. **Social Features**
   - Leaderboard view
   - Achievements panel
   - Daily quests

---

## 🎯 Category Selection

### iOS App Store
- **Primary:** Games
- **Secondary:** Casual
- **Tertiary:** Simulation

### Google Play
- **Category:** Casual
- **Tags:** farming, crafting, cooking, tropical, relaxing

---

## 💰 Pricing & Availability

### Price
- **Free** with in-app purchases

### In-App Purchases
List all IAPs:
- Gem packs (5 tiers)
- Coin packs (3 tiers)
- Special bundles
- Premium pass (optional)

### Availability
- **Countries:** Worldwide (start with Russia, then expand)
- **Release type:** Phased release recommended

---

## 📅 Timeline

1. **Week 1:** Design all assets
2. **Week 2:** Create screenshots and video
3. **Week 3:** Write all metadata
4. **Week 4:** Translate to priority languages
5. **Week 5:** Submit for review

---

This document should be updated as Apple/Google requirements change.

**Last updated:** December 27, 2024
