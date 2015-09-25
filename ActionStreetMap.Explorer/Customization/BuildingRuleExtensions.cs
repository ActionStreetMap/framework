using ActionStreetMap.Core.MapCss.Domain;

namespace ActionStreetMap.Explorer.Customization
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

        public static string GetFacadeMaterial(this Rule rule)
        {
            return rule.Evaluate<string>("roof-material");
        }

        public static string GetFacadeTexture(this Rule rule)
        {
            return rule.Evaluate<string>("facade-texture");
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
            return rule.Evaluate<string>("roof-material");
        }

        public static string GetRoofTexture(this Rule rule)
        {
            return rule.Evaluate<string>("roof-texture");
        }

        #endregion

        #region Floor

        public static string GetFloorFrontColor(this Rule rule)
        {
            return rule.Evaluate<string>("floor-front-color");
        }

        public static string GetFloorFrontTexture(this Rule rule)
        {
            return rule.Evaluate<string>("floor-front-texture");
        }

        public static string GetFloorBackColor(this Rule rule)
        {
            return rule.Evaluate<string>("floor-back-color");
        }

        public static string GetFloorBackTexture(this Rule rule)
        {
            return rule.Evaluate<string>("floor-back-texture");
        }

        public static int GetLevels(this Rule rule, int @default = 0)
        {
            return (int)rule.EvaluateDefault("levels", (float)@default);
        }

        #endregion

        public static float GetMinHeight(this Rule rule, float defaultValue = 0)
        {
            return rule.EvaluateDefault<float>("min-height", defaultValue);
        }      

        public static bool IsPart(this Rule rule)
        {
            return rule.EvaluateDefault("part", false);
        }

        public static bool HasWindows(this Rule rule)
        {
            return rule.EvaluateDefault("windowed", false);
        }
    }
}
