using ActionStreetMap.Core.MapCss.Domain;
using ActionStreetMap.Core.Utils;
using ActionStreetMap.Explorer.Infrastructure;
using UnityEngine;

namespace ActionStreetMap.Explorer.Helpers
{
    internal static class ModelRuleExtensions
    {
        public static Material GetMaterial(this Rule rule, IResourceProvider resourceProvider)
        {
            var path = rule.Evaluate<string>("material");
            return resourceProvider.GetMaterial(@"Materials/" + path);
        }

        public static Material GetMaterial(this Rule rule, string path, IResourceProvider resourceProvider)
        {
            var materialPath = rule.Evaluate<string>(path);
            return resourceProvider.GetMaterial(@"Materials/" + materialPath);
        }

        public static string GetMaterialKey(this Rule rule)
        {
            return rule.Evaluate<string>("material");
        }

        public static Texture GetTexture(this Rule rule, IResourceProvider resourceProvider)
        {
            var path = rule.Evaluate<string>("material");
            return resourceProvider.GetTexture(@"Textures/" + path);
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
