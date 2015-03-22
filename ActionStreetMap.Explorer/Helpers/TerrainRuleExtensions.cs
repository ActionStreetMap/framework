using System.Collections.Generic;
using ActionStreetMap.Core.MapCss.Domain;
using ActionStreetMap.Core.Scene.Details;

namespace ActionStreetMap.Explorer.Helpers
{
    internal static class TerrainRuleExtensions
    {
        public static int GetSplatIndex(this Rule rule)
        {
            return rule.Evaluate<int>("splat");
        }

        public static bool IsForest(this Rule rule)
        {
            return rule.EvaluateDefault<bool>("forest", false);
        }
    }
}
