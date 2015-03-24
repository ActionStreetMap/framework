using ActionStreetMap.Core.MapCss.Domain;

namespace ActionStreetMap.Explorer.Helpers
{
    internal static class BuildingRuleExtensions
    {
        #region Facade

        public static string GetFacadeBuilder(this Rule rule)
        {
            return rule.Evaluate<string>("facade-builder");
        }

        public static string GetFacadeColor(this Rule rule)
        {
            return rule.Evaluate<string>("facade-color");
        }

        public static string GetFacadeMaterial(this Rule rule, string @default = null)
        {
            return rule.EvaluateDefault<string>("facade-material", @default);
        }

        #endregion

        #region Roof

        public static string GetRoofBuilder(this Rule rule, string @default = null)
        {
            return rule.EvaluateDefault<string>("roof-builder", @default);
        }

        public static string GetRoofColor(this Rule rule)
        {
            return rule.Evaluate<string>("roof-color");
        }

        public static float GetRoofHeight(this Rule rule, float defaultValue = 0)
        {
            return rule.EvaluateDefault<float>("roof-height", defaultValue);
        }

        public static string GetRoofMaterial(this Rule rule, string @default = null)
        {
            return rule.EvaluateDefault<string>("roof-material", @default);
        }

        #endregion

        public static float GetMinHeight(this Rule rule, float defaultValue = 0)
        {
            return rule.EvaluateDefault<float>("min_height", defaultValue);
        }

        public static int GetLevels(this Rule rule, int @default = 0)
        {
            return rule.EvaluateDefault("levels", @default);
        }

        public static bool IsPart(this Rule rule)
        {
            return rule.EvaluateDefault("part", false);
        }
    }
}
