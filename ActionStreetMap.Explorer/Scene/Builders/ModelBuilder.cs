using System;
using ActionStreetMap.Core;
using ActionStreetMap.Core.MapCss.Domain;
using ActionStreetMap.Core.Tiling.Models;
using ActionStreetMap.Core.Unity;
using ActionStreetMap.Explorer.Scene.Utils;
using ActionStreetMap.Infrastructure.Dependencies;
using ActionStreetMap.Infrastructure.Diagnostic;
using ActionStreetMap.Infrastructure.Utilities;

namespace ActionStreetMap.Explorer.Scene.Builders
{
    /// <summary> Defines model builder logic. </summary>
    public interface IModelBuilder
    {
        /// <summary> Name of model builder. </summary>
        string Name { get; }

        /// <summary> Builds model from area. </summary>
        /// <param name="tile">Tile.</param>
        /// <param name="rule">Rule.</param>
        /// <param name="area">Area.</param>
        /// <returns>Game object wrapper.</returns>
        IGameObject BuildArea(Tile tile, Rule rule, Area area);

        /// <summary> Builds model from way. </summary>
        /// <param name="tile">Tile.</param>
        /// <param name="rule">Rule.</param>
        /// <param name="way">Way.</param>
        /// <returns>Game object wrapper.</returns>
        IGameObject BuildWay(Tile tile, Rule rule, Way way);

        /// <summary> Builds model from node. </summary>
        /// <param name="tile">Tile.</param>
        /// <param name="rule">Rule.</param>
        /// <param name="node">Node.</param>
        /// <returns>Game object wrapper.</returns>
        IGameObject BuildNode(Tile tile, Rule rule, Node node);
    }

    /// <summary> Defines base class for model builders which provides helper logic. </summary>
    public abstract class ModelBuilder : IModelBuilder
    {
        /// <inheritdoc />
        public abstract string Name { get; }

        #region Properties. These properties are public due to Reflection limitations on some platform

        /// <summary> Gets trace. </summary>
        [Dependency]
        public ITrace Trace { get; set; }

        /// <summary> Gets elevation provider. </summary>
        [Dependency]
        public IElevationProvider ElevationProvider { get; set; }

        /// <summary> Game object factory. </summary>
        [Dependency]
        public IGameObjectFactory GameObjectFactory { get; set; }

        /// <summary> Gets theme provider. </summary>
        [Dependency]
        public IThemeProvider ThemeProvider { get; set; }

        /// <summary> Gets resource provider. </summary>
        [Dependency]
        public IResourceProvider ResourceProvider { get; set; }

        /// <summary> Gets object pool. </summary>
        [Dependency]
        public IObjectPool ObjectPool { get; set; }

        #endregion

        /// <inheritdoc />
        public virtual IGameObject BuildArea(Tile tile, Rule rule, Area area)
        {
            //Trace.Normal(String.Format("{0}: building area {1}", Name, area.Id));
            return null;
        }

        /// <inheritdoc />
        public virtual IGameObject BuildWay(Tile tile, Rule rule, Way way)
        {
            //Trace.Normal(String.Format("{0}: building way {1}", Name, way.Id));
            return null;
        }

        /// <inheritdoc />
        public virtual IGameObject BuildNode(Tile tile, Rule rule, Node node)
        {
            //Trace.Normal(String.Format("{0}: building node {1}", Name, node.Id));
            return null;
        }

        /// <summary> Returns name of game object. </summary>
        /// <param name="model">Model.</param>
        /// <returns>Name of game object.</returns>
        protected string GetName(Model model)
        {
            // NOTE this is performance optimization for release mode only
#if DEBUG
            return String.Format("{0} {1}", Name, model);
#else
            return null;
#endif
        }
    }
}
