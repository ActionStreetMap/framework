using ActionStreetMap.Core.MapCss.Domain;
using ActionStreetMap.Unity.Wrappers;

namespace ActionStreetMap.Explorer.Customization
{
    internal static class TreeRuleExtensions
    {
        public static GradientWrapper GetTrunkGradient(this Rule rule,
           CustomizationService customizationService)
        {
            return customizationService.GetGradient(rule.Evaluate<string>("trunk-color"));
        }

        public static GradientWrapper GetFoliageGradient(this Rule rule,
           CustomizationService customizationService)
        {
            return customizationService.GetGradient(rule.Evaluate<string>("foliage-color"));
        }

        public static TextureGroup.Texture GetTrunkTexture(this Rule rule, int seed,
            CustomizationService customizationService)
        {
            return customizationService
                .GetAtlas(rule.Evaluate<string>("material"))
                .Get(rule.Evaluate<string>("trunk-texture"))
                .Get(seed);
        }

        public static TextureGroup.Texture GetFoliageTexture(this Rule rule, int seed,
            CustomizationService customizationService)
        {
            return customizationService
                .GetAtlas(rule.Evaluate<string>("material"))
                .Get(rule.Evaluate<string>("foliage-texture"))
                .Get(seed);
        }
    }
}
