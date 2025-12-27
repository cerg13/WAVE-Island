# Unity Editor Workflow Guide

Step-by-step guide to complete Unity setup for WAVE Island.

## 🎯 Quick Setup (30 minutes)

This guide will walk you through completing the Unity project setup using the automated tools we've created.

---

## ✅ Prerequisites

Before starting:

- [ ] Unity Hub installed
- [ ] Unity 2022.3.x LTS installed
- [ ] Git repository cloned
- [ ] Project opened in Unity Editor
- [ ] No compilation errors

---

## 🚀 Step-by-Step Setup

### Step 1: Open Project (2 minutes)

```bash
# Open Unity Hub
# Click "Open" or "Add"
# Navigate to WAVE-Island folder
# Select the folder
# Unity version: 2022.3.x LTS
# Wait for project to open and import
```

**Expected:**
- Project opens without errors
- Console may show warnings (OK for now)
- Scripts compile successfully

---

### Step 2: Create Scenes (5 minutes)

**Menu:** `Tools > WAVE Island > Create All Scenes`

This will create:
1. `Assets/Scenes/Bootstrap.unity`
2. `Assets/Scenes/MainMenu.unity`
3. `Assets/Scenes/Game.unity`

**What it does:**
- Creates scene files
- Adds basic hierarchy
- Creates UI canvases
- Adds manager GameObjects
- Saves scenes

**Verify:**
```
Project window > Assets > Scenes
✓ Bootstrap.unity exists
✓ MainMenu.unity exists
✓ Game.unity exists
```

**Alternative (Manual):**
If automated tool fails, see [Manual Scene Creation](#manual-scene-creation) below.

---

### Step 3: Create Prefabs (5 minutes)

**Menu:** `Tools > WAVE Island > Create Manager Prefabs`

This creates 10 manager prefabs in `Assets/Prefabs/Managers/`:
1. GameManager.prefab
2. SaveManager.prefab
3. AudioManager.prefab
4. LocalizationManager.prefab
5. AnalyticsManager.prefab
6. QualityManager.prefab
7. ErrorHandler.prefab
8. PoolManager.prefab
9. PerformanceMonitor.prefab
10. NotificationManager.prefab

**Verify:**
```
Project window > Assets > Prefabs > Managers
✓ All 10 prefabs created
✓ Each prefab has correct component
```

**Then:** `Tools > WAVE Island > Create UI Prefabs`

Creates:
- LoadingScreen.prefab

---

### Step 4: Configure Bootstrap Scene (5 minutes)

1. **Open Bootstrap scene:**
   ```
   Assets > Scenes > Bootstrap.unity
   Double-click to open
   ```

2. **Select GameBootstrap GameObject:**
   ```
   Hierarchy > GameBootstrap
   Click to select
   ```

3. **Assign Manager Prefabs in Inspector:**
   ```
   GameBootstrap (Script) component:

   Manager Prefabs section:
   - Game Manager Prefab: Drag GameManager.prefab here
   - Save Manager Prefab: Drag SaveManager.prefab here
   - Audio Manager Prefab: Drag AudioManager.prefab here
   - Localization Manager Prefab: Drag LocalizationManager.prefab
   - Analytics Manager Prefab: Drag AnalyticsManager.prefab
   - Quality Manager Prefab: Drag QualityManager.prefab
   - Error Handler Prefab: Drag ErrorHandler.prefab
   - Pool Manager Prefab: Drag PoolManager.prefab
   - Performance Monitor Prefab: Drag PerformanceMonitor.prefab
   - Notification Manager Prefab: Drag NotificationManager.prefab

   UI Prefabs section:
   - Loading Screen Prefab: Drag LoadingScreen.prefab

   Scene Names:
   - Main Menu Scene: "MainMenu"
   - Game Scene: "Game"
   ```

4. **Save scene:**
   ```
   Ctrl+S (Windows) or Cmd+S (Mac)
   ```

---

### Step 5: Configure Build Settings (3 minutes)

1. **Open Build Settings:**
   ```
   File > Build Settings
   ```

2. **Add Scenes:**
   ```
   Click "Add Open Scenes" (if Bootstrap is open)
   OR
   Drag scenes from Project window:
   - Assets/Scenes/Bootstrap.unity (Index 0 - MUST BE FIRST)
   - Assets/Scenes/MainMenu.unity (Index 1)
   - Assets/Scenes/Game.unity (Index 2)
   ```

3. **Verify Order:**
   ```
   ✓ Bootstrap at index 0
   ✓ MainMenu at index 1
   ✓ Game at index 2
   ```

---

### Step 6: Configure Player Settings (5 minutes)

**Menu:** `Edit > Project Settings > Player`

#### **Common Settings:**

```
Company Name: WAVE Island Team
Product Name: WAVE Island
Version: 1.0.0
```

#### **iOS Settings:**

```
Icon:
- Default Icon: (assign when ready)
- All sizes: (assign when ready)

Bundle Identifier: com.wave.island

Target minimum iOS Version: 13.0
Target Device: iPhone & iPad
Architecture: ARM64

Requires ARKit support: NO
Requires iOS 13.0+: YES

Scripting Backend: IL2CPP
```

#### **Android Settings:**

```
Icon:
- Default Icon: (assign when ready)
- All sizes: (assign when ready)

Package Name: com.wave.island
Version: 1.0.0
Bundle Version Code: 1

Minimum API Level: Android 8.0 'Oreo' (API level 26)
Target API Level: Android 13 (API level 33)

Scripting Backend: IL2CPP
Target Architectures: ✓ ARM64 (required for Play Store)

Install Location: Automatic
Internet Access: Require
Write Permission: External (SD Card)
```

---

### Step 7: Validate Project (2 minutes)

**Menu:** `Tools > WAVE Island > Validate Project`

This runs 300+ automated checks:
- ✓ Scenes exist and in build settings
- ✓ Prefabs exist and assigned
- ✓ Build settings correct
- ✓ Player settings configured
- ✓ Resources present
- ✓ Scripts compile

**Expected Output:**
```
[Scenes] ✓ Found Bootstrap.unity
[Scenes] ✓ Found MainMenu.unity
[Scenes] ✓ Found Game.unity
[Scenes] Build Settings: 3 scene(s) configured
[Prefabs] Found 10/10 manager prefabs
[Prefabs] ✓ LoadingScreen prefab found
[Build] ✓ Bundle Identifier correct
[Build] Version: 1.0.0
...
[Summary] ✅ Validation complete! No blocking issues found.
```

**If errors:**
- Fix each error listed
- Re-run validation
- Repeat until clean

---

### Step 8: Test in Play Mode (5 minutes)

1. **Open Bootstrap scene:**
   ```
   Assets > Scenes > Bootstrap.unity
   ```

2. **Enter Play Mode:**
   ```
   Click Play button (top center)
   Or press Ctrl+P (Windows) / Cmd+P (Mac)
   ```

3. **Expected Behavior:**
   ```
   1. Loading screen appears
   2. Progress bar fills (0% → 100%)
   3. "Инициализация ядра..." → "Финальная настройка..."
   4. Automatically loads MainMenu scene
   5. Main menu appears with navigation buttons
   ```

4. **Check Console:**
   ```
   Window > General > Console

   Should see:
   ✓ "GameBootstrap: Starting initialization..."
   ✓ Manager initialization logs
   ✓ "Scene loaded: MainMenu in X.XXs"

   No errors in red
   ```

5. **Exit Play Mode:**
   ```
   Click Play button again
   Or press Ctrl+P / Cmd+P
   ```

---

## 🎨 Adding Visual Assets (Later)

### Icons

**Create icon:**
- Size: 1024x1024 px
- Format: PNG (no transparency for iOS)
- Design: App icon with WAVE Island branding

**Assign:**
```
Edit > Project Settings > Player > Icon
- Default Icon: Select your icon
- iOS: Will auto-generate all sizes
- Android: Will auto-generate all sizes
```

### Splash Screen

```
Edit > Project Settings > Player > Splash Image

Background:
- Solid Color: Tropical blue/green
- Background Image: Optional logo

Logos:
- Unity Logo: Can show or hide
- Custom Logo: WAVE Island logo
```

### UI Sprites

Place in:
```
Assets/Resources/Sprites/UI/
- buttons.png
- icons.png
- backgrounds.png
```

Import settings:
- Texture Type: Sprite (2D and UI)
- Sprite Mode: Multiple (if sprite sheet)
- Pixels Per Unit: 100
- Filter Mode: Bilinear
- Compression: Normal Quality

---

## 🔧 Advanced Configuration

### Localization Setup

1. **Check localization files exist:**
   ```
   Assets/Resources/Localization/
   ✓ ru.json
   ✓ en.json
   (others optional)
   ```

2. **Test language switching:**
   ```
   Play Mode > Settings > Language
   Switch between Russian/English
   All text should update
   ```

### Audio Setup

1. **Create SoundLibrary ScriptableObject:**
   ```
   Assets > Create > WAVE Island > Sound Library
   Name: MainSoundLibrary
   ```

2. **Assign audio clips:**
   ```
   Inspector > Sound Library
   - Button Click: (assign audio clip)
   - Harvest: (assign audio clip)
   - Craft Success: (assign audio clip)
   etc.
   ```

3. **Assign to AudioManager prefab:**
   ```
   Assets/Prefabs/Managers/AudioManager.prefab
   AudioManager (Script):
   - Sound Library: Drag MainSoundLibrary
   ```

### Analytics Setup

If using Unity Analytics:
```
Window > General > Services
Create Unity Project ID
Enable Analytics
```

In AnalyticsManager.prefab:
```
Analytics Manager (Script):
- Analytics Enabled: true
- API URL: (your backend URL if custom)
```

---

## 📱 Build for Testing

### iOS Build

1. **Switch Platform:**
   ```
   File > Build Settings
   Platform: iOS
   Click "Switch Platform" (wait for reimport)
   ```

2. **Build:**
   ```
   Click "Build"
   Select output folder: Builds/iOS/
   Wait for build (5-15 minutes)
   ```

3. **Open in Xcode:**
   ```
   Open: Builds/iOS/Unity-iPhone.xcodeproj
   Configure signing
   Build and run on device
   ```

### Android Build

1. **Switch Platform:**
   ```
   File > Build Settings
   Platform: Android
   Click "Switch Platform"
   ```

2. **Configure Keystore:**
   ```
   Edit > Project Settings > Player > Android > Publishing Settings

   Create New Keystore (first time):
   - Keystore password: (secure password)
   - Alias: wave_island_key
   - Password: (secure password)

   IMPORTANT: Backup keystore file!
   ```

3. **Build AAB:**
   ```
   File > Build Settings
   ✓ Build App Bundle (Google Play)
   Click "Build"
   Select output: Builds/Android/wave-island.aab
   Wait for build
   ```

---

## 🐛 Troubleshooting

### "Scene not found" error

**Solution:**
```
File > Build Settings
Ensure all 3 scenes are listed
Bootstrap must be index 0
```

### "Missing prefab" warnings

**Solution:**
```
Open Bootstrap scene
Select GameBootstrap
Re-assign all prefabs in Inspector
Save scene
```

### Compilation errors

**Solution:**
```
Check Console for red errors
Fix script errors
Wait for recompilation
Assets > Refresh (if needed)
```

### "Singleton already exists" errors

**Solution:**
```
This is OK in Play Mode if you:
1. Start from MainMenu instead of Bootstrap
2. Have managers in scene already

Always start from Bootstrap scene for testing.
```

### Black screen on play

**Solution:**
```
Check Console for errors
Ensure Bootstrap scene has:
- GameBootstrap GameObject
- Loading Canvas
- EventSystem

Ensure GameBootstrap has prefabs assigned.
```

---

## 📋 Manual Scene Creation

If automated tools don't work, create scenes manually:

### Bootstrap Scene

1. **Create scene:**
   ```
   File > New Scene
   Empty scene
   ```

2. **Add GameBootstrap:**
   ```
   GameObject > Create Empty
   Name: GameBootstrap
   Add Component > GameBootstrap script
   ```

3. **Add Loading UI:**
   ```
   GameObject > UI > Canvas
   Name: LoadingCanvas

   Add children:
   - Background (Image)
   - Logo (Image)
   - ProgressBar (Slider)
   - StatusText (Text)
   ```

4. **Add EventSystem:**
   ```
   GameObject > UI > Event System
   (created automatically with Canvas)
   ```

5. **Save:**
   ```
   File > Save As
   Assets/Scenes/Bootstrap.unity
   ```

### MainMenu Scene

Follow similar process. See `UNITY_SCENES.md` for detailed hierarchy.

---

## ✅ Completion Checklist

After completing this workflow:

- [ ] All 3 scenes created
- [ ] All 10 manager prefabs created
- [ ] LoadingScreen prefab created
- [ ] Bootstrap configured with prefabs
- [ ] Build settings configured (3 scenes)
- [ ] Player settings configured (iOS/Android)
- [ ] Validation passes (no errors)
- [ ] Play mode test successful
- [ ] Test build created (iOS or Android)

**Next:** Create store assets (icons, screenshots)

---

## 🚀 Ready for Next Steps

Once Unity setup is complete:
1. ✅ Unity project configured
2. → Create store assets
3. → Device testing
4. → Beta launch
5. → Production release

---

**Questions?** Check troubleshooting or see detailed docs:
- `UNITY_SCENES.md` - Scene structure details
- `PREFABS_GUIDE.md` - Prefab configuration
- `QA_TESTING.md` - Testing procedures

**Time to complete:** ~30 minutes for experienced Unity user, ~1-2 hours for beginner

Good luck! 🌴
