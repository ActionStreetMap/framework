using System;
using System.Collections.Generic;
using ActionStreetMap.Core.Scene.Buildings;
using ActionStreetMap.Core.Scene.Infos;
using ActionStreetMap.Core.Scene.Roads;
using ActionStreetMap.Explorer.Scene.Buildings;
using ActionStreetMap.Explorer.Scene.Infos;
using ActionStreetMap.Explorer.Scene.Roads;

namespace ActionStreetMap.Explorer.Themes
{
    /// <summary> Represents game theme. Bridge to style providers for different models. </summary>
    public class Theme
    {
        private readonly IBuildingStyleProvider _buildingStyleProvider;
        private readonly IRoadStyleProvider _roadStyleProvider;
        private readonly IInfoStyleProvider _infoStyleProvider;
        private readonly Dictionary<Type, object> _providers;

        /// <summary> Name of theme. </summary>
        public string Name { get; set; }

        /// <summary> Creates instance of <see cref="Theme"/>. </summary>
        /// <param name="buildingStyleProvider">Building style provider.</param>
        /// <param name="roadStyleProvider">Road style provider.</param>
        /// <param name="infoStyleProvider">Info style provider.</param>
        public Theme(IBuildingStyleProvider buildingStyleProvider, IRoadStyleProvider roadStyleProvider, 
            IInfoStyleProvider infoStyleProvider)
        {
            _buildingStyleProvider = buildingStyleProvider;
            _roadStyleProvider = roadStyleProvider;
            _infoStyleProvider = infoStyleProvider;
            _providers = new Dictionary<Type, object>
            {
                {typeof (IBuildingStyleProvider), _buildingStyleProvider},
                {typeof (IRoadStyleProvider), _roadStyleProvider},
                {typeof (IInfoStyleProvider), _infoStyleProvider}
            };
        }

        /// <summary> Gets building style. </summary>
        /// <param name="building">Building.</param>
        /// <returns>BuildingStyle.</returns>
        public BuildingStyle GetBuildingStyle(Building building)
        {
            return _buildingStyleProvider.Get(building);
        }

        /// <summary> Gets road style. </summary>
        /// <param name="road">Road.</param>
        /// <returns>RoadStyle.</returns>
        public RoadStyle GetRoadStyle(Road road)
        {
            return _roadStyleProvider.Get(road);
        }

        /// <summary> Gets info style. </summary>
        /// <param name="info">Info.</param>
        /// <returns>InfoStyle.</returns>
        public InfoStyle GetInfoStyle(Info info)
        {
            return _infoStyleProvider.Get(info);
        }

        /// <summary> Gets style provider by type. </summary>
        /// <typeparam name="T">Type.</typeparam>
        /// <returns>Style provider.</returns>
        public T GetStyleProvider<T>() where T:class
        {
            return (T) _providers[typeof(T)];
        }
    }
}
