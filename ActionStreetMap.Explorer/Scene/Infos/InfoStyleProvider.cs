using System.Collections.Generic;
using ActionStreetMap.Core.Scene.Infos;

namespace ActionStreetMap.Explorer.Scene.Infos
{
    /// <summary> Defines info style provider. </summary>
    public interface IInfoStyleProvider
    {
        /// <summary> Gets style for given info. </summary>
        /// <param name="info">Info.</param>
        /// <returns>Style.</returns>
        InfoStyle Get(Info info);
    }

    /// <summary> Default info style provider which uses key-value map. </summary>
    internal class InfoStyleProvider : IInfoStyleProvider
    {
        private readonly Dictionary<string, InfoStyle> _infoStyleMap;

        /// <summary> Creates InfoStyleProvider. </summary>
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
