using ActionStreetMap.Core.Scene;
using ActionStreetMap.Core.Tiling;
using ActionStreetMap.Maps.Data;

namespace ActionStreetMap.Explorer.Tiling
{
    /// <summary> Provides the way to edit tile models. </summary>
    public interface ITileModelEditor
    {
        /// <summary> Adds building to current scene. </summary>
        void AddBuilding(Building building);
    }

    /// <summary> Default implementation of <see cref="ITileModelEditor"/>. </summary>
    internal sealed class TileModelEditor : ITileModelEditor
    {
        private readonly ITileController _tileController;
        private readonly IElementSourceEditor _elementSourceEditor;

        /// <summary> Creates instance of <see cref="TileModelEditor"/>. </summary>
        /// <param name="tileController">Tile controller. </param>
        /// <param name="elementSourceEditor">Element source editor.</param>
        public TileModelEditor(ITileController tileController, IElementSourceEditor elementSourceEditor)
        {
            _tileController = tileController;
            _elementSourceEditor = elementSourceEditor;
        }

        /// <inheritdoc />
        public void AddBuilding(Building building)
        {
            throw new System.NotImplementedException();
        }
    }
}
