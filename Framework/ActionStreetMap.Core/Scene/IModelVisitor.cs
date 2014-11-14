﻿using ActionStreetMap.Core.Scene.Models;

namespace ActionStreetMap.Core.Scene
{
    /// <summary>
    ///     Defines visitor interface for model types.
    /// </summary>
    public interface IModelVisitor
    {
        /// <summary>
        ///     Visits tile.
        /// </summary>
        /// <param name="tile">Tile.</param>
        void VisitTile(Tile tile);

        /// <summary>
        ///     Visits area.
        /// </summary>
        /// <param name="area">Area.</param>
        void VisitArea(Area area);

        /// <summary>
        ///     Visits way.
        /// </summary>
        /// <param name="way">Way.</param>
        void VisitWay(Way way);

        /// <summary>
        ///     Visits node.
        /// </summary>
        /// <param name="node">Node.</param>
        void VisitNode(Node node);

        /// <summary>
        ///     Visits canvas.
        /// </summary>
        /// <param name="canvas">Canvas.</param>
        void VisitCanvas(Canvas canvas);
    }
}
