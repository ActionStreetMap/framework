using ActionStreetMap.Core.MapCss.Domain;
using ActionStreetMap.Core.Scene;
using ActionStreetMap.Core.Tiling;
using ActionStreetMap.Explorer.Scene;

namespace ActionStreetMap.Explorer.Helpers
{
    /// <summary> Provides methods for basic mapcss properties receiving. </summary>
    internal static class CommonRuleExtensions
    {
        public static float GetHeight(this Rule rule, float defaultValue = 0)
        {
            return rule.EvaluateDefault("height", defaultValue);
        }
 
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

        /// <summary> Gets width. </summary>
        public static float GetWidth(this Rule rule)
        {
            return rule.Evaluate<float>("width");
        }

        /// <summary> Gets road type. </summary>
        public static RoadElement.RoadType GetRoadType(this Rule rule)
        {
            var typeStr = rule.Evaluate<string>("type");
            switch (typeStr)
            {
                case "pedestrian":
                    return RoadElement.RoadType.Pedestrian;
                case "bike":
                    return RoadElement.RoadType.Bike;
                default:
                    return RoadElement.RoadType.Car;
            }
        }
    }
}