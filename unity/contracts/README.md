# Unity Contracts Handoff

This folder contains Unity-facing reference artifacts, not engine-generated output.

## Source of truth

- Portable TypeScript contracts:
  - `../../packages/shared-types/src/deepStake.ts`
- Save schema example:
  - `./deep-stake-save-schema.example.json`

## Use in Unity

- map these contracts to C# data models
- keep rendering and scene logic separate from the data contracts
- preserve `local/offline` boot as the default first vertical slice
- consume only derived health-link values, never raw health records

## Do not store here

- Unity `Library`, `Temp`, `Logs`, or `UserSettings`
- generated build output
- machine-specific editor state
