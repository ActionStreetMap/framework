using System.Collections.Generic;

namespace ActionStreetMap.Core.Tiling.Models
{
    /// <summary> Represents a set of connected points. For example, used for roads. </summary>
    public class Way: Model
    {
        /// <summary> Gets or sets geo coordinates for this model. </summary>
        public List<GeoCoordinate> Points { get; set; }

        /// <inheritdoc />
        public override bool IsClosed { get { return Points[0] == Points[Points.Count - 1]; } }

        /// <inheritdoc />
        public override void Accept(Tile tile, IModelLoader loader)
        {
            loader.LoadWay(tile, this);
        }
    }
}
