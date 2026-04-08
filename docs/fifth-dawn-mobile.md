# Fifth Dawn Mobile Deployment Foundation

## Goal

Prepare the standalone Fifth Dawn app for a first phone deployment path without disrupting the current React/Vite game runtime.

The chosen path is `Capacitor`, because it is the lowest-disruption option for the existing standalone web client.

## Current Foundation

- App: `apps/fifth-dawn-game`
- Capacitor config: `apps/fifth-dawn-game/capacitor.config.ts`
- Mobile env example: `apps/fifth-dawn-game/.env.mobile.example`
- Root helper scripts:
  - `npm run mobile:game:build`
  - `npm run mobile:game:copy:android`
  - `npm run mobile:game:sync:android`
  - `npm run mobile:game:open:android`

## Local Commands

From repository root:

```powershell
npm run build:game
npm run mobile:game:build
npm run mobile:game:copy:android
npm run mobile:game:sync:android
npm run mobile:game:open:android
```

From the app directory:

```powershell
cd apps/fifth-dawn-game
npm run build
npm run cap:copy:android
npm run cap:sync:android
npm run cap:open:android
```

## Manual Setup Still Required

These steps are intentionally not automated in source control:

1. Install and/or confirm `@capacitor/cli`
2. Add native platforms the first time
   - `npx cap add android`
   - `npx cap add ios`
3. Configure Android package id / iOS bundle id for the release target
4. Add icons, splash, signing, certificates, and store metadata
5. Verify orientation and safe-area behavior on real devices

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
- Verify app resumes cleanly after backgrounding
- Add icons / splash / package ids before store builds

## Notes

- Fifth Dawn remains playable without any health linkage.
- The mobile shell must still consume only derived game-link values, never raw health records.
- Future controller support and richer mobile input should be layered on top of the current input abstraction rather than bypassing it.
