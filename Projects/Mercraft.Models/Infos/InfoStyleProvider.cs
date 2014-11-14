﻿using ActionStreetMap.Core.Scene.World.Infos;

namespace ActionStreetMap.Models.Infos
{
    /// <summary>
    ///     Defines info style provider.
    /// </summary>
    public interface IInfoStyleProvider
    {
        /// <summary>
        ///     Gets style for given info.
        /// </summary>
        /// <param name="info">Info.</param>
        /// <returns>Style.</returns>
        InfoStyle Get(Info info);
    }
}
