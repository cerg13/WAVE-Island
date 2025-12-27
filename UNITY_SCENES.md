# Unity Scenes Structure

Документация по структуре сцен Unity для WAVE Island.

## 📁 Scenes Overview

### Required Scenes

| Scene Name | Path | Purpose | Status |
|------------|------|---------|--------|
| **Bootstrap** | `Assets/Scenes/Bootstrap.unity` | Game initialization | ⚠️ To Create |
| **MainMenu** | `Assets/Scenes/MainMenu.unity` | Main menu and navigation | ⚠️ To Create |
| **Game** | `Assets/Scenes/Game.unity` | Main gameplay (Garden, Alchemy, etc.) | ⚠️ To Create |

### Optional Scenes

| Scene Name | Path | Purpose | Status |
|------------|------|---------|--------|
| **LoadingScene** | `Assets/Scenes/Loading.unity` | Dedicated loading screen | ⚠️ Optional |
| **Tutorial** | `Assets/Scenes/Tutorial.unity` | First-time user onboarding | ⚠️ Optional |

---

## 🎬 Scene 1: Bootstrap

**Purpose:** Initialize core systems before any UI appears.

### Hierarchy

```
Bootstrap Scene
├── GameBootstrap (GameBootstrap.cs)
├── --- Managers (Created at runtime) ---
│   ├── GameManager (Persistent)
│   ├── SaveManager (Persistent)
│   ├── AudioManager (Persistent)
│   ├── LocalizationManager (Persistent)
│   ├── AnalyticsManager (Persistent)
│   ├── QualityManager (Persistent)
│   ├── ErrorHandler (Persistent)
│   ├── PoolManager (Persistent)
│   ├── PerformanceMonitor (Persistent)
│   └── NotificationManager (Persistent)
├── Loading Canvas
│   ├── Background Image
│   ├── Logo Image
│   ├── Progress Bar
│   └── Status Text ("Инициализация...")
└── EventSystem
```

### Components

**GameBootstrap GameObject:**
- `GameBootstrap.cs` script
- Settings:
  - Manager Prefabs: Assign all manager prefabs
  - Main Menu Scene: "MainMenu"
  - Game Scene: "Game"
  - Enable Debug Mode: false (production)

### Flow

1. Scene loads
2. `GameBootstrap` initializes core systems
3. Loading progress displayed (0% → 100%)
4. Automatically loads MainMenu scene
5. Bootstrap scene unloads (managers persist via DontDestroyOnLoad)

---

## 🏠 Scene 2: MainMenu

**Purpose:** Main navigation hub for all game features.

### Hierarchy

```
MainMenu Scene
├── Canvas
│   ├── Background
│   │   ├── Background Image
│   │   ├── Animated Elements (particles, etc.)
│   │   └── Logo
│   ├── Header Panel
│   │   ├── Profile Button
│   │   │   ├── Avatar Image
│   │   │   ├── Player Name
│   │   │   └── Level Badge
│   │   ├── Resources
│   │   │   ├── Coins (Icon + Text)
│   │   │   └── Gems (Icon + Text)
│   │   └── Settings Button
│   ├── Main Navigation
│   │   ├── Garden Button
│   │   ├── Alchemy Button
│   │   ├── Spirits Button
│   │   ├── Shop Button
│   │   ├── Achievements Button
│   │   ├── Leaderboard Button
│   │   ├── Quests Button
│   │   └── iiko Button
│   ├── Event Banner (optional)
│   │   ├── Background
│   │   ├── Event Title
│   │   ├── Event Timer
│   │   └── CTA Button
│   ├── Bottom Bar
│   │   ├── Daily Quest Progress
│   │   ├── Notification Badge
│   │   └── News Indicator
│   └── Popups (initially hidden)
│       ├── Profile Panel
│       ├── Daily Reward Panel
│       └── News Panel
├── EventSystem
└── MainMenuController (MainMenuController.cs)
```

### Components

**MainMenuController:**
- Script: `MainMenuController.cs`
- Assign all UI references
- Navigation buttons
- Profile elements

### Navigation Flow

```
MainMenu → Garden Button → Game Scene (Garden area)
MainMenu → Alchemy Button → Game Scene (Alchemy area)
MainMenu → Shop Button → Shop UI overlay
MainMenu → Spirits Button → Spirits UI overlay
MainMenu → Settings Button → Settings UI overlay
```

---

## 🎮 Scene 3: Game

**Purpose:** Main gameplay scene with multiple areas (Garden, Alchemy, Village, Temple).

### Hierarchy

```
Game Scene
├── Environment
│   ├── Sky
│   ├── Lighting
│   ├── Ground
│   └── Background Elements
├── Camera
│   ├── Main Camera
│   └── Camera Controller (pan/zoom)
├── --- Areas (Active based on currentArea) ---
│   ├── Garden Area
│   │   ├── GardenGrid (GardenGrid.cs)
│   │   ├── Plots (0-20 PlotVisual instances)
│   │   ├── Decorations
│   │   └── Garden UI Canvas
│   ├── Alchemy Area
│   │   ├── CraftingTable (CraftingTable.cs)
│   │   ├── Crafting Slots (5x CraftingSlot.cs)
│   │   ├── Ingredient Palette
│   │   └── Alchemy UI Canvas
│   ├── Village Area (optional)
│   │   ├── Buildings
│   │   ├── NPCs
│   │   └── Village UI
│   └── Temple Area (optional)
│       ├── Altar
│       ├── Transmutation Table
│       └── Temple UI
├── UI Canvas
│   ├── Top Bar
│   │   ├── Back to Menu Button
│   │   ├── Current Area Title
│   │   ├── Resources Display
│   │   └── Energy Bar
│   ├── Area Switcher
│   │   ├── Garden Tab
│   │   ├── Alchemy Tab
│   │   ├── Village Tab
│   │   └── Temple Tab
│   └── Bottom Panel (context-sensitive)
│       ├── Garden: Seed Selection
│       ├── Alchemy: Recipe Book
│       └── etc.
├── Managers
│   ├── GardenManager (GardenManager.cs)
│   ├── AlchemyManager (AlchemyManager.cs)
│   ├── SpiritManager (SpiritManager.cs)
│   └── ShopManager (ShopManager.cs)
└── EventSystem
```

### Area Switching

**SceneLoader handles area switching:**

```csharp
SceneLoader.Instance.NavigateToArea(SceneLoader.SceneArea.Garden);
SceneLoader.Instance.NavigateToArea(SceneLoader.SceneArea.Alchemy);
```

**Areas enable/disable GameObjects:**
- Only active area is visible
- Inactive areas disabled to save performance
- Smooth transitions between areas

---

## 🔄 Scene Flow Diagram

```
┌─────────────┐
│  Bootstrap  │ (Auto-loads on app start)
└──────┬──────┘
       │
       ▼
┌─────────────┐
│  MainMenu   │ ◄───┐ (Back button)
└──────┬──────┘     │
       │            │
       ▼            │
┌─────────────┐     │
│    Game     │─────┘
│             │
│ - Garden    │
│ - Alchemy   │
│ - Village   │
│ - Temple    │
└─────────────┘
```

---

## 🎨 UI Overlays (Not Separate Scenes)

These are shown as overlays within MainMenu or Game scenes:

- **Shop UI** (`ShopUIController.cs`)
- **Spirits UI** (`GachaUIController.cs`, `SpiritCollectionUI.cs`)
- **Achievements UI** (`AchievementUIController.cs`)
- **Leaderboard UI** (`LeaderboardUIController.cs`)
- **Quests UI** (`DailyQuestUIController.cs`)
- **Settings UI** (`SettingsUIController.cs`)
- **iiko UI** (`IikoUIController.cs`)
- **Tutorial UI** (`TutorialUIController.cs`)

**Accessed via UIManager:**

```csharp
UIManager.Instance.ShowShop();
UIManager.Instance.ShowSpirits();
UIManager.Instance.ShowAchievements();
// etc.
```

---

## ⚙️ Build Settings

### Scenes in Build

**File > Build Settings > Scenes In Build:**

```
✓ 0. Assets/Scenes/Bootstrap.unity
✓ 1. Assets/Scenes/MainMenu.unity
✓ 2. Assets/Scenes/Game.unity
```

**Order matters!** Bootstrap must be index 0.

---

## 📝 Scene Creation Checklist

### Bootstrap Scene

- [ ] Create `Bootstrap.unity` scene
- [ ] Add `GameBootstrap` GameObject
- [ ] Assign all manager prefabs
- [ ] Add Loading UI
- [ ] Add EventSystem
- [ ] Set as first scene in Build Settings

### MainMenu Scene

- [ ] Create `MainMenu.unity` scene
- [ ] Build UI hierarchy
- [ ] Add `MainMenuController` script
- [ ] Assign all button listeners
- [ ] Connect to localization
- [ ] Add background art/animations
- [ ] Test all navigation buttons

### Game Scene

- [ ] Create `Game.unity` scene
- [ ] Setup Camera and Environment
- [ ] Create Garden Area
  - [ ] GardenGrid with PlotVisuals
  - [ ] Garden UI
- [ ] Create Alchemy Area
  - [ ] CraftingTable with slots
  - [ ] Alchemy UI
- [ ] Add Area Switcher UI
- [ ] Implement area enable/disable logic
- [ ] Test area switching

---

## 🔧 Prefabs Needed

### Manager Prefabs

Create these prefabs and assign to GameBootstrap:

- `GameManager.prefab`
- `SaveManager.prefab`
- `AudioManager.prefab`
- `LocalizationManager.prefab`
- `AnalyticsManager.prefab`
- `QualityManager.prefab`
- `ErrorHandler.prefab`
- `PoolManager.prefab`
- `PerformanceMonitor.prefab`
- `NotificationManager.prefab`

**Location:** `Assets/Prefabs/Managers/`

### UI Prefabs

- `LoadingScreen.prefab`
- `PlotVisual.prefab`
- `CraftingSlot.prefab`
- `ShopItemUI.prefab`
- `SpiritCardUI.prefab`
- `AchievementItemUI.prefab`
- etc.

**Location:** `Assets/Prefabs/UI/`

---

## 🎯 Performance Optimization

### Scene Optimization Tips

1. **Use Object Pooling**
   - PlotVisuals (20 max)
   - Crafting slots (5)
   - UI elements (lists, grids)

2. **Area Culling**
   - Disable inactive areas completely
   - Use Occlusion Culling if needed

3. **LOD (Level of Detail)**
   - Distant decorative objects
   - Background elements

4. **Batching**
   - Static batching for environment
   - Dynamic batching for UI

5. **Texture Atlasing**
   - Combine UI sprites
   - Use Sprite Atlas

---

## 📱 Mobile Considerations

### Canvas Setup

**All canvases should use:**
- Render Mode: Screen Space - Overlay
- UI Scale Mode: Scale With Screen Size
- Reference Resolution: 1920 x 1080
- Match: 0.5 (balance width/height)

### Safe Area

Handle notches (iPhone X, etc.):
- Use SafeArea script on main Canvas
- Avoid content in unsafe zones

### Touch Input

- Button sizes: min 100x100 px
- Spacing: min 20px between buttons
- Swipe gestures for area switching

---

## 🐛 Common Issues

### Issue: "Scene not found"

**Solution:** Add scene to Build Settings

### Issue: "Manager already exists"

**Solution:** GameBootstrap creates managers once, they persist via DontDestroyOnLoad

### Issue: "UI not scaling"

**Solution:** Check Canvas Scaler settings

### Issue: "Area not switching"

**Solution:** Ensure SceneLoader.Instance exists and OnAreaChange listener is set

---

## 📚 References

- [Unity Manual: Scenes](https://docs.unity3d.com/Manual/CreatingScenes.html)
- [Unity Manual: Build Settings](https://docs.unity3d.com/Manual/BuildSettings.html)
- [Unity UI Best Practices](https://docs.unity3d.com/Manual/HOWTO-UIFitContentSize.html)

---

**Last Updated:** December 27, 2024

**Next Steps:**
1. Create scenes in Unity Editor
2. Build UI hierarchies
3. Test scene transitions
4. Optimize for mobile
