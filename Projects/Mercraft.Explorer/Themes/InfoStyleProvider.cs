﻿using System.Collections.Generic;
using Mercraft.Core.World.Infos;
using Mercraft.Models.Infos;

namespace Mercraft.Explorer.Themes
{
    /// <summary>
    ///     Default info style provider which uses key-value map.
    /// </summary>
    public class InfoStyleProvider: IInfoStyleProvider
    {
        private readonly Dictionary<string, InfoStyle> _infoStyleMap;

        /// <summary>
        ///     Creates InfoStyleProvider.
        /// </summary>
        /// <param name="infoStyleMap">InfoStyle map.</param>
        public InfoStyleProvider(Dictionary<string, InfoStyle> infoStyleMap)
        {
            _infoStyleMap = infoStyleMap;
        }

        /// <inheritdoc />
        public InfoStyle Get(Info info)
        {
            return _infoStyleMap[info.Key];
        }
    }
}
