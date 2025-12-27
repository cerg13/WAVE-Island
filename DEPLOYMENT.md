# Deployment Guide

Complete guide for deploying WAVE Island to production.

## 📋 Pre-Deployment Checklist

### Code

- [ ] All features implemented
- [ ] All bugs fixed (P0/P1)
- [ ] Code reviewed
- [ ] Tests passing
- [ ] No debug code left
- [ ] API keys in environment variables (not hardcoded)

### Assets

- [ ] All sprites optimized
- [ ] Audio files compressed
- [ ] Unused assets removed
- [ ] Addressables built (if using)

### Configuration

- [ ] Build number incremented
- [ ] Version number updated
- [ ] Bundle identifier correct
- [ ] App icons set
- [ ] Splash screen set

---

## 🔧 Backend Deployment

### 1. Prepare Server

**DigitalOcean / AWS / Other:**

```bash
# SSH into server
ssh root@your-server-ip

# Update system
apt update && apt upgrade -y

# Install Node.js 18
curl -fsSL https://deb.nodesource.com/setup_18.x | bash -
apt install -y nodejs

# Install PostgreSQL 15
apt install -y postgresql postgresql-contrib

# Install Redis
apt install -y redis-server

# Install PM2 for process management
npm install -g pm2
```

### 2. Setup Database

```bash
# Login to PostgreSQL
sudo -u postgres psql

# Create database and user
CREATE DATABASE wave_island;
CREATE USER wave_user WITH ENCRYPTED PASSWORD 'your_secure_password';
GRANT ALL PRIVILEGES ON DATABASE wave_island TO wave_user;
\q

# Apply schema
psql -U wave_user -d wave_island < Database/schema.sql
```

### 3. Deploy Backend Code

```bash
# Create app directory
mkdir -p /var/www/wave-island-api
cd /var/www/wave-island-api

# Clone repository or upload code
git clone https://github.com/your-org/WAVE-Island.git .
cd Backend

# Install dependencies (production only)
npm ci --production

# Create .env file
nano .env
```

**`.env` Production:**

```env
NODE_ENV=production
PORT=3000

# Database
DB_HOST=localhost
DB_PORT=5432
DB_NAME=wave_island
DB_USER=wave_user
DB_PASSWORD=your_secure_password

# JWT
JWT_SECRET=your_very_long_random_secret_key_here
JWT_EXPIRES_IN=7d

# iiko API
IIKO_API_URL=https://api-ru.iiko.services
IIKO_API_LOGIN=your_iiko_login
IIKO_ORG_ID=your_organization_id

# Redis
REDIS_HOST=localhost
REDIS_PORT=6379
REDIS_PASSWORD=your_redis_password

# CORS
CORS_ORIGIN=https://wave-island.com

# Analytics (optional)
ANALYTICS_ENABLED=true
```

### 4. Start with PM2

```bash
# Start application
pm2 start src/index.js --name wave-island-api

# Save PM2 process list
pm2 save

# Auto-start on reboot
pm2 startup
# Follow the command it outputs

# View logs
pm2 logs wave-island-api

# Monitor
pm2 monit
```

### 5. Setup Nginx Reverse Proxy

```bash
# Install Nginx
apt install -y nginx

# Create config
nano /etc/nginx/sites-available/wave-island
```

**Nginx Config:**

```nginx
server {
    listen 80;
    server_name api.wave-island.com;

    location / {
        proxy_pass http://localhost:3000;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection 'upgrade';
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        proxy_cache_bypass $http_upgrade;
    }
}
```

```bash
# Enable site
ln -s /etc/nginx/sites-available/wave-island /etc/nginx/sites-enabled/

# Test config
nginx -t

# Restart Nginx
systemctl restart nginx
```

### 6. Setup SSL with Let's Encrypt

```bash
# Install Certbot
apt install -y certbot python3-certbot-nginx

# Get certificate
certbot --nginx -d api.wave-island.com

# Auto-renewal is set up automatically
# Test renewal
certbot renew --dry-run
```

### 7. Monitoring & Logging

```bash
# Setup log rotation
nano /etc/logrotate.d/wave-island
```

```
/var/log/wave-island/*.log {
    daily
    rotate 14
    compress
    delaycompress
    notifempty
    create 0640 www-data www-data
    sharedscripts
}
```

---

## 📱 Unity Client Deployment

### iOS Deployment

#### 1. Prepare Xcode Project

**In Unity:**

```
File > Build Settings
- Platform: iOS
- Switch Platform (if needed)

Player Settings:
- Company Name: WAVE Island Team
- Product Name: WAVE Island
- Bundle Identifier: com.wave.island
- Version: 1.0.0
- Build Number: 1

- Minimum iOS Version: 13.0
- Target Device: iPhone & iPad
- Architecture: ARM64

- Camera Usage Description: "To scan QR codes" (if used)
- Location Usage Description: "To find nearby WAVE restaurants" (if used)

Build
```

**Select iOS build folder, click Build.**

#### 2. Xcode Configuration

Open the generated Xcode project:

```bash
open Builds/iOS/Unity-iPhone.xcworkspace
```

**In Xcode:**

1. **Signing & Capabilities:**
   - Team: Select your Apple Developer account
   - Signing Certificate: Automatic
   - Provisioning Profile: Automatic

2. **Capabilities:**
   - Push Notifications: ON
   - In-App Purchase: ON
   - Game Center: ON (optional)

3. **Build Settings:**
   - Code Signing Identity: iPhone Distribution
   - Provisioning Profile: (auto-selected)

4. **Info.plist:**
   - Privacy - User Tracking Usage Description: "To provide personalized content"
   - Privacy - Photo Library Usage: "To save screenshots" (if used)

#### 3. TestFlight Beta

```
Product > Archive

When archive completes:
- Window > Organizer
- Select archive
- Distribute App
- App Store Connect
- Upload
- Automatic signing
- Upload

Wait for processing (~30 minutes)
```

**In App Store Connect:**

1. Go to TestFlight tab
2. Select build
3. Add beta testers (email or groups)
4. Submit for beta review
5. Wait for approval (~24 hours)

#### 4. Production Release

**In App Store Connect:**

1. **App Information:**
   - Name: WAVE Island
   - Subtitle: Tropical Farming & Alchemy
   - Category: Games > Casual
   - Age Rating: 12+

2. **Pricing:**
   - Free
   - In-App Purchases configured

3. **App Privacy:**
   - Complete privacy questionnaire
   - Link privacy policy: https://wave-island.com/privacy

4. **Version Information:**
   - Version: 1.0.0
   - Screenshots uploaded (all sizes)
   - Description written
   - Keywords: farming,crafting,cooking,cocktails,casual
   - Support URL: https://wave-island.com/support
   - Marketing URL: https://wave-island.com

5. **Build:**
   - Select uploaded build

6. **Submit for Review**
   - Review time: 1-3 days typically

---

### Android Deployment

#### 1. Build Configuration

**In Unity:**

```
File > Build Settings
- Platform: Android
- Switch Platform

Player Settings:
- Company Name: WAVE Island Team
- Product Name: WAVE Island
- Package Name: com.wave.island
- Version: 1.0.0
- Bundle Version Code: 1

- Minimum API Level: Android 8.0 (API 26)
- Target API Level: Android 13 (API 33)
- Scripting Backend: IL2CPP
- Target Architectures: ARM64

Other Settings:
- Install Location: Automatic
- Internet Access: Require
- Write Permission: External (SD Card)
```

#### 2. Keystore

**Create keystore (first time only):**

```
Unity > Publish Settings
- Create a new keystore
- Keystore password: [secure password]
- Alias: wave_island_key
- Alias password: [secure password]

IMPORTANT: Backup keystore file!
Store passwords securely!
```

**Or use existing:**

```
Keystore: Select your .keystore file
Password: Enter keystore password
Alias: wave_island_key
Password: Enter alias password
```

#### 3. Build AAB

```
Build Settings > Build App Bundle (AAB)
Select output folder
Build
```

File generated: `wave-island.aab`

#### 4. Google Play Console - Internal Testing

1. **Create App:**
   - Go to Google Play Console
   - Create Application
   - App name: WAVE Island
   - Default language: English
   - App/Game: Game
   - Free/Paid: Free

2. **App Content:**
   - Privacy Policy: https://wave-island.com/privacy
   - App Access: All functionality available
   - Ads: No (or Yes if using ads)
   - Content Rating: Complete questionnaire → PEGI 12 / ESRB Teen
   - Target Audience: 13+
   - Data Safety: Complete form (data collection, usage)

3. **Internal Testing:**
   - Testing > Internal Testing
   - Create Release
   - Upload AAB
   - Release name: v1.0.0
   - Release notes: "Initial internal test"
   - Add testers (emails)
   - Review and Rollout

4. **Testing:**
   - Testers receive email with link
   - Download and test
   - Collect feedback

#### 5. Production Release

1. **Store Listing:**
   - App name: WAVE Island
   - Short description: "Grow plants, craft cocktails, collect spirits!"
   - Full description: (500+ words, use STORE_ASSETS.md template)
   - Screenshots: Upload all required sizes
   - Feature graphic: 1024x500 PNG
   - Icon: 512x512 PNG
   - Category: Casual
   - Tags: farming, crafting, cooking, cocktails

2. **Production Release:**
   - Production > Create Release
   - Upload AAB (new build with incremented version)
   - Release notes
   - Countries: Select all or specific
   - Review and Rollout

3. **Submit for Review:**
   - Review time: Few hours to 2 days

---

## 🔄 Continuous Deployment

### GitHub Actions

Already configured in `.github/workflows/build.yml`

**Trigger on:**
- Push to `main` branch
- Manual workflow dispatch

**Jobs:**
1. Build Android AAB
2. Build iOS Xcode project
3. Run tests
4. Upload artifacts

**Secrets to configure:**

```
Settings > Secrets > Actions

Required:
- UNITY_LICENSE
- UNITY_EMAIL
- UNITY_PASSWORD
- ANDROID_KEYSTORE_BASE64 (keystore file as base64)
- ANDROID_KEYSTORE_PASSWORD
- ANDROID_KEY_ALIAS
- ANDROID_KEY_PASSWORD
```

### Automatic Versioning

Use git tags:

```bash
# Tag release
git tag -a v1.0.0 -m "Release version 1.0.0"
git push origin v1.0.0

# CI will detect tag and auto-increment build
```

---

## 🔥 Rollback Procedure

### Backend Rollback

```bash
# SSH to server
ssh root@your-server-ip

# Check PM2 logs for errors
pm2 logs wave-island-api --lines 100

# Revert to previous version
cd /var/www/wave-island-api/Backend
git log --oneline -n 5
git checkout [previous-commit-hash]
npm ci --production

# Restart
pm2 restart wave-island-api

# If database schema changed, rollback DB:
psql -U wave_user -d wave_island < backups/schema_backup_[date].sql
```

### Client Rollback

**iOS:**
- In App Store Connect, can't rollback published version
- Must submit new version
- Can remove from sale temporarily

**Android:**
- Google Play Console > Production
- Can rollback to previous version
- Or stop rollout and release new version

---

## 📊 Monitoring

### Backend Monitoring

```bash
# PM2 Monitoring
pm2 monit

# Logs
pm2 logs wave-island-api

# Status
pm2 status
```

**Setup external monitoring (optional):**
- [Sentry](https://sentry.io/) for error tracking
- [DataDog](https://www.datadoghq.com/) for APM
- [New Relic](https://newrelic.com/) for monitoring

### Mobile Analytics

- Unity Analytics
- Firebase Analytics (recommended)
- Custom analytics to your backend

### Crash Reporting

- Unity Cloud Diagnostics
- Firebase Crashlytics (recommended)
- Sentry

---

## 🔐 Security Checklist

- [ ] All API keys in environment variables
- [ ] Database credentials secure
- [ ] SSL certificates installed
- [ ] Keystore backed up (Android)
- [ ] Certificates backed up (iOS)
- [ ] No debug code in production
- [ ] Error messages don't expose internals
- [ ] Rate limiting enabled
- [ ] Input validation on all endpoints
- [ ] SQL injection prevented
- [ ] XSS prevented
- [ ] CORS configured correctly

---

## 📅 Release Timeline

### Week 1: Beta Preparation
- Day 1-2: Final bug fixes
- Day 3-4: Build iOS/Android beta
- Day 5: Upload to TestFlight/Internal Testing

### Week 2: Beta Testing
- Day 1-5: Collect feedback
- Fix critical issues
- Daily builds if needed

### Week 3: Production Build
- Day 1-2: Final polish
- Day 3: Production builds
- Day 4: Submit for review
- Day 5: Waiting for approval

### Week 4: Launch
- Day 1: Approval received
- Day 2: Phased rollout (10%)
- Day 3: Increase to 50%
- Day 4-5: 100% rollout
- Monitor closely

---

## 🎯 Post-Launch

### Day 1

- [ ] Monitor crash reports
- [ ] Check server load
- [ ] Verify analytics working
- [ ] Review user feedback

### Week 1

- [ ] Respond to reviews
- [ ] Fix critical bugs (hotfix if needed)
- [ ] Monitor retention metrics
- [ ] Plan first update

### Month 1

- [ ] Review metrics (DAU, retention, revenue)
- [ ] Plan content updates
- [ ] Community engagement
- [ ] Marketing campaigns

---

## 📞 Support

### Issue Escalation

**Critical (Production Down):**
1. Check server status
2. Check error logs
3. Rollback if needed
4. Notify users via status page

**High (Feature Broken):**
1. Identify affected users
2. Hotfix if possible
3. Update in next version

**Medium/Low:**
1. Log in issue tracker
2. Schedule for next sprint

---

## 📚 Resources

- [Unity iOS Build](https://docs.unity3d.com/Manual/iphone-BuildProcess.html)
- [Unity Android Build](https://docs.unity3d.com/Manual/android-BuildProcess.html)
- [App Store Review Guidelines](https://developer.apple.com/app-store/review/guidelines/)
- [Google Play Policy](https://play.google.com/about/developer-content-policy/)
- [PM2 Documentation](https://pm2.keymetrics.io/docs/usage/quick-start/)
- [Nginx Documentation](https://nginx.org/en/docs/)

---

**Last Updated:** December 27, 2024

**Deployment Team:** _[Your Team]_
**Production URL:** https://api.wave-island.com
**Support Email:** support@wave-island.com
