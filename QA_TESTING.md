# QA & Testing Checklist

Comprehensive testing checklist for WAVE Island before release.

## 📋 Pre-Testing Setup

### Environment

- [ ] Unity Editor version: 2022.3+ LTS
- [ ] Target platforms configured: iOS 13+, Android 8.0+
- [ ] Build settings verified
- [ ] All scenes in build settings
- [ ] Test devices available

### Test Devices

**iOS:**
- [ ] iPhone SE (small screen)
- [ ] iPhone 12/13 (standard)
- [ ] iPhone 14 Pro Max (large + notch)
- [ ] iPad (tablet)

**Android:**
- [ ] Low-end device (2GB RAM)
- [ ] Mid-range device (4GB RAM)
- [ ] High-end device (8GB+ RAM)
- [ ] Tablet (optional)

---

## 🎮 Functional Testing

### 1. Authentication & Account

**Registration:**
- [ ] Can create new account with email
- [ ] Password requirements enforced (8+ chars)
- [ ] Email validation works
- [ ] Duplicate email rejected
- [ ] Success message shown
- [ ] Auto-login after registration

**Login:**
- [ ] Can login with correct credentials
- [ ] Error shown for wrong password
- [ ] Error shown for non-existent account
- [ ] "Forgot password" flow works (if implemented)
- [ ] Remember me works
- [ ] Session persists after app restart

**Account Management:**
- [ ] Can view profile
- [ ] Can edit username
- [ ] Can change password
- [ ] Can delete account
- [ ] Logout works correctly

---

### 2. Garden System

**Planting:**
- [ ] Can select seed from inventory
- [ ] Can plant on empty plot
- [ ] Cannot plant on occupied plot
- [ ] Seed consumed from inventory
- [ ] Visual feedback on planting
- [ ] Sound plays on plant

**Growing:**
- [ ] Plants grow in real-time
- [ ] Progress bar updates correctly
- [ ] Growth continues when app closed (offline progress)
- [ ] Growth state saved correctly
- [ ] Visual stages update (sprouting, growing, ready)
- [ ] Ready indicator shows when harvestable

**Harvesting:**
- [ ] Can harvest when ready
- [ ] Ingredients added to inventory
- [ ] Bonus seed chance works
- [ ] Experience and coins awarded
- [ ] Plot becomes empty after harvest
- [ ] Perfect harvest bonus (if harvested on time)

**Withering:**
- [ ] Plants wither if not harvested
- [ ] Reduced yield when withered
- [ ] Dead plants can be cleared
- [ ] Warning shown before death

**Watering/Care:**
- [ ] Can water plants manually
- [ ] Watering bonus applied
- [ ] Auto-water (if spirit active) works
- [ ] Fertilizer speeds up growth

**Garden Expansion:**
- [ ] Can unlock new plots
- [ ] Cost deducted correctly
- [ ] New plots appear
- [ ] Max plots enforced

---

### 3. Alchemy System

**Crafting:**
- [ ] Can drag ingredients to slots
- [ ] Can remove ingredients from slots
- [ ] Up to 5 slots usable
- [ ] Craft button enabled when recipe possible
- [ ] Correct recipe creates item
- [ ] Wrong combo shows failure
- [ ] Ingredients consumed on craft
- [ ] Rewards given (coins, exp)

**Recipe Discovery:**
- [ ] New recipes unlock on first craft
- [ ] Discovery animation plays
- [ ] Recipe added to book
- [ ] Achievement triggered (if applicable)

**Recipe Book:**
- [ ] Can view all discovered recipes
- [ ] Filter by category works
- [ ] Search works
- [ ] Recipe details shown
- [ ] Can craft from book
- [ ] Locked recipes show hints

**Hints System:**
- [ ] Hint level 1 shows category
- [ ] Hint level 2 shows ingredient type
- [ ] Hint level 3 shows full recipe
- [ ] Coins deducted for hints
- [ ] Ad view grants free hint (if ads enabled)

---

### 4. Spirits System

**Summoning (Gacha):**
- [ ] Single summon costs correct gems
- [ ] 10-pull costs correct gems (discount)
- [ ] Spirit awarded randomly
- [ ] Rarity probabilities correct
- [ ] Pity counter increments
- [ ] Guaranteed rare at pity (70/90)
- [ ] Guaranteed rare+ every 10 pulls
- [ ] Duplicate spirits upgrade level
- [ ] New spirit animation plays

**Collection:**
- [ ] Can view all owned spirits
- [ ] Filter by element works
- [ ] Filter by rarity works
- [ ] Spirit details shown
- [ ] Level progress shown
- [ ] Bonuses listed correctly

**Activation:**
- [ ] Can activate spirit
- [ ] Only one active at a time
- [ ] Bonuses apply immediately
- [ ] Visual indicator of active spirit
- [ ] Can deactivate/switch spirits

**Bonuses:**
- [ ] Craft speed bonus works
- [ ] Growth speed bonus works
- [ ] Coin multiplier works
- [ ] Experience multiplier works
- [ ] Auto-harvest works (if applicable)

---

### 5. Shop System

**Browsing:**
- [ ] All items load correctly
- [ ] Filter by category works
- [ ] Prices shown correctly
- [ ] Currency icons shown (coins/gems)
- [ ] Out of stock items disabled

**Purchasing:**
- [ ] Can buy with coins
- [ ] Can buy with gems
- [ ] Insufficient funds shows error
- [ ] Item added to inventory
- [ ] Currency deducted
- [ ] Transaction confirmation shown
- [ ] Purchase history updated

**Daily Specials:**
- [ ] Daily offers shown
- [ ] Discount percentage correct
- [ ] Timer shows remaining time
- [ ] Refreshes at midnight
- [ ] Limited quantity enforced

**In-App Purchases (IAP):**
- [ ] Gem packs shown
- [ ] Real currency prices shown
- [ ] Purchase flow works (iOS)
- [ ] Purchase flow works (Android)
- [ ] Gems credited after purchase
- [ ] Receipt validated
- [ ] Restore purchases works

---

### 6. Achievements System

**Unlocking:**
- [ ] Achievements track correctly
- [ ] Progress updates in real-time
- [ ] Unlock notification shows
- [ ] Sound/animation plays
- [ ] Rewards granted automatically

**Viewing:**
- [ ] All achievements listed
- [ ] Filter by category works
- [ ] Locked achievements shown with requirements
- [ ] Progress bar shown for in-progress
- [ ] Claimed rewards marked

**Categories:**
- [ ] Garden achievements work
- [ ] Crafting achievements work
- [ ] Discovery achievements work
- [ ] Progress achievements work
- [ ] Spirits achievements work
- [ ] Economy achievements work
- [ ] iiko achievements work
- [ ] Daily achievements work
- [ ] Secret achievements work

---

### 7. Leaderboard System

**Loading:**
- [ ] Global leaderboard loads
- [ ] Friend leaderboard loads (if friends feature)
- [ ] Different board types work (level, crafts, etc.)
- [ ] Loading indicator shown
- [ ] Error handled if network down

**Display:**
- [ ] Top players shown with ranks
- [ ] Player stats shown
- [ ] Current player highlighted
- [ ] Scroll to current player works
- [ ] Rank icons shown (gold, silver, bronze)

**Updates:**
- [ ] Score updates when player progresses
- [ ] Leaderboard refreshes periodically
- [ ] Manual refresh works

---

### 8. Daily Quests

**Generation:**
- [ ] 5 quests generated daily
- [ ] Mix of difficulties (Easy/Medium/Hard)
- [ ] Quests match templates
- [ ] Reset at midnight

**Completion:**
- [ ] Progress tracks correctly
- [ ] Completed quests marked
- [ ] Rewards granted
- [ ] Completion bonus (all 5) granted

**UI:**
- [ ] Quest list shown
- [ ] Progress bars update
- [ ] Timer shows time to reset
- [ ] Claim button works

---

### 9. iiko Integration

**Phone Linking:**
- [ ] Can enter phone number
- [ ] Validation works (format)
- [ ] Link successful
- [ ] Existing orders imported
- [ ] Rewards granted for past orders

**Order Sync:**
- [ ] Webhook receives orders
- [ ] Order data parsed correctly
- [ ] Items matched to recipes
- [ ] Rewards calculated correctly
- [ ] Notification sent

**Milestones:**
- [ ] First order milestone unlocked
- [ ] 5 orders milestone unlocked
- [ ] 10 orders milestone unlocked
- [ ] 25 orders milestone unlocked
- [ ] Exclusive spirits granted

**Manual Verification:**
- [ ] Can submit order manually
- [ ] Order number validated
- [ ] Receipt upload works (if implemented)
- [ ] Verification pending status shown
- [ ] Approved orders grant rewards

---

### 10. Settings

**Audio:**
- [ ] Master volume slider works
- [ ] Music volume slider works
- [ ] SFX volume slider works
- [ ] Mute button works
- [ ] Settings persist after restart

**Graphics:**
- [ ] Quality presets work (Low/Med/High)
- [ ] VSync toggle works
- [ ] FPS limit works
- [ ] Particles toggle works
- [ ] Shadows toggle works

**Gameplay:**
- [ ] Language selection works
- [ ] Vibration toggle works
- [ ] Auto-save toggle works
- [ ] Tutorial tips toggle works
- [ ] Camera sensitivity slider works

**Notifications:**
- [ ] Harvest ready toggle works
- [ ] Daily reminder toggle works
- [ ] Energy refill toggle works
- [ ] Shop update toggle works
- [ ] Time picker works (daily reminder)

**Account:**
- [ ] Link account works
- [ ] Unlink account works
- [ ] Logout works
- [ ] Delete account works (with confirmation)

---

### 11. Tutorial

**First Launch:**
- [ ] Tutorial starts automatically
- [ ] Can skip tutorial
- [ ] Steps advance correctly
- [ ] Highlights appear on correct elements
- [ ] Touch blocking works
- [ ] Completion reward granted

**Steps:**
- [ ] Welcome message shown
- [ ] Plant first seed guided
- [ ] Wait for growth explained
- [ ] Harvest guided
- [ ] First craft guided
- [ ] Recipe discovery shown
- [ ] Shop introduced
- [ ] Daily quests shown
- [ ] Completion congratulated

---

### 12. Events System

**Active Events:**
- [ ] Current event shown
- [ ] Event banner displays
- [ ] Event timer counts down
- [ ] Event ended hides banner

**Event Missions:**
- [ ] Event quests shown
- [ ] Progress tracks correctly
- [ ] Event points awarded
- [ ] Point milestones shown

**Rewards:**
- [ ] Tier 1 rewards claimable
- [ ] Tier 2 rewards claimable
- [ ] Tier 3 rewards claimable
- [ ] Exclusive rewards granted
- [ ] Claimed rewards marked

---

## 🔧 Technical Testing

### Performance

**FPS:**
- [ ] 60 FPS on high-end devices
- [ ] 30+ FPS on low-end devices
- [ ] No frame drops during transitions
- [ ] Smooth animations

**Memory:**
- [ ] No memory leaks
- [ ] Memory usage under 500MB (mobile)
- [ ] No crashes after extended play (2+ hours)
- [ ] Handles low memory warnings

**Loading:**
- [ ] Bootstrap loads under 3 seconds
- [ ] Scene transitions under 1 second
- [ ] Asset loading doesn't block UI
- [ ] Loading screens show progress

**Network:**
- [ ] Handles offline mode gracefully
- [ ] Retry logic works
- [ ] Timeout errors shown
- [ ] Queue works when offline
- [ ] Sync when back online

---

### Save System

**Saving:**
- [ ] Auto-save every 60 seconds
- [ ] Save on app pause
- [ ] Save on app quit
- [ ] Manual save works

**Loading:**
- [ ] Load on app start
- [ ] All data restored correctly
- [ ] Handles corrupted save (backup)
- [ ] Cloud sync works

**Data Integrity:**
- [ ] Checksum validation works
- [ ] Encryption works
- [ ] No data loss on crash
- [ ] Backups created (up to 3)

---

### UI/UX

**Responsiveness:**
- [ ] All buttons respond immediately
- [ ] No input lag
- [ ] Scroll smooth
- [ ] Gestures work (pinch, swipe)

**Scaling:**
- [ ] UI scales on all resolutions
- [ ] Text readable on small screens
- [ ] Buttons tap-able on all screens
- [ ] Safe area respected (notch devices)

**Localization:**
- [ ] All text translates
- [ ] No missing keys
- [ ] Formatting correct (dates, numbers)
- [ ] RTL languages work (if supported)

**Accessibility:**
- [ ] Font size adjustable
- [ ] Color blind mode (if implemented)
- [ ] Screen reader compatible (basic)

---

## 🐛 Edge Cases & Error Handling

### Connection Issues

- [ ] Offline mode works
- [ ] Network error shown
- [ ] Reconnect automatic
- [ ] No data loss on disconnect

### Low Resources

- [ ] Low storage warning
- [ ] Low memory handling
- [ ] Low battery mode works

### Invalid Input

- [ ] Empty fields rejected
- [ ] Special characters handled
- [ ] SQL injection prevented
- [ ] XSS prevented

### Race Conditions

- [ ] Double-click prevented
- [ ] Concurrent requests handled
- [ ] State conflicts resolved

---

## 📱 Platform-Specific Testing

### iOS

**Functionality:**
- [ ] Runs on iOS 13+
- [ ] Runs on iPhone SE (small)
- [ ] Runs on iPhone 14 Pro (notch)
- [ ] Runs on iPad
- [ ] App Store build works

**Features:**
- [ ] Notifications work
- [ ] In-app purchases work
- [ ] Game Center (if integrated)
- [ ] iCloud sync (if integrated)
- [ ] Face ID / Touch ID (if used)

**Compliance:**
- [ ] Privacy labels correct
- [ ] App Tracking Transparency shown
- [ ] Age rating appropriate (12+)

### Android

**Functionality:**
- [ ] Runs on Android 8.0+
- [ ] Runs on low-end (2GB RAM)
- [ ] Runs on high-end
- [ ] Different screen sizes
- [ ] Play Store build works

**Features:**
- [ ] Notifications work
- [ ] In-app purchases work
- [ ] Google Play Games (if integrated)
- [ ] Back button works correctly

**Compliance:**
- [ ] Privacy policy linked
- [ ] Permissions justified
- [ ] Age rating appropriate (Teen)

---

## 🔒 Security Testing

### Authentication

- [ ] Passwords encrypted
- [ ] JWT tokens secure
- [ ] Session expiry works
- [ ] No credentials in logs

### Data Protection

- [ ] Local save encrypted
- [ ] Network traffic HTTPS
- [ ] API keys not exposed
- [ ] User data protected

### Anti-Cheat

- [ ] Time manipulation detected
- [ ] Hacked clients rejected
- [ ] Suspicious activity logged

---

## 🎯 Regression Testing

After each build:

- [ ] Critical path works (Register → Play → Craft → Save)
- [ ] No new crashes
- [ ] Performance unchanged or improved
- [ ] All P0/P1 bugs fixed

---

## ✅ Release Criteria

### Must Have (Blockers)

- [ ] No crash on launch
- [ ] Can create account
- [ ] Can save progress
- [ ] Garden works
- [ ] Alchemy works
- [ ] Shop works
- [ ] FPS > 30 on low-end
- [ ] All legal docs in place

### Should Have

- [ ] Tutorial works
- [ ] Achievements work
- [ ] Leaderboard works
- [ ] Daily quests work
- [ ] iiko integration works
- [ ] All translations complete

### Nice to Have

- [ ] Events work
- [ ] Cloud save works
- [ ] All polish complete

---

## 📊 Test Metrics

Track these during testing:

| Metric | Target | Actual |
|--------|--------|--------|
| Crash-free rate | > 99% | |
| Average FPS | > 55 | |
| Load time (cold start) | < 3s | |
| Scene transition | < 1s | |
| Memory usage | < 500MB | |
| Battery drain | < 10%/hour | |
| API success rate | > 99% | |

---

## 🐞 Bug Reporting Template

When logging bugs:

```
Title: [Component] Brief description

Steps to Reproduce:
1.
2.
3.

Expected Result:


Actual Result:


Device: iPhone 12, iOS 16.5
Build: v1.0.0 (123)
Severity: Critical/High/Medium/Low
Frequency: Always/Sometimes/Rare

Screenshots/Video:

Logs:
```

---

## 📅 Testing Timeline

**Week 1: Functional Testing**
- Day 1-2: Core systems
- Day 3-4: Features
- Day 5: UI/UX

**Week 2: Platform Testing**
- Day 1-2: iOS devices
- Day 3-4: Android devices
- Day 5: Edge cases

**Week 3: Polish & Regression**
- Day 1-3: Bug fixes
- Day 4-5: Final regression

---

## ✨ Pre-Release Checklist

- [ ] All critical bugs fixed
- [ ] Performance meets targets
- [ ] Privacy policy live
- [ ] Terms of service live
- [ ] Support email active
- [ ] Store assets uploaded
- [ ] Build signed and archived
- [ ] Beta testers invited
- [ ] Analytics configured
- [ ] Crash reporting enabled

---

**Last Updated:** December 27, 2024

**Testers:** _[Add tester names]_
**Build Version:** _[Current build]_
**Test Start Date:** _[Date]_
**Test End Date:** _[Date]_
