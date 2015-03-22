using ActionStreetMap.Core.MapCss.Domain;
using ActionStreetMap.Explorer.Infrastructure;
using ActionStreetMap.Unity.Wrappers;

namespace ActionStreetMap.Explorer.Helpers
{
    internal static class TerrainRuleExtensions
    {
        public static GradientWrapper GetCanvasLayerGradient(this Rule rule, IResourceProvider resourceProvider)
        {
            var gradientKey = rule.Evaluate<string>("layer_canvas");
            return resourceProvider.GetGradient(gradientKey);
        }

        public static GradientWrapper GetWaterLayerGradient(this Rule rule, IResourceProvider resourceProvider)
        {
            var gradientKey = rule.Evaluate<string>("layer_water");
            return resourceProvider.GetGradient(gradientKey);
        }

        public static GradientWrapper GetCarLayerGradient(this Rule rule, IResourceProvider resourceProvider)
        {
            var gradientKey = rule.Evaluate<string>("layer_cars");
            return resourceProvider.GetGradient(gradientKey);
        }

        public static GradientWrapper GetPedestrianLayerGradient(this Rule rule, IResourceProvider resourceProvider)
        {
            var gradientKey = rule.Evaluate<string>("layer_pedestrian");
            return resourceProvider.GetGradient(gradientKey);
        }

        public static bool IsForest(this Rule rule)
        {
            return rule.EvaluateDefault<bool>("forest", false);
        }
    }
}
