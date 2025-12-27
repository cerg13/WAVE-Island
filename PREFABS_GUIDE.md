# Prefabs Setup Guide

Complete guide for creating and configuring all prefabs in WAVE Island.

## 🎯 Quick Start

### Automated Creation

Use Unity Editor menu tools:

```
Tools > WAVE Island > Create Manager Prefabs
Tools > WAVE Island > Create UI Prefabs
```

This will automatically create all required prefabs in `Assets/Prefabs/`.

---

## 📁 Prefab Structure

```
Assets/Prefabs/
├── Managers/              # Core system managers
│   ├── GameManager.prefab
│   ├── SaveManager.prefab
│   ├── AudioManager.prefab
│   ├── LocalizationManager.prefab
│   ├── AnalyticsManager.prefab
│   ├── QualityManager.prefab
│   ├── ErrorHandler.prefab
│   ├── PoolManager.prefab
│   ├── PerformanceMonitor.prefab
│   └── NotificationManager.prefab
├── UI/                    # UI components
│   ├── LoadingScreen.prefab
│   ├── NotificationToast.prefab
│   └── ConfirmDialog.prefab
├── Garden/                # Garden elements
│   ├── PlotVisual.prefab
│   └── HarvestEffect.prefab
├── Alchemy/              # Alchemy elements
│   ├── CraftingSlot.prefab
│   └── CraftingEffect.prefab
└── Effects/              # Visual effects
    ├── LevelUpEffect.prefab
    └── CoinEffect.prefab
```

---

## 🔧 Manager Prefabs

### 1. GameManager

**Components:**
- `GameManager` script

**Settings:**
- Starting Coins: 100
- Starting Gems: 50
- Starting Energy: 100
- Max Energy: 100
- Energy Regen Rate: 1 per 5 minutes

**Created by:** `Tools > WAVE Island > Create Manager Prefabs`

---

### 2. SaveManager

**Components:**
- `SaveManager` script

**Settings:**
- Auto Save Interval: 60 seconds
- Encryption: AES-256
- Use Checksum: true
- Max Backups: 3
- Cloud Sync Enabled: true
- Cloud Sync URL: `https://your-api.com/save`

**Key Features:**
- Encrypted local saves
- Automatic backups
- Cloud sync support
- Corruption recovery

---

### 3. AudioManager

**Hierarchy:**
```
AudioManager
├── MusicSource (AudioSource)
└── SFXSource (AudioSource)
```

**Components:**
- `AudioManager` script
- 2x `AudioSource` (children)

**Settings:**
- Master Volume: 1.0
- Music Volume: 0.7
- SFX Volume: 0.8
- UI Volume: 0.6
- Music Source: Loop = true
- SFX Source: PlayOnAwake = false

**Required Assets:**
- SoundLibrary ScriptableObject
- Audio clips in Resources/Audio/

---

### 4. LocalizationManager

**Components:**
- `LocalizationManager` script

**Settings:**
- Default Language: Russian (ru)
- Supported Languages: ru, en, uk, kk, uz, ky, az, tr
- Use System Language: true
- Fallback Language: en

**Required Assets:**
- JSON files in `Resources/Localization/`
- `ru.json`, `en.json`, etc.

---

### 5. AnalyticsManager

**Components:**
- `AnalyticsManager` script

**Settings:**
- Batch Events: true
- Batch Interval: 30 seconds
- Max Batch Size: 50
- Privacy Enabled: true
- Track Session: true
- Track Errors: true

**API Configuration:**
- Analytics URL: `https://your-api.com/analytics`
- API Key: (set in .env or Inspector)

---

### 6. QualityManager

**Components:**
- `QualityManager` script

**Settings:**
- Auto Detect Quality: true
- Target FPS: 60
- Adaptive Quality: true
- Low FPS Threshold: 30
- Battery Save Mode: false

**Quality Presets:**
- **Low:** FPS 30, No Shadows, AA 0x, Scale 0.75
- **Medium:** FPS 60, Simple Shadows, AA 2x, Scale 1.0
- **High:** FPS 60, All Shadows, AA 4x, Post-Processing

---

### 7. ErrorHandler

**Components:**
- `ErrorHandler` script

**Settings:**
- Log to File: true
- Max Log Entries: 100
- Crash Reporting: true
- Show User Dialogs: true
- Auto Restart on Critical: false

**Log Path:** `Application.persistentDataPath/logs/`

---

### 8. PoolManager

**Components:**
- `PoolManager` script (static utility)

**Note:** This is a static utility class, prefab created for consistency.

---

### 9. PerformanceMonitor

**Components:**
- `PerformanceMonitor` script

**Settings:**
- Show Debug UI: false (enable with F3)
- Track FPS: true
- Track Memory: true
- Warning Threshold: 30 FPS
- Memory Warning: 80% usage

**Debug UI:**
- FPS counter
- Memory usage
- Frame time graph
- Profiler markers

---

### 10. NotificationManager

**Components:**
- `NotificationManager` script

**Settings:**
- Enable Notifications: true
- Harvest Notifications: true
- Daily Reminder: true
- Daily Reminder Time: 18:00
- Free Summon Notification: true
- Shop Update Notification: true

**Platform:**
- iOS: Unity.Notifications.iOS
- Android: Unity.Notifications.Android

---

## 🎨 UI Prefabs

### LoadingScreen

**Hierarchy:**
```
LoadingScreen (Canvas)
├── Background (Image)
├── Logo (Image)
├── ProgressBar
│   ├── Background (Image)
│   └── Fill (Image)
├── StatusText (Text)
└── TipText (Text)
```

**Components:**
- `Canvas` (Overlay, SortingOrder 9999)
- `CanvasScaler` (ScaleWithScreenSize, 1920x1080)
- `LoadingScreen` script

**Settings:**
- Fade Duration: 0.5s
- Min Display Time: 1.0s
- Tips: Loaded from `Resources/Tips.json`

---

### NotificationToast

**Hierarchy:**
```
NotificationToast
├── Background (Image)
├── Icon (Image)
├── Title (Text)
└── Message (Text)
```

**Components:**
- `NotificationToast` script (TODO: Create)
- `CanvasGroup` (for fade)

**Settings:**
- Display Duration: 3 seconds
- Slide In Animation: 0.3s
- Fade Out: 0.5s

---

### ConfirmDialog

**Hierarchy:**
```
ConfirmDialog
├── Blocker (Image, black 50% alpha)
├── Panel
│   ├── Title (Text)
│   ├── Message (Text)
│   ├── ConfirmButton
│   └── CancelButton
```

**Components:**
- `ConfirmDialog` script (TODO: Create)

---

## 🌿 Garden Prefabs

### PlotVisual

**Hierarchy:**
```
PlotVisual
├── Ground (SpriteRenderer)
├── Plant (SpriteRenderer)
├── ProgressBar
│   ├── Background
│   └── Fill
└── ReadyIndicator (ParticleSystem)
```

**Components:**
- `PlotVisual` script

**Settings:**
- Plot Size: 1x1 units
- Sprites: Loaded from Resources/Sprites/Garden/
- States: Empty, Planted, Growing, Ready, Withering, Dead

**Animations:**
- Plant sprite changes per growth stage
- Ready indicator particles
- Harvest effect on collect

---

### HarvestEffect

**Components:**
- `ParticleSystem` (coins, sparkles)
- `AudioSource` (harvest sound)
- `HarvestEffect` script

**Settings:**
- Duration: 1.5 seconds
- Auto-destroy: true
- Particle Count: 10-20
- Colors: Green/Gold

---

## 🔮 Alchemy Prefabs

### CraftingSlot

**Hierarchy:**
```
CraftingSlot
├── Background (Image)
├── Icon (Image)
├── RemoveButton (Button)
└── Highlight (Image)
```

**Components:**
- `CraftingSlot` script
- `Image` (for drag-drop visual)
- `EventTrigger` (for drag events)

**Settings:**
- Slot Index: 0-4
- Accept Ingredients: true
- Highlight on Hover: true

---

### CraftingEffect

**Components:**
- `ParticleSystem` (mixing effect)
- `AudioSource` (mixing sound)

**Variants:**
- MixingEffect (during craft)
- SuccessEffect (on success)
- FailEffect (on fail)
- DiscoveryEffect (new recipe)

---

## ⚙️ Manual Prefab Creation

If you need to create prefabs manually:

### Step 1: Create GameObject

```
GameObject > Create Empty
```

### Step 2: Add Component

```
Add Component > Search for script name
Example: GameManager
```

### Step 3: Configure

Set all required fields in Inspector.

### Step 4: Save as Prefab

```
Drag GameObject from Hierarchy to Assets/Prefabs/Managers/
```

### Step 5: Delete from Scene

```
Delete the GameObject from Hierarchy (prefab is saved)
```

---

## 🔗 Assigning Prefabs

### GameBootstrap Assignment

Open `Bootstrap` scene and assign all manager prefabs to `GameBootstrap`:

1. Select `GameBootstrap` GameObject
2. In Inspector, find "Manager Prefabs" section
3. Drag each prefab from Project to corresponding slot:
   - Game Manager Prefab → GameManager.prefab
   - Audio Manager Prefab → AudioManager.prefab
   - Save Manager Prefab → SaveManager.prefab
   - ... (all 10 managers)

4. Assign UI prefabs:
   - Loading Screen Prefab → LoadingScreen.prefab

5. Set scene names:
   - Main Menu Scene: "MainMenu"
   - Game Scene: "Game"

### Verification

Use menu tool to verify:

```
Tools > WAVE Island > Validate Setup
```

This will check:
- [ ] All manager prefabs exist
- [ ] All prefabs have required components
- [ ] GameBootstrap has all references
- [ ] Scenes exist in build settings

---

## 📋 Prefab Checklist

### Manager Prefabs (10)

- [ ] GameManager.prefab
- [ ] SaveManager.prefab
- [ ] AudioManager.prefab
- [ ] LocalizationManager.prefab
- [ ] AnalyticsManager.prefab
- [ ] QualityManager.prefab
- [ ] ErrorHandler.prefab
- [ ] PoolManager.prefab
- [ ] PerformanceMonitor.prefab
- [ ] NotificationManager.prefab

### UI Prefabs (3+)

- [ ] LoadingScreen.prefab
- [ ] NotificationToast.prefab (optional)
- [ ] ConfirmDialog.prefab (optional)

### Garden Prefabs (2+)

- [ ] PlotVisual.prefab
- [ ] HarvestEffect.prefab

### Alchemy Prefabs (2+)

- [ ] CraftingSlot.prefab
- [ ] CraftingEffect.prefab (variants)

### All Prefabs Assigned

- [ ] GameBootstrap has all manager prefabs
- [ ] SceneLoader has LoadingScreen prefab
- [ ] GardenGrid has PlotVisual prefab
- [ ] CraftingTable has CraftingSlot prefab

---

## 🎯 Best Practices

### Naming Convention

```
[Category][Name].prefab

Examples:
Manager_Game.prefab
UI_LoadingScreen.prefab
Garden_Plot.prefab
Effect_LevelUp.prefab
```

### Organization

- Group by category in folders
- Use consistent naming
- Add prefab variants for different states

### Performance

- Keep prefab hierarchy shallow
- Use object pooling for frequently instantiated prefabs
- Minimize components per prefab

### Testing

After creating prefabs:
1. Test instantiation in PlayMode
2. Verify all references resolve
3. Check for missing components
4. Test DontDestroyOnLoad behavior (managers)

---

## 🐛 Troubleshooting

### "Missing Prefab" Error

**Solution:** Re-assign prefab in GameBootstrap Inspector

### "Component Not Found"

**Solution:** Ensure script exists and compiles without errors

### Prefab Override Issues

**Solution:** Apply overrides or revert to prefab

### DontDestroyOnLoad Duplicates

**Solution:** Use singleton pattern in manager scripts

---

## 📚 References

- [Unity Prefabs Documentation](https://docs.unity3d.com/Manual/Prefabs.html)
- [Unity Prefab Variants](https://docs.unity3d.com/Manual/PrefabVariants.html)
- [Object Pooling](https://unity.com/how-to/pool-objects-with-object-pooling)

---

**Last Updated:** December 27, 2024

**Next Steps:**
1. Run automated prefab creation
2. Assign prefabs to GameBootstrap
3. Test in PlayMode
4. Build and verify on device
