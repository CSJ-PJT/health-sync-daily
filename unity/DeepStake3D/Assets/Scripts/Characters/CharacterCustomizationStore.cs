using UnityEngine;

namespace DeepStake.Characters
{
    public static class CharacterCustomizationStore
    {
        private const string Prefix = "DeepStake.CharacterCustomization.";
        private const int FaceCount = 4;
        private const int EyeCount = 4;
        private const int EarCount = 3;
        private const int NoseCount = 3;
        private const int ClothesCount = 5;
        private const int ShoesCount = 4;
        private const int HairCount = 5;

        private static readonly Color[] FaceColors =
        {
            new Color(0.94f, 0.82f, 0.73f),
            new Color(0.88f, 0.70f, 0.58f),
            new Color(0.74f, 0.55f, 0.43f),
            new Color(0.62f, 0.43f, 0.32f)
        };

        private static readonly Color[] EyeColors =
        {
            new Color(0.10f, 0.09f, 0.08f),
            new Color(0.17f, 0.20f, 0.22f),
            new Color(0.18f, 0.26f, 0.20f),
            new Color(0.23f, 0.18f, 0.13f)
        };

        private static readonly Color[] ClothesColors =
        {
            new Color(0.08f, 0.09f, 0.10f),
            new Color(0.22f, 0.25f, 0.21f),
            new Color(0.18f, 0.22f, 0.28f),
            new Color(0.30f, 0.26f, 0.20f),
            new Color(0.25f, 0.25f, 0.24f)
        };

        private static readonly Color[] ShoesColors =
        {
            new Color(0.08f, 0.08f, 0.08f),
            new Color(0.18f, 0.12f, 0.08f),
            new Color(0.12f, 0.13f, 0.14f),
            new Color(0.24f, 0.20f, 0.15f)
        };

        private static readonly Color[] HairColors =
        {
            new Color(0.05f, 0.05f, 0.055f),
            new Color(0.20f, 0.13f, 0.08f),
            new Color(0.36f, 0.20f, 0.10f),
            new Color(0.45f, 0.39f, 0.32f),
            new Color(0.11f, 0.11f, 0.12f)
        };

        public static CharacterCustomizationProfile Load()
        {
            return new CharacterCustomizationProfile
            {
                facePreset = PlayerPrefs.GetInt(Key("Face"), 0),
                eyePreset = PlayerPrefs.GetInt(Key("Eye"), 0),
                earPreset = PlayerPrefs.GetInt(Key("Ear"), 0),
                nosePreset = PlayerPrefs.GetInt(Key("Nose"), 0),
                clothesPreset = PlayerPrefs.GetInt(Key("Clothes"), 0),
                shoesPreset = PlayerPrefs.GetInt(Key("Shoes"), 0),
                hairPreset = PlayerPrefs.GetInt(Key("Hair"), 0)
            };
        }

        public static void Save(CharacterCustomizationProfile profile)
        {
            if (profile == null)
            {
                profile = CharacterCustomizationProfile.Default();
            }

            PlayerPrefs.SetInt(Key("Face"), Wrap(profile.facePreset, FaceCount));
            PlayerPrefs.SetInt(Key("Eye"), Wrap(profile.eyePreset, EyeCount));
            PlayerPrefs.SetInt(Key("Ear"), Wrap(profile.earPreset, EarCount));
            PlayerPrefs.SetInt(Key("Nose"), Wrap(profile.nosePreset, NoseCount));
            PlayerPrefs.SetInt(Key("Clothes"), Wrap(profile.clothesPreset, ClothesCount));
            PlayerPrefs.SetInt(Key("Shoes"), Wrap(profile.shoesPreset, ShoesCount));
            PlayerPrefs.SetInt(Key("Hair"), Wrap(profile.hairPreset, HairCount));
            PlayerPrefs.Save();
        }

        public static void Reset()
        {
            Save(CharacterCustomizationProfile.Default());
        }

        public static CharacterCustomizationProfile CycleFace()
        {
            var profile = Load();
            profile.facePreset++;
            Save(profile);
            return Load();
        }

        public static CharacterCustomizationProfile CycleEye()
        {
            var profile = Load();
            profile.eyePreset++;
            Save(profile);
            return Load();
        }

        public static CharacterCustomizationProfile CycleEar()
        {
            var profile = Load();
            profile.earPreset++;
            Save(profile);
            return Load();
        }

        public static CharacterCustomizationProfile CycleNose()
        {
            var profile = Load();
            profile.nosePreset++;
            Save(profile);
            return Load();
        }

        public static CharacterCustomizationProfile CycleClothes()
        {
            var profile = Load();
            profile.clothesPreset++;
            Save(profile);
            return Load();
        }

        public static CharacterCustomizationProfile CycleShoes()
        {
            var profile = Load();
            profile.shoesPreset++;
            Save(profile);
            return Load();
        }

        public static CharacterCustomizationProfile CycleHair()
        {
            var profile = Load();
            profile.hairPreset++;
            Save(profile);
            return Load();
        }

        public static Color ResolveFaceColor(CharacterCustomizationProfile profile)
        {
            return FaceColors[Wrap(profile != null ? profile.facePreset : 0, FaceColors.Length)];
        }

        public static Color ResolveEyeColor(CharacterCustomizationProfile profile)
        {
            return EyeColors[Wrap(profile != null ? profile.eyePreset : 0, EyeColors.Length)];
        }

        public static Color ResolveClothesColor(CharacterCustomizationProfile profile)
        {
            return ClothesColors[Wrap(profile != null ? profile.clothesPreset : 0, ClothesColors.Length)];
        }

        public static Color ResolveShoesColor(CharacterCustomizationProfile profile)
        {
            return ShoesColors[Wrap(profile != null ? profile.shoesPreset : 0, ShoesColors.Length)];
        }

        public static Color ResolveHairColor(CharacterCustomizationProfile profile)
        {
            return HairColors[Wrap(profile != null ? profile.hairPreset : 0, HairColors.Length)];
        }

        public static string Describe(CharacterCustomizationProfile profile)
        {
            if (profile == null)
            {
                profile = CharacterCustomizationProfile.Default();
            }

            return "Face " + (Wrap(profile.facePreset, FaceCount) + 1) +
                   " | Eyes " + (Wrap(profile.eyePreset, EyeCount) + 1) +
                   " | Ears " + (Wrap(profile.earPreset, EarCount) + 1) +
                   " | Nose " + (Wrap(profile.nosePreset, NoseCount) + 1) +
                   " | Clothes " + (Wrap(profile.clothesPreset, ClothesCount) + 1) +
                   " | Shoes " + (Wrap(profile.shoesPreset, ShoesCount) + 1) +
                   " | Hair " + (Wrap(profile.hairPreset, HairCount) + 1);
        }

        public static string LimitationsNote()
        {
            return "Color presets apply now. Face-part shape presets need FBX blendshapes or separate eye/ear/nose meshes.";
        }

        private static string Key(string name)
        {
            return Prefix + name;
        }

        private static int Wrap(int value, int count)
        {
            if (count <= 0)
            {
                return 0;
            }

            var wrapped = value % count;
            return wrapped < 0 ? wrapped + count : wrapped;
        }
    }
}
