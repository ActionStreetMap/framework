﻿using System.Collections.Generic;
using Mercraft.Core.MapCss.Domain;
using Mercraft.Models.Terrain;

namespace Mercraft.Explorer.Helpers
{
    internal static class TerrainRuleExtensions
    {
        public static List<List<string>> GetSplatParams(this Rule rule)
        {
            return rule.EvaluateList<List<string>>("splat");
        }

        public static List<List<string>> GetDetailParams(this Rule rule)
        {
            return rule.EvaluateList<List<string>>("detail");
        }

        public static int GetResolution(this Rule rule)
        {
            return rule.Evaluate<int>("resolution");
        }

        public static int GetHeightMapSize(this Rule rule)
        {
            return rule.Evaluate<int>("heightmapsize");
        }

        public static int GetPixelMapError(this Rule rule)
        {
            return rule.Evaluate<int>("pixelMapError");
        }

        public static int GetSplatIndex(this Rule rule)
        {
            return rule.Evaluate<int>("splat");
        }

        public static int GetTerrainDetailIndex(this Rule rule)
        {
            return rule.EvaluateDefault<int>("terrainDetail", AreaSettings.DefaultDetailIndex);
        }

        public static bool IsForest(this Rule rule)
        {
            return rule.EvaluateDefault<bool>("forest", false);
        }
    }
}
