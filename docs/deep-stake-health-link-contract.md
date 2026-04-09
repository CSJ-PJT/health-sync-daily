# Deep Stake Health-Link Contract

## Rule

Deep Stake, including the Unity client, must never consume raw health records directly.

Only derived, game-safe values may cross from the health product into the game:
- activity tier
- sleep tier
- recovery tier
- hydration tier
- consistency score
- weekly movement score
- focus score
- resonance points
- daily mission flags
- weekly mission flags

## Client behavior

- cloud link is optional
- local/offline mode must remain playable without cloud link
- if cloud link is unavailable, Unity should continue in local mode
- no gameplay-critical system may require raw health sync

## Unity mapping guidance

Suggested C# model:

```csharp
public sealed class DeepStakeHealthLinkProfile {
    public int ActivityTier;
    public int SleepTier;
    public int RecoveryTier;
    public int HydrationTier;
    public int ConsistencyScore;
    public int WeeklyMovementScore;
    public int FocusScore;
    public int ResonancePoints;
    public string[] DailyMissionFlags;
    public string[] WeeklyMissionFlags;
    public string LastRefreshAt;
}
```

## Never send to Unity

- raw heart-rate streams
- full sleep records
- raw body composition records
- direct provider records
- raw normalized health rows

## Source references

- `packages/shared-types/src/index.ts`
- `packages/shared-types/src/deepStake.ts`
