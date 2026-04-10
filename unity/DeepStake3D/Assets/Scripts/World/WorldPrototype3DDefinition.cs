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
    public sealed class WorldPrototype3DDefinition
    {
        public string sceneId = "world-prototype-3d";
        public string bootMode = "local";
        public string startMap = "farm";
        public WorldPrototype3DPoint playerSpawn = new WorldPrototype3DPoint();
        public WorldPrototype3DCameraOffset cameraOffset = new WorldPrototype3DCameraOffset();
        public WorldPrototype3DNpcStub[] npcStubs = Array.Empty<WorldPrototype3DNpcStub>();
        public WorldPrototype3DInteractableStub[] interactables = Array.Empty<WorldPrototype3DInteractableStub>();
        public WorldPrototype3DPoint settlementPlacementOrigin = new WorldPrototype3DPoint();

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
