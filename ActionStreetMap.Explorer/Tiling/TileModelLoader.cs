using System;
using System.Collections.Generic;
using System.Linq;
using ActionStreetMap.Core.MapCss;
using ActionStreetMap.Core.MapCss.Domain;
using ActionStreetMap.Core.Tiling;
using ActionStreetMap.Core.Tiling.Models;
using ActionStreetMap.Core.Unity;
using ActionStreetMap.Explorer.Helpers;
using ActionStreetMap.Explorer.Scene;
using ActionStreetMap.Explorer.Scene.Terrain;
using ActionStreetMap.Infrastructure.Dependencies;
using ActionStreetMap.Infrastructure.Reactive;
using ActionStreetMap.Infrastructure.Utilities;
using UnityEngine;

namespace ActionStreetMap.Explorer.Tiling
{
    /// <summary> Represents class responsible to process all models for tile. </summary>
    public class TileModelLoader : IModelLoader
    {
        private readonly ITerrainBuilder _terrainBuilder;
        private readonly IObjectPool _objectPool;
        private readonly IModelBuilder[] _builders;
        private readonly IModelBehaviour[] _behaviours;
        private readonly IGameObjectFactory _gameObjectFactory;

        private readonly Stylesheet _stylesheet;

        /// <summary> Creates <see cref="TileModelLoader"/>. </summary>
        [Dependency]
        public TileModelLoader(IGameObjectFactory gameObjectFactory,
            ITerrainBuilder terrainBuilder, IStylesheetProvider stylesheetProvider,
            IEnumerable<IModelBuilder> builders, IEnumerable<IModelBehaviour> behaviours,
            IObjectPool objectPool)
        {
            _terrainBuilder = terrainBuilder;

            _objectPool = objectPool;
            _builders = builders.ToArray();
            _behaviours = behaviours.ToArray();
            _gameObjectFactory = gameObjectFactory;
            _stylesheet = stylesheetProvider.Get();
        }

        #region IModelLoader

        /// <inheritdoc />
        public void PrepareTile(Tile tile)
        {
            tile.GameObject = _gameObjectFactory.CreateNew("tile");
            Scheduler.MainThread.Schedule(() => tile.GameObject.AddComponent(new GameObject()));
        }

        /// <inheritdoc />
        public void LoadRelation(Tile tile, Relation relation)
        {
            if (relation.Areas != null)
                foreach (var area in relation.Areas)
                    LoadArea(tile, area);
        }

        /// <inheritdoc />
        public void LoadArea(Tile tile, Area area)
        {
            LoadModel(tile, area, (rule, modelBuilder) =>
            {
                var result = modelBuilder.BuildArea(tile, rule, area);
                _objectPool.StoreList(area.Points);
                return result;
            });
        }

        /// <inheritdoc />
        public void LoadWay(Tile tile, Way way)
        {
            LoadModel(tile, way, (rule, modelBuilder) =>
            {
                var result = modelBuilder.BuildWay(tile, rule, way);
                _objectPool.StoreList(way.Points);
                return result;
            });
        }

        /// <inheritdoc />
        public void LoadNode(Tile tile, Node node)
        {
            LoadModel(tile, node, (rule, modelBuilder) => modelBuilder.BuildNode(tile, rule, node));
        }

        private void LoadModel(Tile tile, Model model, Func<Rule, IModelBuilder, IGameObject> func)
        {
            var rule = _stylesheet.GetModelRule(model);
            if (ShouldUseBuilder(tile, rule, model))
            {
                var modelBuilder = rule.GetModelBuilder(_builders);
                if (modelBuilder != null)
                {
                    var gameObject = func(rule, modelBuilder);
                    Scheduler.MainThread.Schedule(() => AttachExtras(gameObject, tile, rule, model));
                }
            }
        }

        /// <inheritdoc />
        public void CompleteTile(Tile tile)
        {
            var rule = _stylesheet.GetCanvasRule(tile.Canvas);

            _terrainBuilder.Build(tile, rule);

            tile.Canvas.Dispose();
            _objectPool.Shrink();
        }

        private bool ShouldUseBuilder(Tile tile, Rule rule, Model model)
        {
            if (!rule.IsApplicable)
                return false;

            if (rule.IsSkipped())
            {
#if DEBUG
                // Performance optimization: do not create in release
                _gameObjectFactory.CreateNew(String.Format("skip {0}", model), tile.GameObject);
#endif
                return false;
            }
            return true;
        }

        private void AttachExtras(IGameObject gameObject, Tile tile, Rule rule, Model model)
        {
            if (gameObject != null)
            {
                gameObject.Parent = tile.GameObject;
                var behaviour = rule.GetModelBehaviour(_behaviours);
                if (behaviour != null)
                    behaviour.Apply(gameObject, model);
            }
        }

        #endregion
    }
}
