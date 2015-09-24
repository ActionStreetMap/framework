using ActionStreetMap.Core.MapCss.Domain;
using ActionStreetMap.Unity.Wrappers;

namespace ActionStreetMap.Explorer.Customization
{
    internal static class TreeRuleExtensions
    {
        public static GradientWrapper GetTrunkGradient(this Rule rule,
           CustomizationService customizationService)
        {
            return customizationService.GetGradient(rule.Evaluate<string>("trunk-gradient"));
        }

        public static GradientWrapper GetFoliageGradient(this Rule rule,
           CustomizationService customizationService)
        {
            return customizationService.GetGradient(rule.Evaluate<string>("foliage-gradient"));
        }

        public static TextureGroup.Texture GetTrunkTexture(this Rule rule, int seed,
            CustomizationService customizationService)
        {
            return customizationService
                .GetAtlas(rule.Evaluate<string>("material"))
                .Get("trunk-texture")
                .Get(seed);
        }

        public static TextureGroup.Texture GetFoliageTexture(this Rule rule, int seed,
            CustomizationService customizationService)
        {
            return customizationService
                .GetAtlas(rule.Evaluate<string>("material"))
                .Get("foliage-texture")
                .Get(seed);
        }
    }
}
