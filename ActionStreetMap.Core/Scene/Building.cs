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
        /// <summary> Id. </summary>
        public long Id;

        /// <summary> Game object wrapper which holds game engine specific classes. </summary>
        public IGameObject GameObject;

        /// <summary> Elevation. </summary>
        public float Elevation;

        /// <summary> Building footprint. </summary>
        public List<Vector2d> Footprint;

        /// <summary> Contains floor plans. </summary>
        public List<Floor> FloorPlans;

        /// <summary> True if building has windows. </summary>
        public bool HasWindows;

        // NOTE OSM-available info 

        /// <summary> Part flag. </summary>
        public bool IsPart;

        #region Height specific

        /// <summary> Height of building. </summary>
        public float Height;

        /// <summary> Gap between terrain and building. </summary>
        public float MinHeight;

        /// <summary> Floor count. </summary>
        public int Levels;

        #endregion

        #region Appearance

        /// <summary> Facade color. </summary>
        public string FacadeColor;

        /// <summary> Facade material. </summary>
        public string FacadeMaterial;

        /// <summary> Facade texture. </summary>
        public string FacadeTexture;

        /// <summary> Facade type </summary>
        public string FacadeType;

        /// <summary> Roof color. </summary>
        public string RoofColor;

        /// <summary> Roof material. </summary>
        public string RoofMaterial;

        /// <summary> Roof texture. </summary>
        public string RoofTexture;

        /// <summary> Roof type (see OSM roof types). </summary>
        public string RoofType;

        /// <summary> Toof height (see OSM roof types). </summary>
        public float RoofHeight;

        /// <summary> Front floor color. </summary>
        public string FloorFrontColor;

        /// <summary> Front floor texture. </summary>
        public string FloorFrontTexture;

        /// <summary> Back floor color. </summary>
        public string FloorBackColor;

        /// <summary> Back floor texture. </summary>
        public string FloorBackTexture;

        #endregion

        #region Characteristics

        /// <summary> Indicates that the building is used as a specific shop. </summary>
        public string Shop;

        /// <summary> Describes what the building is used for, for example: school, theatre, bank. </summary>
        public string Amenity;

        /// <summary> Ruins of buildings. </summary>
        public string Ruins;

        /// <summary> For a building which has been abandoned by its owner and is no longer maintained. </summary>
        public string Abandoned;

        #endregion
    }
}
