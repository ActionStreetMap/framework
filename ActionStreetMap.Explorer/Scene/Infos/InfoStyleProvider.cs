using ActionStreetMap.Core.Scene.Infos;

namespace ActionStreetMap.Explorer.Scene.Infos
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
