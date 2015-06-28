using System;
using System.Collections.Generic;
using System.Linq;
using ActionStreetMap.Core.MapCss;
using ActionStreetMap.Core.MapCss.Domain;
using ActionStreetMap.Core.Tiling;
using ActionStreetMap.Core.Tiling.Models;
using ActionStreetMap.Core.Unity;
using ActionStreetMap.Explorer.Helpers;
using ActionStreetMap.Explorer.Infrastructure;
using ActionStreetMap.Explorer.Scene.Builders;
using ActionStreetMap.Explorer.Scene.Terrain;
using ActionStreetMap.Infrastructure.Dependencies;
using ActionStreetMap.Infrastructure.Reactive;
using ActionStreetMap.Infrastructure.Utilities;
using ActionStreetMap.Maps.Data.Helpers;
using UnityEngine;

namespace ActionStreetMap.Explorer.Tiling
{
    /// <summary> Represents class responsible to process all models for tile. </summary>
    public class TileModelLoader : IModelLoader
    {
        private readonly ITerrainBuilder _terrainBuilder;
        private readonly BehaviourProvider _behaviourProvider;
        private readonly IObjectPool _objectPool;
        private readonly IModelBuilder[] _builders;
        private readonly IGameObjectFactory _gameObjectFactory;

        private readonly Stylesheet _stylesheet;

        /// <summary> Creates <see cref="TileModelLoader"/>. </summary>
        [Dependency]
        public TileModelLoader(IGameObjectFactory gameObjectFactory,
            ITerrainBuilder terrainBuilder, IStylesheetProvider stylesheetProvider,
            IEnumerable<IModelBuilder> builders, BehaviourProvider behaviourProvider,
            IObjectPool objectPool)
        {
            _terrainBuilder = terrainBuilder;
            _behaviourProvider = behaviourProvider;

            _objectPool = objectPool;
            _builders = builders.ToArray();

            _gameObjectFactory = gameObjectFactory;
            _stylesheet = stylesheetProvider.Get();
        }

        #region IModelLoader

        /// <inheritdoc />
        public void PrepareTile(Tile tile)
        {
            tile.GameObject = _gameObjectFactory.CreateNew(
                String.Format("tile_{0}", tile.RenderMode.ToString().ToLower()));
            Observable.Start((() => tile.GameObject.AddComponent(new GameObject())), 
                Scheduler.MainThread);
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
            var zoomLevel = ZoomHelper.GetZoomLevel(tile.RenderMode);
            var rule = _stylesheet.GetModelRule(model, zoomLevel);
            if (rule.IsApplicable && ShouldUseBuilder(tile, rule, model))
            {
                var modelBuilder = rule.GetModelBuilder(_builders);
                if (modelBuilder != null)
                {
                    var gameObject = func(rule, modelBuilder);
                    AttachBehaviours(gameObject, rule, model);
                }
            }
        }

        /// <inheritdoc />
        public void CompleteTile(Tile tile)
        {
            var canvas = tile.Canvas;
            var rule = _stylesheet.GetCanvasRule(canvas);

            var gameObject = _terrainBuilder.Build(tile, rule);

            AttachBehaviours(gameObject, rule, canvas);

            tile.Canvas.Dispose();
            _objectPool.Shrink();
        }

        private void AttachBehaviours(IGameObject gameObject, Rule rule, Model model)
        {
            var behaviourTypes = rule.GetModelBehaviours(_behaviourProvider);
            if (gameObject != null && !gameObject.IsBehaviourAttached && behaviourTypes.Any())
                Observable.Start(() =>
                {
                    foreach (var behaviourType in behaviourTypes)
                    {
                        var behaviour = gameObject.AddComponent<IModelBehaviour>(behaviourType);
                        if (behaviour != null)
                            behaviour.Apply(gameObject, model);
                    }
                }, Scheduler.MainThread);
        }

        private bool ShouldUseBuilder(Tile tile, Rule rule, Model model)
        {
            if (rule.IsSkipped())
            {
                tile.Registry.RegisterGlobal(model.Id);
#if DEBUG
                // Performance optimization: do not create in release
                _gameObjectFactory.CreateNew(String.Format("skip {0}", model), tile.GameObject);
#endif
                return false;
            }
            return true;
        }

        #endregion
    }
}
