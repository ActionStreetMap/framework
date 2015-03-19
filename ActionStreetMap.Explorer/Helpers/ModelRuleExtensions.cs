using ActionStreetMap.Core.MapCss.Domain;
using ActionStreetMap.Core.Utilities;
using ActionStreetMap.Explorer.Scene.Utils;
using UnityEngine;

namespace ActionStreetMap.Explorer.Helpers
{
    internal static class ModelRuleExtensions
    {
        public static Material GetMaterial(this Rule rule, IResourceProvider resourceProvider)
        {
            var path = rule.Evaluate<string>("material");
            return resourceProvider.GetMatertial(@"Materials/" + path);
        }

        public static Material GetMaterial(this Rule rule, string path, IResourceProvider resourceProvider)
        {
            var materialPath = rule.Evaluate<string>(path);
            return resourceProvider.GetMatertial(@"Materials/" + materialPath);
        }

        public static Texture GetTexture(this Rule rule, IResourceProvider resourceProvider)
        {
            var path = rule.Evaluate<string>("material");
            return resourceProvider.GetTexture(@"Textures/" + path);
        }

        public static Color32 GetFillUnityColor(this Rule rule)
        {
            var coreColor = rule.Evaluate<Core.Unity.Color32>("fill-color", ColorUtility.FromUnknown);
            return new Color32(coreColor.R, coreColor.G, coreColor.B, coreColor.A);
        }

        public static Core.Unity.Color32 GetFillColor(this Rule rule)
        {
            return rule.Evaluate<Core.Unity.Color32>("fill-color", ColorUtility.FromUnknown);
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
