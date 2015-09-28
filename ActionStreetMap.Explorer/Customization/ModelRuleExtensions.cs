using ActionStreetMap.Core.MapCss.Domain;
using ActionStreetMap.Core.Utils;
using UnityEngine;

namespace ActionStreetMap.Explorer.Customization
{
    internal static class ModelRuleExtensions
    {
        public static string GetMaterialKey(this Rule rule, string path = "material", bool evaluate = true)
        {
            return @"Materials/" + (evaluate ? rule.Evaluate<string>(path) : path);
        }

        public static Material GetMaterial(this Rule rule, CustomizationService customizationService)
        {
            return customizationService.GetMaterial(rule.GetMaterialKey());
        }

        public static Material GetMaterial(this Rule rule, string path, CustomizationService customizationService)
        {
            return customizationService.GetMaterial(rule.GetMaterialKey(path));
        }

        public static Color32 GetUnityColor(this Rule rule)
        {
            var coreColor = rule.Evaluate<Core.Unity.Color32>("color", ColorUtils.FromUnknown);
            return new Color32(coreColor.R, coreColor.G, coreColor.B, coreColor.A);
        }

        public static string GetColor(this Rule rule)
        {
            return rule.Evaluate<string>("color");
        }

        public static string GetTextureAtlas(this Rule rule)
        {
            return rule.Evaluate<string>("material");
        }

        public static string GetTextureKey(this Rule rule)
        {
            return rule.Evaluate<string>("texture");
        }

        public static TextureGroup.Texture GetTexture(this Rule rule, int seed, 
            CustomizationService customizationService)
        {
            return customizationService
                .GetAtlas(rule.GetTextureAtlas())
                .Get(rule.GetTextureKey())
                .Get(seed); 
        }

        public static bool IsSkipped(this Rule rule)
        {
            return rule.EvaluateDefault("skip", false);
        }

        public static int GetLayerIndex(this Rule rule, int @default = -1)
        {
            return rule.EvaluateDefault("layer", @default);
        }
    }
}
