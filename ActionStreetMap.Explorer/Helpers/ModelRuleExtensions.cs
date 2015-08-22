using ActionStreetMap.Core.MapCss.Domain;
using ActionStreetMap.Core.Utils;
using ActionStreetMap.Explorer.Infrastructure;
using UnityEngine;

namespace ActionStreetMap.Explorer.Helpers
{
    internal static class ModelRuleExtensions
    {
        public static string GetMaterialKey(this Rule rule, string path = "material")
        {
            return @"Materials/" + rule.Evaluate<string>(path);
        }

        public static Material GetMaterial(this Rule rule, IResourceProvider resourceProvider)
        {
            var path = rule.GetMaterialKey();
            return resourceProvider.GetMaterial(path);
        }

        public static Material GetMaterial(this Rule rule, string path, IResourceProvider resourceProvider)
        {
            var materialPath = rule.GetMaterialKey(path);
            return resourceProvider.GetMaterial(materialPath);
        }

        public static Color32 GetFillUnityColor(this Rule rule)
        {
            var coreColor = rule.Evaluate<Core.Unity.Color32>("fill-color", ColorUtils.FromUnknown);
            return new Color32(coreColor.R, coreColor.G, coreColor.B, coreColor.A);
        }

        public static string GetFillColor(this Rule rule)
        {
            return rule.Evaluate<string>("fill-color");
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
