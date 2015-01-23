using System;
using System.Collections.Generic;
using System.Linq;
using ActionStreetMap.Core.Elevation;
using ActionStreetMap.Core.MapCss;
using ActionStreetMap.Core.MapCss.Domain;
using ActionStreetMap.Core.Tiling;
using ActionStreetMap.Core.Tiling.Models;
using ActionStreetMap.Core.Unity;
using ActionStreetMap.Explorer.Helpers;
using ActionStreetMap.Explorer.Scene;
using ActionStreetMap.Explorer.Themes;
using ActionStreetMap.Infrastructure.Dependencies;
using ActionStreetMap.Infrastructure.Reactive;
using ActionStreetMap.Infrastructure.Utilities;
using ActionStreetMap.Explorer.Scene.Roads;
using ActionStreetMap.Explorer.Scene.Terrain;
using ActionStreetMap.Explorer.Scene.Utils;
using UnityEngine;

namespace ActionStreetMap.Explorer.Tiling
{
    /// <summary>
    ///     Represents class responsible to process all models for tile.
    /// </summary>
    public class TileModelLoader : IModelVisitor
    {
        private readonly IHeightMapProvider _heighMapProvider;
        private readonly ITerrainBuilder _terrainBuilder;
        private readonly IObjectPool _objectPool;
        private readonly IModelBuilder[] _builders;
        private readonly IModelBehaviour[] _behaviours;
        private readonly IGameObjectFactory _gameObjectFactory;
        private readonly IThemeProvider _themeProvider;
        private readonly HeightMapProcessor _heightMapProcessor;
        private readonly Stylesheet _stylesheet;

        private Tile _tile;

        /// <summary>
        ///     Creates <see cref="TileModelLoader"/>
        /// </summary>
        [Dependency]
        public TileModelLoader(IGameObjectFactory gameObjectFactory, IThemeProvider themeProvider,
            IHeightMapProvider heighMapProvider, ITerrainBuilder terrainBuilder, IStylesheetProvider stylesheetProvider,
            IEnumerable<IModelBuilder> builders, IEnumerable<IModelBehaviour> behaviours,
            IObjectPool objectPool, HeightMapProcessor heightMapProcessor)
        {
            _heighMapProvider = heighMapProvider;
            _terrainBuilder = terrainBuilder;

            _objectPool = objectPool;
            _builders = builders.ToArray();
            _behaviours = behaviours.ToArray();
            _gameObjectFactory = gameObjectFactory;
            _themeProvider = themeProvider;
            _heightMapProcessor = heightMapProcessor;
            _stylesheet = stylesheetProvider.Get();
        }

        #region IModelVisitor

        /// <inheritdoc />
        public void VisitTile(Tile tile)
        {
            _tile = tile;
            _tile.GameObject = _gameObjectFactory.CreateNew("tile");
            Scheduler.MainThread.Schedule(() => _tile.GameObject.AddComponent(new GameObject()));
            _heightMapProcessor.Recycle(_tile.HeightMap);
        }

        /// <inheritdoc />
        public void VisitRelation(Relation relation)
        {
            if (relation.Areas != null)
                foreach (var area in relation.Areas)
                    VisitArea(area);
        }

        /// <inheritdoc />
        public void VisitArea(Area area)
        {
            var rule = _stylesheet.GetModelRule(area);
            if (ShouldUseBuilder(rule, area))
            {
                var modelBuilder = rule.GetModelBuilder(_builders);
                if (modelBuilder != null)
                {
                    var gameObject = modelBuilder.BuildArea(_tile, rule, area);
                    Scheduler.MainThread.Schedule(() => AttachExtras(gameObject, rule, area));
                }

            }
            _objectPool.StoreList(area.Points);
        }

        /// <inheritdoc />
        public void VisitWay(Way way)
        {
            var rule = _stylesheet.GetModelRule(way);
            if (ShouldUseBuilder(rule, way))
            {
                var modelBuilder = rule.GetModelBuilder(_builders);
                if (modelBuilder != null)
                {
                    var gameObject = modelBuilder.BuildWay(_tile, rule, way);
                    Scheduler.MainThread.Schedule(() => AttachExtras(gameObject, rule, way));
                }
            }
            _objectPool.StoreList(way.Points);
        }

        /// <inheritdoc />
        public void VisitNode(Node node)
        {
            var rule = _stylesheet.GetModelRule(node);
            if (ShouldUseBuilder(rule, node))
            {
                var modelBuilder = rule.GetModelBuilder(_builders);
                if (modelBuilder != null)
                {
                    var gameObject = modelBuilder.BuildNode(_tile, rule, node);
                    Scheduler.MainThread.Schedule(() => AttachExtras(gameObject, rule, node));
                }
            }
        }

        /// <inheritdoc />
        public void VisitCanvas(Canvas canvas)
        {
            var rule = _stylesheet.GetCanvasRule(canvas);

            _terrainBuilder.Build(_tile.GameObject, new TerrainSettings
            {
                Tile = _tile,
                Resolution = rule.GetResolution(),
                CenterPosition = new Vector2(_tile.MapCenter.X, _tile.MapCenter.Y),
                CornerPosition = new Vector2(_tile.BottomLeft.X, _tile.BottomLeft.Y),
                PixelMapError = rule.GetPixelMapError(),
                ZIndex = rule.GetZIndex(),
                SplatParams = rule.GetSplatParams(),
                DetailParams = rule.GetDetailParams(),
                RoadStyleProvider = _themeProvider.Get().GetStyleProvider<IRoadStyleProvider>()
            });

            // NOTE schedule cleanup on UI thread as data may be used
            Scheduler.MainThread.Schedule(() =>
            {
                _heightMapProcessor.Clear();
                _heighMapProvider.Store(_tile.HeightMap);
                _tile.HeightMap = null;
            });

            _objectPool.Shrink();
        }

        private bool ShouldUseBuilder(Rule rule, Model model)
        {
            if (!rule.IsApplicable)
                return false;

            if (rule.IsSkipped())
            {
                _gameObjectFactory.CreateNew(String.Format("skip {0}", model), _tile.GameObject);
                return false;
            }
            return true;
        }

        private void AttachExtras(IGameObject gameObject, Rule rule, Model model)
        {
            if (gameObject != null)
            {
                gameObject.Parent = _tile.GameObject;
                var behaviour = rule.GetModelBehaviour(_behaviours);
                if (behaviour != null)
                    behaviour.Apply(gameObject, model);
            }
        }

        #endregion
    }
}
