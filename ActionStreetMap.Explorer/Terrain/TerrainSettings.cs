using System.Collections.Generic;
using ActionStreetMap.Core.Tiling.Models;
using ActionStreetMap.Explorer.Scene.Roads;
using UnityEngine;

namespace ActionStreetMap.Explorer.Terrain
{
    /// <summary> Defines terrain settings. </summary>
    public class TerrainSettings
    {
        /// <summary> Tile object. </summary>
        public Tile Tile { get; set; }

        /// <summary> This is the control map that controls how the splat textures and details will be blended. </summary>
        public int Resolution = 512;

        /// <summary> A lower pixel error will draw terrain at a higher Level of detail but will be slower. </summary>
        public float PixelMapError = 5f;

        /// <summary> The distance at which the low res base map will be drawn. Decrease to increase performance. </summary>
        public float BaseMapDist = 750.0f;

        /// <summary> Splat list parameters. </summary>
        public List<List<string>> SplatParams { get; set; }

        /// <summary> Details list parameters. </summary>
        public List<List<string>> DetailParams { get; set; }

        /// <summary> Road style provider. </summary>
        public IRoadStyleProvider RoadStyleProvider { get; set; }

        /// <summary> ZIndex. </summary>
        public float ZIndex;

        /// <summary> Terrain center. </summary>
        public Vector2 CenterPosition;

        /// <summary> Left bottom corner. </summary>
        public Vector2 CornerPosition;
    }
}