using ActionStreetMap.Core.Unity;
using ActionStreetMap.Core.Utils;

namespace ActionStreetMap.Core.Tiling.Models
{
    /// <summary> Represents map tile. </summary>
    public class Tile : Model
    {
        /// <summary> Stores map center coordinate in lat/lon. </summary>
        public GeoCoordinate RelativeNullPoint { get; private set; }

        /// <summary> Stores tile center coordinate in Unity metrics. </summary>
        public MapPoint MapCenter { get; private set; }

        /// <summary> Gets width in meters. </summary>
        public float Width { get; private set; }

        /// <summary> Gets height in meters. </summary>
        public float Height { get; private set; }

        /// <summary> Gets or sets tile canvas. </summary>
        public Canvas Canvas { get; private set; }

        /// <summary> Gets bounding box for current tile. </summary>
        public BoundingBox BoundingBox { get; private set; }

        /// <summary> Gets or sets game object which is used to represent this tile. </summary>
        public IGameObject GameObject { get; set; }

        /// <summary> Gets ModelRegistry of given tile. </summary>
        public TileRegistry Registry { get; private set; }

        /// <summary> Gets map rectangle. </summary>
        public MapRectangle Rectangle { get; private set; }

        /// <inheritdoc />
        public override bool IsClosed { get { return false; } }

        /// <summary> Creates tile. </summary>
        /// <param name="relativeNullPoint">Relative null point.</param>
        /// <param name="mapCenter">Center of map.</param>
        /// <param name="canvas">Map canvas.</param>
        /// <param name="width">Tile width in meters.</param>
        /// <param name="height">Tile height in meters.</param>
        public Tile(GeoCoordinate relativeNullPoint, MapPoint mapCenter, Canvas canvas, 
            float width, float height)
        {
            RelativeNullPoint = relativeNullPoint;
            MapCenter = mapCenter;
            Canvas = canvas;

            Width = width;
            Height = height;

            var geoCenter = GeoProjection.ToGeoCoordinate(relativeNullPoint, mapCenter);
            BoundingBox = BoundingBox.CreateBoundingBox(geoCenter, width, height);

            Rectangle = new MapRectangle(MapCenter.X - width / 2, MapCenter.Y - height / 2, width, height);

            Registry = new TileRegistry();
        }

        /// <summary> Checks whether absolute position locates in tile with bound offset. </summary>
        /// <param name="position">Absolute position in game.</param>
        /// <param name="offset">offset from bounds.</param>
        /// <returns>Tres if position in tile</returns>
        public bool Contains(MapPoint position, float offset)
        {
            var rectangle = Rectangle;
            return (position.X > rectangle.TopLeft.X + offset) && (position.Y < rectangle.TopLeft.Y - offset) &&
                         (position.X < rectangle.BottomRight.X - offset) && (position.Y > rectangle.BottomRight.Y + offset);
        }

        /// <inheritdoc />
        public override void Accept(Tile tile, IModelLoader loader)
        {
            System.Diagnostics.Debug.Assert(tile == this);
            loader.PrepareTile(this);
        }
    }
}