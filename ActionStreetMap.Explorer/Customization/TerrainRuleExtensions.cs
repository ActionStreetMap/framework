using ActionStreetMap.Core.MapCss.Domain;
using ActionStreetMap.Unity.Wrappers;
using UnityEngine;

namespace ActionStreetMap.Explorer.Customization
{
    internal static class TerrainRuleExtensions
    {
        #region Background layer

        public static GradientWrapper GetBackgroundLayerGradient(this Rule rule, 
            CustomizationService customizationService)
        {
            return customizationService.GetGradient(rule.Evaluate<string>("background-gradient"));
        }

        public static TextureGroup.Texture GetBackgroundLayerTexture(this Rule rule, int seed,
            CustomizationService customizationService)
        {
            return customizationService
                .GetAtlas(rule.Evaluate<string>("material"))
                .Get(rule.Evaluate<string>("background-texture"))
                .Get(seed);
        }

        public static float GetBackgroundLayerColorNoiseFreq(this Rule rule)
        {
            return rule.Evaluate<float>("background-color-noise-freq");
        }

        public static float GetBackgroundLayerEleNoiseFreq(this Rule rule)
        {
            return rule.Evaluate<float>("background-ele-noise-freq");
        }

        #endregion

        #region Water layer

        public static GradientWrapper GetWaterLayerGradient(this Rule rule, 
            CustomizationService customizationService)
        {
            return customizationService.GetGradient(rule.Evaluate<string>("water-gradient"));
        }

        public static TextureGroup.Texture GetWaterLayerTexture(this Rule rule, int seed,
            CustomizationService customizationService)
        {
            return customizationService
                .GetAtlas(rule.Evaluate<string>("water-material"))
                .Get(rule.Evaluate<string>("water-texture"))
                .Get(seed);
        }

        public static float GetWaterLayerColorNoiseFreq(this Rule rule)
        {
            return rule.Evaluate<float>("water-color-noise-freq");
        }

        public static float GetWaterLayerEleNoiseFreq(this Rule rule)
        {
            return rule.Evaluate<float>("water-ele-noise-freq");
        }

        public static float GetWaterLayerBottomLevel(this Rule rule)
        {
            return rule.Evaluate<float>("water-bottom-level");
        }

        public static float GetWaterLayerSurfaceLevel(this Rule rule)
        {
            return rule.Evaluate<float>("water-surface-level");
        }

        public static Material GetWaterMaterial(this Rule rule, CustomizationService customizationService)
        {
            return rule.GetMaterial("water-material", customizationService);
        }

        #endregion

        #region Car layer

        public static GradientWrapper GetCarLayerGradient(this Rule rule, CustomizationService customizationService)
        {
            return customizationService.GetGradient(rule.Evaluate<string>("car-gradient"));
        }

        public static TextureGroup.Texture CarLayerTexture(this Rule rule, int seed,
            CustomizationService customizationService)
        {
            return customizationService
                .GetAtlas(rule.Evaluate<string>("material"))
                .Get(rule.Evaluate<string>("car-texture"))
                .Get(seed);
        }

        public static float GetCarLayerColorNoiseFreq(this Rule rule)
        {
            return rule.Evaluate<float>("car-color-noise-freq");
        }

        public static float GetCarLayerEleNoiseFreq(this Rule rule)
        {
            return rule.Evaluate<float>("car-ele-noise-freq");
        }

        #endregion

        #region Pedestrian layer

        public static GradientWrapper GetPedestrianLayerGradient(this Rule rule, 
            CustomizationService customizationService)
        {
            return customizationService.GetGradient(rule.Evaluate<string>("pedestrian-gradient"));
        }

        public static TextureGroup.Texture GetPedestrianLayerTexture(this Rule rule, int seed, 
            CustomizationService customizationService)
        {
            return customizationService
                .GetAtlas(rule.Evaluate<string>("material"))
                .Get(rule.Evaluate<string>("pedestrian-texture"))
                .Get(seed);
        }

        public static float GetPedestrianLayerColorNoiseFreq(this Rule rule)
        {
            return rule.Evaluate<float>("pedestrian-color-noise-freq");
        }

        public static float GetPedestrianLayerEleNoiseFreq(this Rule rule)
        {
            return rule.Evaluate<float>("pedestrian-ele-noise-freq");
        }

        #endregion

        #region Surfaces

        public static float GetColorNoiseFreq(this Rule rule, float @default = 0.05f)
        {
            return rule.EvaluateDefault<float>("color-noise-freq", @default);
        }

        public static float GetEleNoiseFreq(this Rule rule, float @default = 0.15f)
        {
            return rule.EvaluateDefault<float>("ele-noise-freq", @default);
        }

        #endregion

        public static bool IsForest(this Rule rule)
        {
            return rule.EvaluateDefault<bool>("forest", false);
        }
    }
}
