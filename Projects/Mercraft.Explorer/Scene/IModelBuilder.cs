﻿using Mercraft.Core.MapCss.Domain;
using Mercraft.Core.Scene.Models;
using Mercraft.Core.Unity;
using Mercraft.Core.World;
using Mercraft.Explorer.Themes;
using Mercraft.Infrastructure.Dependencies;
using Mercraft.Infrastructure.Diagnostic;
using Mercraft.Infrastructure.Utilities;
using Mercraft.Models.Utils;

namespace Mercraft.Explorer.Scene
{
    /// <summary>
    ///     Defines model builder logic.
    /// </summary>
    public interface IModelBuilder
    {
        /// <summary>
        ///     Name of model builder.
        /// </summary>
        string Name { get; }

        /// <summary>
        ///     Builds model from area.
        /// </summary>
        /// <param name="tile">Tile.</param>
        /// <param name="rule">Rule.</param>
        /// <param name="area">Area.</param>
        /// <returns>Game object wrapper.</returns>
        IGameObject BuildArea(Tile tile, Rule rule, Area area);

        /// <summary>
        ///     Builds model from way.
        /// </summary>
        /// <param name="tile">Tile.</param>
        /// <param name="rule">Rule.</param>
        /// <param name="way">Way.</param>
        /// <returns>Game object wrapper.</returns>
        IGameObject BuildWay(Tile tile, Rule rule, Way way);

        /// <summary>
        ///     Builds model from node.
        /// </summary>
        /// <param name="tile">Tile.</param>
        /// <param name="rule">Rule.</param>
        /// <param name="node">Node.</param>
        /// <returns>Game object wrapper.</returns>
        IGameObject BuildNode(Tile tile, Rule rule, Node node);
    }

    /// <summary>
    ///     Defines base class for model builders which provides helper logic.
    /// </summary>
    public abstract class ModelBuilder : IModelBuilder
    {
        /// <inheritdoc />
        public abstract string Name { get; }

        #region Properties. These properties are public due to Reflection limitations on some platform

        /// <summary>
        ///     Trace.
        /// </summary>
        [Dependency]
        public ITrace Trace { get; set; }

        /// <summary>
        ///     World manager.
        /// </summary>
        [Dependency]
        public WorldManager WorldManager { get; set; }

        /// <summary>
        ///     Game object factory.
        /// </summary>
        [Dependency]
        public IGameObjectFactory GameObjectFactory { get; set; }

        /// <summary>
        ///     Theme provider.
        /// </summary>
        [Dependency]
        public IThemeProvider ThemeProvider { get; set; }

        /// <summary>
        ///     Resource provider.
        /// </summary>
        [Dependency]
        public IResourceProvider ResourceProvider { get; set; }

        /// <summary>
        ///     Object pool.
        /// </summary>
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
    }
}
