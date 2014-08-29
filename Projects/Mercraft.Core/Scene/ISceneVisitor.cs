﻿using Mercraft.Core.MapCss.Domain;
using Mercraft.Core.Scene.Models;
using Mercraft.Core.Unity;

namespace Mercraft.Core.Scene
{
    /// <summary>
    ///     Represents behavior which builds game objects from given models and rules
    /// </summary>
    public interface ISceneVisitor
    {
        void Prepare(IScene scene, Stylesheet stylesheet);
        void Finalize(IScene scene);

        bool VisitCanvas(GeoCoordinate center, IGameObject parent, Rule rule, Canvas canvas, bool visitedBefore);
        bool VisitArea(GeoCoordinate center, IGameObject parent, Rule rule, Area area, bool visitedBefore);
        bool VisitWay(GeoCoordinate center, IGameObject parent, Rule rule, Way way, bool visitedBefore);
    }
}