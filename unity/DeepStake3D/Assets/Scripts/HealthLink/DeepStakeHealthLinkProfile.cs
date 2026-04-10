using System;

namespace DeepStake.HealthLink
{
    [Serializable]
    public sealed class DeepStakeHealthLinkProfile
    {
        public int ActivityTier;
        public int SleepTier;
        public int RecoveryTier;
        public int HydrationTier;
        public int ConsistencyScore;
        public int WeeklyMovementScore;
        public int FocusScore;
        public int ResonancePoints;
        public string[] DailyMissionFlags = Array.Empty<string>();
        public string[] WeeklyMissionFlags = Array.Empty<string>();
        public string LastRefreshAt = string.Empty;
    }
}
