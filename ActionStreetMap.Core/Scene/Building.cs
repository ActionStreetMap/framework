using System.Collections.Generic;
using ActionStreetMap.Core.Geometry;
using ActionStreetMap.Core.Scene.InDoor;
using ActionStreetMap.Core.Unity;

namespace ActionStreetMap.Core.Scene
{
    /// <summary> 
    ///     Represents building. See available OSM properties:
    ///     See http://wiki.openstreetmap.org/wiki/Buildings 
    /// </summary>
    public class Building
    {
        /// <summary> Gets or sets Id. </summary>
        public long Id { get; set; }

        /// <summary> Gets or sets game object wrapper which holds game engine specific classes. </summary>
        public IGameObject GameObject { get; set; }

        /// <summary> Gets or sets elevation. </summary>
        public float Elevation { get; set; }

        /// <summary> Gets or sets building footprint. </summary>
        public List<Vector2d> Footprint { get; set; }

        /// <summary> Contains floor plans. </summary>
        public List<Floor> FloorPlans { get; set; }

        /// <summary> True if building has windows. </summary>
        public bool HasWindows { get; set; }

        // NOTE OSM-available info 

        /// <summary> Gets or sets part flag. </summary>
        public bool IsPart { get; set; }

        #region Height specific

        /// <summary> Gets or sets height of building. </summary>
        public float Height { get; set; }

        /// <summary> Gets or sets gap between ground and terrain. </summary>
        public float MinHeight { get; set; }

        /// <summary> Gets or sets floor count. </summary>
        public int Levels { get; set; }

        #endregion

        #region Appearance

        /// <summary> Gets or sets texture atlas. </summary>
        public string TextureAtlas { get; set; }

        /// <summary> Gets or sets facade color. </summary>
        public string FacadeColor { get; set; }

        /// <summary> Gets or sets facade material. </summary>
        public string FacadeMaterial { get; set; }

        /// <summary> Gets or sets facade texture. </summary>
        public string FacadeTexture { get; set; }

        /// <summary> Gets or ses facade type </summary>
        public string FacadeType { get; set; }

        /// <summary> Gets or sets roof color. </summary>
        public string RoofColor { get; set; }

        /// <summary> Gets or sets roof material. </summary>
        public string RoofMaterial { get; set; }

        /// <summary> Gets or sets roof texture. </summary>
        public string RoofTexture { get; set; }

        /// <summary> Gets or sets roof type (see OSM roof types). </summary>
        public string RoofType { get; set; }

        /// <summary> Gets or sets roof height (see OSM roof types). </summary>
        public float RoofHeight { get; set; }

        /// <summary> Gets or sets front floor color. </summary>
        public string FloorFrontColor { get; set; }

        /// <summary> Gets or sets back floor color. </summary>
        public string FloorBackColor { get; set; }

        #endregion

        #region Characteristics

        /// <summary> Indicates that the building is used as a specific shop. </summary>
        public string Shop { get; set; }

        /// <summary> Describes what the building is used for, for example: school, theatre, bank. </summary>
        public string Amenity { get; set; }

        /// <summary> Ruins of buildings. </summary>
        public string Ruins { get; set; }

        /// <summary> For a building which has been abandoned by its owner and is no longer maintained. </summary>
        public string Abandoned { get; set; }

        #endregion
    }
}
