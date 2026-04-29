using System;

namespace DeepStake.Rendering
{
    [Serializable]
    public sealed class DeepStakePbrVector2Value
    {
        public float x = 1f;
        public float y = 1f;
    }

    [Serializable]
    public sealed class DeepStakePbrColorValue
    {
        public float r = 1f;
        public float g = 1f;
        public float b = 1f;
        public float a = 1f;
    }

    [Serializable]
    public sealed class DeepStakePbrTextureChannels
    {
        public string baseColor = string.Empty;
        public string normal = string.Empty;
        public string roughness = string.Empty;
        public string metallic = string.Empty;
        public string ambientOcclusion = string.Empty;
        public string height = string.Empty;
        public string notes = string.Empty;
    }

    [Serializable]
    public sealed class DeepStakePbrMaterialSlot
    {
        public string slotKey = string.Empty;
        public string family = string.Empty;
        public string description = string.Empty;
        public string placeholderColorHex = "#808080";
        public DeepStakePbrTextureChannels textures = new DeepStakePbrTextureChannels();
        public DeepStakePbrVector2Value uvTiling = new DeepStakePbrVector2Value();
        public DeepStakePbrColorValue colorTint = new DeepStakePbrColorValue();
        public float normalStrength = 0.85f;
        public float roughnessMultiplier = 1f;
        public float metallicMultiplier = 0f;
        public float aoIntensity = 0.8f;
        public bool supportsHeight = false;
        public bool placeholderAllowed = true;
        public bool isDecal = false;
    }

    [Serializable]
    public sealed class DeepStakePbrMaterialLibrary
    {
        public DeepStakePbrMaterialSlot[] slots = Array.Empty<DeepStakePbrMaterialSlot>();
    }

    [Serializable]
    public sealed class DeepStakePbrSceneMappingRule
    {
        public string slotKey = string.Empty;
        public string[] includeAny = Array.Empty<string>();
        public string[] excludeAny = Array.Empty<string>();
        public string notes = string.Empty;
    }

    [Serializable]
    public sealed class DeepStakePbrSceneMapping
    {
        public DeepStakePbrSceneMappingRule[] rules = Array.Empty<DeepStakePbrSceneMappingRule>();
    }

    [Serializable]
    public sealed class DeepStakePbrLightingProfile
    {
        public DeepStakePbrColorValue directionalColor = new DeepStakePbrColorValue();
        public float directionalIntensity = 1.15f;
        public float directionalShadowStrength = 0.86f;
        public DeepStakePbrColorValue ambientColor = new DeepStakePbrColorValue();
        public bool enableFog = false;
        public DeepStakePbrColorValue fogColor = new DeepStakePbrColorValue();
        public float fogDensity = 0.0014f;
        public float postExposure = 0f;
        public float contrast = 6f;
        public float saturation = -10f;
        public float temperature = 6f;
        public float bloomIntensity = 0f;
        public float vignetteIntensity = 0.12f;
        public string notes = string.Empty;
    }
}
