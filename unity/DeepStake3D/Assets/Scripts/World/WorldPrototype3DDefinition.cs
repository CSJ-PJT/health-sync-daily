using System;
using UnityEngine;

namespace DeepStake.World
{
    [Serializable]
    public sealed class WorldPrototype3DPoint
    {
        public float x;
        public float z;
    }

    [Serializable]
    public sealed class WorldPrototype3DCameraOffset
    {
        public float x;
        public float y;
        public float z;
    }

    [Serializable]
    public sealed class WorldPrototype3DNpcStub
    {
        public string id = string.Empty;
        public string displayName = string.Empty;
        public float x;
        public float z;
        public string questId = string.Empty;
        public string dialogue = string.Empty;
    }

    [Serializable]
    public sealed class WorldPrototype3DInteractableStub
    {
        public string id = string.Empty;
        public string label = string.Empty;
        public float x;
        public float z;
        public string message = string.Empty;
    }

    [Serializable]
    public sealed class WorldPrototype3DLandmark
    {
        public string id = string.Empty;
        public string kind = "crate";
        public string label = string.Empty;
        public float x;
        public float z;
        public float width = 1f;
        public float depth = 1f;
        public float height = 1f;
    }

    [Serializable]
    public sealed class WorldPrototype3DPlacementZone
    {
        public string objectType = "beacon";
        public string label = "Recovery Beacon";
        public float x;
        public float z;
    }

    [Serializable]
    public sealed class WorldPrototype3DDefinition
    {
        public string sceneId = "world-prototype-3d";
        public string bootMode = "local";
        public string startMap = "recovery-field";
        public string zoneId = "recovery-field";
        public string zoneLabel = "Recovery Field";
        public string zoneSummary =
            "A farm edge and service road where ordinary modern life has started to fray under hidden pressure.";
        public WorldPrototype3DPoint playerSpawn = new WorldPrototype3DPoint();
        public WorldPrototype3DCameraOffset cameraOffset = new WorldPrototype3DCameraOffset();
        public WorldPrototype3DNpcStub[] npcStubs = Array.Empty<WorldPrototype3DNpcStub>();
        public WorldPrototype3DInteractableStub[] interactables = Array.Empty<WorldPrototype3DInteractableStub>();
        public WorldPrototype3DLandmark[] landmarks = Array.Empty<WorldPrototype3DLandmark>();
        public WorldPrototype3DPlacementZone placementZone = new WorldPrototype3DPlacementZone();

        public static WorldPrototype3DDefinition FromJson(TextAsset asset)
        {
            if (asset == null || string.IsNullOrWhiteSpace(asset.text))
            {
                return new WorldPrototype3DDefinition();
            }

            var parsed = JsonUtility.FromJson<WorldPrototype3DDefinition>(asset.text);
            return parsed != null ? parsed : new WorldPrototype3DDefinition();
        }
    }
}
