using System.Collections.Generic;

namespace ActionStreetMap.Core.Tiling.Models
{
    /// <summary> Represents connected polygon. Used for buildings, park areas, etc. </summary>
    public class Area : Model
    {
        /// <summary> Gets or sets geo coordinates for this model. </summary>
        public List<GeoCoordinate> Points { get; set; }

        /// <summary> Gets or sets points for holes inside this polygon. </summary>
        public List<List<GeoCoordinate>> Holes { get; set; }

        /// <inheritdoc />
        public override bool IsClosed { get { return Points.Count > 2 && Points[0] == Points[Points.Count - 1]; } }

        /// <inheritdoc />
        public override void Accept(Tile tile, IModelLoader loader)
        {
            loader.LoadArea(tile, this);
        }
    }
}