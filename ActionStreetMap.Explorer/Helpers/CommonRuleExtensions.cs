using System;
using System.Collections.Generic;
using ActionStreetMap.Core.MapCss.Domain;
using ActionStreetMap.Core.Scene;
using ActionStreetMap.Explorer.Infrastructure;
using ActionStreetMap.Explorer.Scene.Builders;

namespace ActionStreetMap.Explorer.Helpers
{
    /// <summary> Provides methods for basic mapcss properties receiving. </summary>
    internal static class CommonRuleExtensions
    {
        public static float GetHeight(this Rule rule, float defaultValue = 0)
        {
            return rule.EvaluateDefault("height", defaultValue);
        }

        public static IEnumerable<IModelBuilder> GetModelBuilders(this Rule rule, BehaviourProvider provider)
        {
            foreach (var name in rule.EvaluateList<string>("builders"))
                yield return provider.GetBuilder(name);
        }

        /// <summary> Gets list of behaviours for the rule. </summary>
        public static IEnumerable<Type> GetModelBehaviours(this Rule rule, BehaviourProvider provider)
        {
            // TODO check performance impact
            foreach (var name in rule.EvaluateList<string>("behaviours"))
                yield return provider.GetBehaviour(name);
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