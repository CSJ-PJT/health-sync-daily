using System;
using UnityEngine;

namespace DeepStake.World
{
    [Serializable]
    public sealed class WorldPrototypePoint
    {
        public float x;
        public float y;
    }

    [Serializable]
    public sealed class WorldPrototypeNpcStub
    {
        public string id = string.Empty;
        public string displayName = string.Empty;
        public float x;
        public float y;
        public string questId = string.Empty;
    }

    [Serializable]
    public sealed class WorldPrototypeInteractableStub
    {
        public string id = string.Empty;
        public string label = string.Empty;
        public float x;
        public float y;
        public string message = string.Empty;
    }

    [Serializable]
    public sealed class WorldPrototypeDefinition
    {
        public string sceneId = "world-prototype";
        public string bootMode = "local";
        public string startMap = "farm";
        public WorldPrototypePoint playerSpawn = new();
        public WorldPrototypeNpcStub[] npcStubs = Array.Empty<WorldPrototypeNpcStub>();
        public WorldPrototypeInteractableStub[] interactables = Array.Empty<WorldPrototypeInteractableStub>();
        public WorldPrototypePoint settlementPlacementOrigin = new();

        public static WorldPrototypeDefinition FromJson(TextAsset asset)
        {
            if (asset == null || string.IsNullOrWhiteSpace(asset.text))
            {
                return new WorldPrototypeDefinition();
            }

            return JsonUtility.FromJson<WorldPrototypeDefinition>(asset.text) ?? new WorldPrototypeDefinition();
        }
    }
}
