﻿using ActionStreetMap.Core.Scene.World.Roads;

namespace ActionStreetMap.Models.Roads
{
    /// <summary>
    ///     Defines road style provider logic.
    /// </summary>
    public interface IRoadStyleProvider
    {
        /// <summary>
        ///     Gets road style for given road.
        /// </summary>
        /// <param name="road">Road.</param>
        /// <returns>Road style.</returns>
        RoadStyle Get(Road road);
    }
}