using System;

namespace DeepStake.Environment
{
    [Serializable]
    public sealed class DeepStakeMeshyModelRegistry
    {
        public int version;
        public string project = string.Empty;
        public string[] preferredFormatOrder = Array.Empty<string>();
        public DeepStakeMeshyModelRegistryEntry[] entries = Array.Empty<DeepStakeMeshyModelRegistryEntry>();
    }

    [Serializable]
    public sealed class DeepStakeMeshyModelRegistryEntry
    {
        public string assetId = string.Empty;
        public string category = string.Empty;
        public string sourceZipPattern = string.Empty;
        public string thirdPartySourcePath = string.Empty;
        public string curatedModelPath = string.Empty;
        public string prefabPath = string.Empty;
        public int priority;
        public string status = string.Empty;
        public string preferredFormat = string.Empty;
        public string expectedScaleHint = string.Empty;
        public string pivotHint = string.Empty;
        public string materialPolicy = string.Empty;
        public string placementUseCase = string.Empty;
        public string notes = string.Empty;
    }

    [Serializable]
    public sealed class DeepStakeMeshyPlacementMapping
    {
        public int version;
        public string project = string.Empty;
        public DeepStakeMeshyPlacementGroup[] placementGroups = Array.Empty<DeepStakeMeshyPlacementGroup>();
    }

    [Serializable]
    public sealed class DeepStakeMeshyPlacementGroup
    {
        public string assetId = string.Empty;
        public string[] targetZones = Array.Empty<string>();
        public string anchorType = string.Empty;
        public string placementRule = string.Empty;
        public string readabilityGoal = string.Empty;
        public string notes = string.Empty;
    }
}
