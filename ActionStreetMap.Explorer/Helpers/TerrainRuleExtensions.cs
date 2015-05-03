using ActionStreetMap.Core.MapCss.Domain;
using ActionStreetMap.Explorer.Infrastructure;
using ActionStreetMap.Unity.Wrappers;

namespace ActionStreetMap.Explorer.Helpers
{
    internal static class TerrainRuleExtensions
    {
        #region Background layer

        public static GradientWrapper GetBackgroundLayerGradient(this Rule rule, IResourceProvider resourceProvider)
        {
            var gradientKey = rule.Evaluate<string>("background_gradient");
            return resourceProvider.GetGradient(gradientKey);
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

        public static GradientWrapper GetWaterLayerGradient(this Rule rule, IResourceProvider resourceProvider)
        {
            var gradientKey = rule.Evaluate<string>("water_gradient");
            return resourceProvider.GetGradient(gradientKey);
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

        #endregion

        #region Car layer

        public static GradientWrapper GetCarLayerGradient(this Rule rule, IResourceProvider resourceProvider)
        {
            var gradientKey = rule.Evaluate<string>("car_gradient");
            return resourceProvider.GetGradient(gradientKey);
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

        public static GradientWrapper GetPedestrianLayerGradient(this Rule rule, IResourceProvider resourceProvider)
        {
            var gradientKey = rule.Evaluate<string>("pedestrian_gradient");
            return resourceProvider.GetGradient(gradientKey);
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

        public static float GetColorNoiseFreq(this Rule rule, float @default = 0.2f)
        {
            return rule.EvaluateDefault<float>("color_noise_freq", @default);
        }

        public static float GetEleNoiseFreq(this Rule rule, float @default = 0.2f)
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
