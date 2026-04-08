# Fifth Dawn First Mobile Release Checklist

## Scope

This checklist covers the first mobile deployment path for the standalone Fifth Dawn app.

Current target:
- Android first
- Capacitor shell
- standalone Fifth Dawn build
- optional health linkage only

## Before Native Project Creation

- Confirm `@capacitor/cli` is installed
- Confirm Android SDK tools are available
  - current local SDK path detected: `C:\Users\dan18\AppData\Local\Android\Sdk\platform-tools\adb.exe`
- Confirm `apps/fifth-dawn-game/.env.mobile.example` values for:
  - `VITE_SUPABASE_URL`
  - `VITE_SUPABASE_PUBLISHABLE_KEY`
  - `VITE_FIFTH_DAWN_PRODUCT_KEY`
  - `VITE_FIFTH_DAWN_APP_ID`
  - `VITE_FIFTH_DAWN_APP_NAME`
  - `VITE_FIFTH_DAWN_ANDROID_SCHEME`
  - `VITE_FIFTH_DAWN_ORIENTATION`
  - `VITE_FIFTH_DAWN_STATUS_BAR_COLOR`
- Build the standalone game:
  - `npm run build:game`
- Verify gameplay basics in browser:
  - top-down movement
  - farming loop
  - village / mine / northern reach transitions
  - settlement panel
  - local save
  - linked save

## Native Project Creation

- Add Android shell:
  - `npm run mobile:game:add:android`
- Optional iOS shell:
  - `npm run mobile:game:add:ios`

## After Native Project Creation

- Copy/sync web assets:
  - `npm run mobile:game:build`
  - `npm run mobile:game:copy:android`
  - `npm run mobile:game:sync:android`
- Open Android Studio project:
  - `npm run mobile:game:open:android`

## Device Verification

- Confirm fullscreen host readability
- Confirm touch controls for:
  - move
  - action
  - interact
  - sleep
- Confirm hotbar selection on small screens
- Confirm settlement builder usability on touch
- Confirm keyboard fallback still works
- Confirm app resumes cleanly after backgrounding
- Confirm save/load after app relaunch
- Confirm linked bonus fetch does not require raw health data

## Release Prep

- Replace placeholder app id
- Add launcher icons
- Add splash assets
- Configure signing key
- Set package / bundle names
- Fill store metadata
- Verify privacy disclosure around health linkage:
  - derived values only
  - no raw health record export to game

## Known Manual Blockers

- Android SDK / Android Studio setup
- `npx cap add android` first-run project generation
- signing config
- store credentials
- real-device orientation tuning
- final icon / splash art
