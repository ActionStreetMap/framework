using ActionStreetMap.Core.MapCss.Domain;
using ActionStreetMap.Explorer.Infrastructure;
using ActionStreetMap.Unity.Wrappers;

namespace ActionStreetMap.Explorer.Helpers
{
    internal static class TerrainRuleExtensions
    {
        public static GradientWrapper GetBackgroundLayerGradient(this Rule rule, IResourceProvider resourceProvider)
        {
            var gradientKey = rule.Evaluate<string>("gradient_background");
            return resourceProvider.GetGradient(gradientKey);
        }

        public static GradientWrapper GetWaterLayerGradient(this Rule rule, IResourceProvider resourceProvider)
        {
            var gradientKey = rule.Evaluate<string>("gradient_water");
            return resourceProvider.GetGradient(gradientKey);
        }

        public static GradientWrapper GetCarLayerGradient(this Rule rule, IResourceProvider resourceProvider)
        {
            var gradientKey = rule.Evaluate<string>("gradient_cars");
            return resourceProvider.GetGradient(gradientKey);
        }

        public static GradientWrapper GetPedestrianLayerGradient(this Rule rule, IResourceProvider resourceProvider)
        {
            var gradientKey = rule.Evaluate<string>("gradient_pedestrian");
            return resourceProvider.GetGradient(gradientKey);
        }

        public static bool IsForest(this Rule rule)
        {
            return rule.EvaluateDefault<bool>("forest", false);
        }
    }
}
