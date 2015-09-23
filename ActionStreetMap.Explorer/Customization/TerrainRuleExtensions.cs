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
            return customizationService.GetGradient(rule.Evaluate<string>("background_gradient"));
        }

        public static TextureGroup.Texture GetBackgroundLayerTexture(this Rule rule, int seed,
            CustomizationService customizationService)
        {
            return customizationService
                .GetAtlas(rule.Evaluate<string>("material"))
                .Get("background_texture")
                .Get(seed);
        }

        public static float GetBackgroundLayerColorNoiseFreq(this Rule rule)
        {
            return rule.Evaluate<float>("background_color_noise_freq");
        }

        public static float GetBackgroundLayerEleNoiseFreq(this Rule rule)
        {
            return rule.Evaluate<float>("background_ele_noise_freq");
        }

        #endregion

        #region Water layer

        public static GradientWrapper GetWaterLayerGradient(this Rule rule, 
            CustomizationService customizationService)
        {
            return customizationService.GetGradient(rule.Evaluate<string>("water_gradient"));
        }

        public static TextureGroup.Texture GetWaterLayerTexture(this Rule rule, int seed,
            CustomizationService customizationService)
        {
            return customizationService
                .GetAtlas(rule.Evaluate<string>("water_material"))
                .Get("water_texture")
                .Get(seed);
        }

        public static float GetWaterLayerColorNoiseFreq(this Rule rule)
        {
            return rule.Evaluate<float>("water_color_noise_freq");
        }

        public static float GetWaterLayerEleNoiseFreq(this Rule rule)
        {
            return rule.Evaluate<float>("water_ele_noise_freq");
        }

        public static float GetWaterLayerBottomLevel(this Rule rule)
        {
            return rule.Evaluate<float>("water_bottom_level");
        }

        public static float GetWaterLayerSurfaceLevel(this Rule rule)
        {
            return rule.Evaluate<float>("water_surface_level");
        }

        public static Material GetWaterMaterial(this Rule rule, CustomizationService customizationService)
        {
            return rule.GetMaterial("water_material", customizationService);
        }

        #endregion

        #region Car layer

        public static GradientWrapper GetCarLayerGradient(this Rule rule, CustomizationService customizationService)
        {
            return customizationService.GetGradient(rule.Evaluate<string>("car_gradient"));
        }

        public static TextureGroup.Texture CarLayerTexture(this Rule rule, int seed,
            CustomizationService customizationService)
        {
            return customizationService
                .GetAtlas(rule.Evaluate<string>("material"))
                .Get("car_texture")
                .Get(seed);
        }

        public static float GetCarLayerColorNoiseFreq(this Rule rule)
        {
            return rule.Evaluate<float>("car_color_noise_freq");
        }

        public static float GetCarLayerEleNoiseFreq(this Rule rule)
        {
            return rule.Evaluate<float>("car_ele_noise_freq");
        }

        #endregion

        #region Pedestrian layer

        public static GradientWrapper GetPedestrianLayerGradient(this Rule rule, 
            CustomizationService customizationService)
        {
            return customizationService.GetGradient(rule.Evaluate<string>("pedestrian_gradient"));
        }

        public static TextureGroup.Texture GetPedestrianLayerTexture(this Rule rule, int seed, 
            CustomizationService customizationService)
        {
            return customizationService
                .GetAtlas(rule.Evaluate<string>("material"))
                .Get("pedestrian_texture")
                .Get(seed);
        }

        public static float GetPedestrianLayerColorNoiseFreq(this Rule rule)
        {
            return rule.Evaluate<float>("pedestrian_color_noise_freq");
        }

        public static float GetPedestrianLayerEleNoiseFreq(this Rule rule)
        {
            return rule.Evaluate<float>("pedestrian_ele_noise_freq");
        }

        #endregion

        #region Surfaces

        public static float GetColorNoiseFreq(this Rule rule, float @default = 0.05f)
        {
            return rule.EvaluateDefault<float>("color_noise_freq", @default);
        }

        public static float GetEleNoiseFreq(this Rule rule, float @default = 0.15f)
        {
            return rule.EvaluateDefault<float>("ele_noise_freq", @default);
        }

        #endregion

        public static bool IsForest(this Rule rule)
        {
            return rule.EvaluateDefault<bool>("forest", false);
        }
    }
}
