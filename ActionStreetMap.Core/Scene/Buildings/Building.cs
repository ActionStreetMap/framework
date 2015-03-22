using System.Collections.Generic;
using ActionStreetMap.Core.Unity;

namespace ActionStreetMap.Core.Scene.Buildings
{
    /// <summary>
    ///     Represents building. See available OSM properties: http://wiki.openstreetmap.org/wiki/Buildings
    /// </summary>
    public class Building
    {
        /// <summary> Gets or sets Id. </summary>
        public long Id { get; set; }

        /// <summary> Gets or sets address. </summary>
        public Address Address { get; set; }

        /// <summary> Gets or sets game object wrapper which holds game engine specific classes. </summary>
        public IGameObject GameObject { get; set; }

        /// <summary> Gets or sets elevation. </summary>
        public float Elevation { get; set; }

        /// <summary> Gets or sets building footprint. </summary>
        public List<MapPoint> Footprint { get; set; }

        // NOTE OSM-available info 

        /// <summary> Gets or sets type of building. </summary>
        public string Type { get; set; }

        #region Height specific

        /// <summary> Gets or sets height of building. </summary>
        public float Height { get; set; }

        /// <summary> Gets or sets gap between ground and terrain. </summary>
        public float MinHeight { get; set; }

        /// <summary> Gets or sets floor count. </summary>
        public int Levels { get; set; }

        #endregion

        #region Appearance

        /// <summary> Gets or sets facade color. </summary>
        public Color32 FacadeColor { get; set; }

        /// <summary> Gets or sets facade material. </summary>
        public string FacadeMaterial { get; set; }

        /// <summary> Gets or ses facade type </summary>
        public string FacadeType { get; set; }

        /// <summary> Gets or sets roof color. </summary>
        public Color32 RoofColor { get; set; }

        /// <summary> Gets or sets roof material. </summary>
        public string RoofMaterial { get; set; }

        /// <summary> Gets or sets roof type (see OSM roof types). </summary>
        public string RoofType { get; set; }

        /// <summary> Gets or sets roof height (see OSM roof types). </summary>
        public float RoofHeight { get; set; }

        #endregion

        #region Characteristics

        /// <summary> Indicates that the building is used as a specific shop. </summary>
        public string Shop { get; set; }

        /// <summary> Describes what the building is used for, for example: school, theatre, bank. </summary>
        public string Amenity { get; set; }

        /// <summary>  Ruins of buildings. </summary>
        public string Ruins { get; set; }

        /// <summary> For a building which has been abandoned by its owner and is no longer maintained. </summary>
        public string Abandoned { get; set; }

        #endregion
    }
}
