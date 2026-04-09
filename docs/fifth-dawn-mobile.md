# Fifth Dawn Mobile Deployment Foundation

## Goal

Prepare the standalone Fifth Dawn app for a first phone deployment path without disrupting the current React/Vite game runtime.

The chosen path is `Capacitor`, because it is the lowest-disruption option for the existing standalone web client.

## Cloud Config Boot Behavior

The standalone game must still boot when Supabase public env values are missing.

Current behavior:

- if `VITE_SUPABASE_URL` and `VITE_SUPABASE_PUBLISHABLE_KEY` are both present:
  - boot in `cloud` mode
  - derived link and cloud save paths may run
- if either value is missing:
  - boot in `local` mode
  - local save stays available
  - cloud sync and remote link fetch are disabled
  - the app shell shows a visible cloud status notice instead of crashing

Expected public env values for cloud mode:

- `VITE_SUPABASE_URL`
- `VITE_SUPABASE_PUBLISHABLE_KEY`

These are public client values only. Do not embed server secrets in the mobile app.

## Current Foundation

- App: `apps/fifth-dawn-game`
- Capacitor config: `apps/fifth-dawn-game/capacitor.config.ts`
- Mobile env example: `apps/fifth-dawn-game/.env.mobile.example`
- Product scope env: `VITE_FIFTH_DAWN_PRODUCT_KEY`
- Root helper scripts:
  - `npm run mobile:game:build`
  - `npm run mobile:game:add:android`
  - `npm run mobile:game:add:ios`
  - `npm run mobile:game:copy:android`
  - `npm run mobile:game:sync:android`
  - `npm run mobile:game:assemble:android`
  - `npm run mobile:game:install:android`
  - `npm run mobile:game:install:android:info`
  - `npm run mobile:game:install:android:stacktrace`
  - `npm run mobile:game:adb:devices`
  - `npm run mobile:game:gradle:stop`
  - `npm run mobile:game:open:android`

## Stable Gradle Cache

Repeated Android builds were slow because recent manual installs kept using a new temporary `GRADLE_USER_HOME` for each run. That forces Gradle to recreate caches, re-extract wrapper files, and cold-start daemons repeatedly.

The game now uses a stable reusable cache path for local development:

- default: `.gradle-local\\fifth-dawn-game`
- override: `FIFTH_DAWN_GRADLE_USER_HOME`

The wrapper is resolved through:

- `apps/fifth-dawn-game/scripts/android-gradle.ps1`

Do not keep generating random temp Gradle homes for normal debug installs.

## Local Commands

From repository root:

```powershell
npm run build:game
npm run mobile:game:build
npm run mobile:game:add:android
npm run mobile:game:copy:android
npm run mobile:game:sync:android
npm run mobile:game:assemble:android
npm run mobile:game:install:android
npm run mobile:game:install:android:info
npm run mobile:game:install:android:stacktrace
npm run mobile:game:adb:devices
npm run mobile:game:gradle:stop
npm run mobile:game:open:android
```

From the app directory:

```powershell
cd apps/fifth-dawn-game
npm run build
npm run cap:add:android
npm run cap:copy:android
npm run cap:sync:android
npm run android:assemble:debug
npm run android:install:debug
npm run android:install:debug:info
npm run android:install:debug:stacktrace
npm run android:adb:devices
npm run android:gradle:stop
npm run cap:open:android
```

## Recommended Android Debug Sequence

Use this order when debugging installs:

```powershell
npm run mobile:game:build
npm run mobile:game:sync:android
npm run mobile:game:assemble:android
npm run mobile:game:adb:devices
npm run mobile:game:install:android
```

Why the steps are split:

- `build`: web/Vite build only
- `sync`: copies web assets and updates Capacitor Android files
- `assemble`: compiles the Android app without touching the device
- `install`: pushes the built APK to the connected phone

If `installDebug` looks stuck:

1. Run `npm run mobile:game:adb:devices`
2. Run `npm run mobile:game:gradle:stop`
3. Retry `npm run mobile:game:install:android:info`
4. If still unclear, use `npm run mobile:game:install:android:stacktrace`

## Manual Setup Still Required

These steps are intentionally not automated in source control:

1. Install and/or confirm `@capacitor/cli`
   - current local check: not installed in this repo yet
2. Add native platforms the first time
   - `npm run mobile:game:add:android`
   - `npm run mobile:game:add:ios`
3. Configure Android package id / iOS bundle id for the release target
4. Add icons, splash, signing, certificates, and store metadata
5. Verify orientation and safe-area behavior on real devices
6. Add `adb` to PATH or keep using the explicit SDK path
   - current local SDK path exists: `C:\Users\dan18\AppData\Local\Android\Sdk\platform-tools\adb.exe`

## Recommended First Mobile Release Checklist

- Confirm Supabase env values in the mobile build
- Confirm fullscreen host readability on small screens
- Verify touch input for:
  - movement
  - action
  - interact
  - sleep
  - hotbar switching
  - settlement building panel
- Verify local save and linked cloud save
- Verify game-link token entry and refresh flow
- Verify product-scoped linkage stays on `fifth-dawn`
- Verify app resumes cleanly after backgrounding
- Add icons / splash / package ids before store builds
- Follow `docs/fifth-dawn-mobile-release-checklist.md` before store submission

## Notes

- Fifth Dawn remains playable without any health linkage.
- The mobile shell must still consume only derived game-link values, never raw health records.
- Linked cloud save and derived bonuses are both scoped by the `fifth-dawn` product key.
- Future controller support and richer mobile input should be layered on top of the current input abstraction rather than bypassing it.
- `adb` detection now prefers `ANDROID_SDK_ROOT`, then `ANDROID_HOME`, then `%LOCALAPPDATA%\\Android\\Sdk\\platform-tools\\adb.exe`.
- The stable local Gradle cache directory is intentionally reusable across repeated runs, so `assembleDebug` and `installDebug` stop paying the cold-cache cost every time.
