using ActionStreetMap.Core.MapCss.Domain;
using ActionStreetMap.Core.Scene.Roads;
using ActionStreetMap.Core.Tiling;
using ActionStreetMap.Explorer.Scene;
using ActionStreetMap.Explorer.Scene.Builders;

namespace ActionStreetMap.Explorer.Helpers
{
    /// <summary> Provides methods for basic mapcss properties receiving. </summary>
    internal static class CommonRuleExtensions
    {
        public static string GetKey(this Rule rule)
        {
            return rule.Evaluate<string>("key");
        }

        public static float GetHeight(this Rule rule, float defaultValue = 0)
        {
            return rule.EvaluateDefault("height", defaultValue);
        }

        /*public static IEnumerable<IModelBuilder> GetModelBuilders(this Rule rule, IEnumerable<IModelBuilder> builders)
        {
            var builderNames = rule.Evaluate<List<string>>("builder");
            if (builderNames == null)
                return null;
            return builders.Where(mb => builderNames.Contains(mb.Name));
        }*/

        public static IModelBuilder GetModelBuilder(this Rule rule, IModelBuilder[] builders)
        {
            var builderName = rule.EvaluateDefault<string>("builder", null);
            if (builderName == null)
                return null;
            // NOTE use for to avoid allocations
            for (int i = 0; i < builders.Length; i++)
                if (builders[i].Name == builderName)
                    return builders[i];
            return null;
        }

        public static IModelBehaviour GetModelBehaviour(this Rule rule, IModelBehaviour[] behaviours)
        {
            var behaviorName = rule.EvaluateDefault<string>("behaviour", null);
            if (behaviorName == null)
                return null;
            // NOTE use for to avoid allocations
            for (int i = 0; i < behaviours.Length; i++)
                if (behaviours[i].Name == behaviorName)
                    return behaviours[i];
            return null;
        }

        /// <summary> Z-index is just the lowest y coordinate. </summary>
        public static float GetZIndex(this Rule rule)
        {
            return rule.Evaluate<float>("z-index");
        }

        /// <summary> Gets width. </summary>
        public static float GetWidth(this Rule rule)
        {
            return rule.Evaluate<float>("width");
        }

        /// <summary> Gets road type. </summary>
        public static RoadType GetRoadType(this Rule rule)
        {
            var typeStr = rule.Evaluate<string>("type");
            switch (typeStr)
            {
                case "pedestrian":
                    return RoadType.Pedestrian;
                case "bike":
                    return RoadType.Bike;
                default:
                    return RoadType.Car;
            }
        }
    }
}