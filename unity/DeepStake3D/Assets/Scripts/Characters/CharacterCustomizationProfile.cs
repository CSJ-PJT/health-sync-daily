using System;

namespace DeepStake.Characters
{
    [Serializable]
    public sealed class CharacterCustomizationProfile
    {
        public int facePreset;
        public int eyePreset;
        public int earPreset;
        public int nosePreset;
        public int clothesPreset;
        public int shoesPreset;
        public int hairPreset;

        public static CharacterCustomizationProfile Default()
        {
            return new CharacterCustomizationProfile
            {
                facePreset = 0,
                eyePreset = 0,
                earPreset = 0,
                nosePreset = 0,
                clothesPreset = 0,
                shoesPreset = 0,
                hairPreset = 0
            };
        }
    }
}
