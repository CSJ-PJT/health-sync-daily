using System;
using System.Collections.Generic;
using DeepStake.World;
using UnityEngine;
using UnityEngine.Rendering;

namespace DeepStake.Rendering
{
    public static class DeepStakePbrEnvironmentPipeline
    {
        private const string MaterialLibraryResourcePath = "PBR/deepstake_pbr_material_library";
        private const string SceneMappingResourcePath = "PBR/deepstake_pbr_scene_mapping";
        private const string LightingProfileResourcePath = "PBR/deepstake_pbr_lighting_profile";
        private const string GeneratedVisualRootName = "__GeneratedZoneVisuals";
        private static DeepStakePbrMaterialLibrary cachedLibrary;
        private static DeepStakePbrSceneMapping cachedSceneMapping;
        private static DeepStakePbrLightingProfile cachedLightingProfile;
        private static readonly Dictionary<string, Material> RuntimeMaterialCache = new Dictionary<string, Material>(StringComparer.OrdinalIgnoreCase);
        private static readonly HashSet<string> RoughnessWarnings = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        public static void ApplyToWorld(
            Transform zoneRoot,
            Transform primaryInteractable,
            Transform secondaryInteractable,
            Transform tertiaryInteractable,
            Transform primaryPlacement,
            Transform secondaryPlacement,
            Transform primaryPreview,
            Transform secondaryPreview)
        {
            ApplyLightingProfile();

            if (zoneRoot == null)
            {
                return;
            }

            var generatedRoot = zoneRoot.Find(GeneratedVisualRootName);
            ApplyMappingsToHierarchy(generatedRoot != null ? generatedRoot : zoneRoot);

            ApplyMappingsToHierarchy(primaryInteractable);
            ApplyMappingsToHierarchy(secondaryInteractable);
            ApplyMappingsToHierarchy(tertiaryInteractable);
            ApplyMappingsToHierarchy(primaryPlacement);
            ApplyMappingsToHierarchy(secondaryPlacement);
            ApplyMappingsToHierarchy(primaryPreview);
            ApplyMappingsToHierarchy(secondaryPreview);
        }

        public static void ApplyLightingProfile()
        {
            var profile = LoadLightingProfile();
            if (profile == null)
            {
                return;
            }

            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = ToColor(profile.ambientColor, new Color(0.48f, 0.47f, 0.44f));
            RenderSettings.fog = profile.enableFog;
            RenderSettings.fogColor = ToColor(profile.fogColor, new Color(0.68f, 0.70f, 0.69f));
            RenderSettings.fogDensity = Mathf.Max(0f, profile.fogDensity);

            var lights = UnityEngine.Object.FindObjectsByType<Light>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (var index = 0; index < lights.Length; index++)
            {
                var light = lights[index];
                if (light == null || light.type != LightType.Directional)
                {
                    continue;
                }

                light.color = ToColor(profile.directionalColor, new Color(1f, 0.95f, 0.88f));
                light.intensity = Mathf.Max(0.1f, profile.directionalIntensity);
                light.shadows = LightShadows.Soft;
                light.shadowStrength = Mathf.Clamp01(profile.directionalShadowStrength);
                light.shadowBias = 0.08f;
                light.shadowNormalBias = 0.4f;
            }
        }

        private static void ApplyMappingsToHierarchy(Transform root)
        {
            if (root == null)
            {
                return;
            }

            var renderers = root.GetComponentsInChildren<Renderer>(true);
            for (var index = 0; index < renderers.Length; index++)
            {
                var renderer = renderers[index];
                if (renderer == null)
                {
                    continue;
                }

                var slot = ResolveSlotForName(renderer.transform.name);
                if (slot == null)
                {
                    continue;
                }

                var sharedMaterials = renderer.sharedMaterials;
                if (sharedMaterials == null || sharedMaterials.Length == 0)
                {
                    renderer.sharedMaterial = GetOrCreateRuntimeMaterial(slot);
                    continue;
                }

                var material = GetOrCreateRuntimeMaterial(slot);
                var cloned = new Material[sharedMaterials.Length];
                for (var materialIndex = 0; materialIndex < cloned.Length; materialIndex++)
                {
                    cloned[materialIndex] = material;
                }

                renderer.sharedMaterials = cloned;
            }
        }

        private static DeepStakePbrMaterialSlot ResolveSlotForName(string objectName)
        {
            var mapping = LoadSceneMapping();
            var library = LoadMaterialLibrary();
            if (mapping == null || library == null || string.IsNullOrWhiteSpace(objectName))
            {
                return null;
            }

            var lowerName = objectName.ToLowerInvariant();
            for (var index = 0; index < mapping.rules.Length; index++)
            {
                var rule = mapping.rules[index];
                if (!MatchesRule(rule, lowerName))
                {
                    continue;
                }

                for (var slotIndex = 0; slotIndex < library.slots.Length; slotIndex++)
                {
                    if (string.Equals(library.slots[slotIndex].slotKey, rule.slotKey, StringComparison.OrdinalIgnoreCase))
                    {
                        return library.slots[slotIndex];
                    }
                }
            }

            return null;
        }

        private static bool MatchesRule(DeepStakePbrSceneMappingRule rule, string lowerName)
        {
            if (rule == null)
            {
                return false;
            }

            for (var index = 0; index < rule.excludeAny.Length; index++)
            {
                var token = rule.excludeAny[index];
                if (!string.IsNullOrWhiteSpace(token) && lowerName.Contains(token.ToLowerInvariant()))
                {
                    return false;
                }
            }

            if (rule.includeAny == null || rule.includeAny.Length == 0)
            {
                return true;
            }

            for (var index = 0; index < rule.includeAny.Length; index++)
            {
                var token = rule.includeAny[index];
                if (!string.IsNullOrWhiteSpace(token) && lowerName.Contains(token.ToLowerInvariant()))
                {
                    return true;
                }
            }

            return false;
        }

        private static Material GetOrCreateRuntimeMaterial(DeepStakePbrMaterialSlot slot)
        {
            if (RuntimeMaterialCache.TryGetValue(slot.slotKey, out var cached) && cached != null)
            {
                return cached;
            }

            var shader = slot.isDecal ? Shader.Find("Universal Render Pipeline/Decal") : null;
            if (shader == null)
            {
                shader = Shader.Find("Universal Render Pipeline/Lit");
            }
            if (shader == null)
            {
                shader = Shader.Find("Standard");
            }

            var material = new Material(shader)
            {
                name = "DSPBR_" + slot.slotKey,
                hideFlags = HideFlags.DontSave
            };

            var baseTexture = LoadTexture(slot.textures.baseColor);
            var tint = ToColor(slot.colorTint, Color.white);
            var placeholderColor = ParseHtmlColor(slot.placeholderColorHex, new Color(0.5f, 0.5f, 0.5f));
            var effectiveBaseColor = baseTexture != null
                ? MultiplyColors(Color.white, tint)
                : MultiplyColors(placeholderColor, tint);

            SetColor(material, "_BaseColor", effectiveBaseColor);
            SetColor(material, "_Color", effectiveBaseColor);

            if (baseTexture != null)
            {
                SetTexture(material, "_BaseMap", baseTexture, slot.uvTiling);
                SetTexture(material, "_MainTex", baseTexture, slot.uvTiling);
            }

            var normalTexture = LoadTexture(slot.textures.normal);
            if (normalTexture != null)
            {
                SetTexture(material, "_BumpMap", normalTexture, slot.uvTiling);
                SetFloat(material, "_BumpScale", Mathf.Max(0f, slot.normalStrength));
                material.EnableKeyword("_NORMALMAP");
            }

            var occlusionTexture = LoadTexture(slot.textures.ambientOcclusion);
            if (occlusionTexture != null)
            {
                SetTexture(material, "_OcclusionMap", occlusionTexture, slot.uvTiling);
            }
            SetFloat(material, "_OcclusionStrength", Mathf.Clamp(slot.aoIntensity, 0f, 1.5f));

            var metallicTexture = LoadTexture(slot.textures.metallic);
            if (metallicTexture != null)
            {
                SetTexture(material, "_MetallicGlossMap", metallicTexture, slot.uvTiling);
                material.EnableKeyword("_METALLICSPECGLOSSMAP");
            }
            SetFloat(material, "_Metallic", Mathf.Clamp01(slot.metallicMultiplier));

            // This scaffold does not repack standalone roughness maps into a URP mask map at runtime.
            // Until final import packs roughness into a shader-compatible texture, use the multiplier fallback.
            WarnAboutStandaloneRoughness(slot);
            var smoothness = 1f - Mathf.Clamp(slot.roughnessMultiplier * 0.75f, 0.05f, 0.95f);
            SetFloat(material, "_Smoothness", smoothness);

            var heightTexture = LoadTexture(slot.textures.height);
            if (heightTexture != null && slot.supportsHeight)
            {
                SetTexture(material, "_ParallaxMap", heightTexture, slot.uvTiling);
                SetFloat(material, "_Parallax", 0.015f);
            }

            RuntimeMaterialCache[slot.slotKey] = material;
            return material;
        }

        private static Texture2D LoadTexture(string resourcePath)
        {
            if (string.IsNullOrWhiteSpace(resourcePath))
            {
                return null;
            }

            return Resources.Load<Texture2D>(resourcePath);
        }

        private static void SetTexture(Material material, string propertyName, Texture texture, DeepStakePbrVector2Value tiling)
        {
            if (!material.HasProperty(propertyName) || texture == null)
            {
                return;
            }

            material.SetTexture(propertyName, texture);
            material.SetTextureScale(propertyName, new Vector2(
                tiling != null ? tiling.x : 1f,
                tiling != null ? tiling.y : 1f));
        }

        private static void SetColor(Material material, string propertyName, Color color)
        {
            if (material.HasProperty(propertyName))
            {
                material.SetColor(propertyName, color);
            }
        }

        private static void SetFloat(Material material, string propertyName, float value)
        {
            if (material.HasProperty(propertyName))
            {
                material.SetFloat(propertyName, value);
            }
        }

        private static void WarnAboutStandaloneRoughness(DeepStakePbrMaterialSlot slot)
        {
            if (slot == null || string.IsNullOrWhiteSpace(slot.textures.roughness))
            {
                return;
            }

            if (!RoughnessWarnings.Add(slot.slotKey))
            {
                return;
            }

            Debug.LogWarning(
                "[DeepStakePbr] Standalone roughness map is declared for slot '" + slot.slotKey +
                "' but the current runtime scaffold does not repack roughness into a URP mask map. " +
                "Using roughnessMultiplier fallback until final imported textures provide a shader-compatible packing.");
        }

        private static DeepStakePbrMaterialLibrary LoadMaterialLibrary()
        {
            if (cachedLibrary != null)
            {
                return cachedLibrary;
            }

            var asset = Resources.Load<TextAsset>(MaterialLibraryResourcePath);
            cachedLibrary = asset != null
                ? JsonUtility.FromJson<DeepStakePbrMaterialLibrary>(asset.text)
                : new DeepStakePbrMaterialLibrary();
            return cachedLibrary;
        }

        private static DeepStakePbrSceneMapping LoadSceneMapping()
        {
            if (cachedSceneMapping != null)
            {
                return cachedSceneMapping;
            }

            var asset = Resources.Load<TextAsset>(SceneMappingResourcePath);
            cachedSceneMapping = asset != null
                ? JsonUtility.FromJson<DeepStakePbrSceneMapping>(asset.text)
                : new DeepStakePbrSceneMapping();
            return cachedSceneMapping;
        }

        private static DeepStakePbrLightingProfile LoadLightingProfile()
        {
            if (cachedLightingProfile != null)
            {
                return cachedLightingProfile;
            }

            var asset = Resources.Load<TextAsset>(LightingProfileResourcePath);
            cachedLightingProfile = asset != null
                ? JsonUtility.FromJson<DeepStakePbrLightingProfile>(asset.text)
                : new DeepStakePbrLightingProfile();
            return cachedLightingProfile;
        }

        private static Color ToColor(DeepStakePbrColorValue value, Color fallback)
        {
            if (value == null)
            {
                return fallback;
            }

            return new Color(value.r, value.g, value.b, value.a);
        }

        private static Color ParseHtmlColor(string hex, Color fallback)
        {
            if (!string.IsNullOrWhiteSpace(hex) && ColorUtility.TryParseHtmlString(hex, out var parsed))
            {
                return parsed;
            }

            return fallback;
        }

        private static Color MultiplyColors(Color a, Color b)
        {
            return new Color(a.r * b.r, a.g * b.g, a.b * b.b, a.a * b.a);
        }
    }
}
