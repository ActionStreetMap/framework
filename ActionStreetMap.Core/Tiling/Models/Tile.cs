using ActionStreetMap.Core.Geometry;
using ActionStreetMap.Core.Unity;
using ActionStreetMap.Core.Utils;

namespace ActionStreetMap.Core.Tiling.Models
{
    /// <summary> Represents map tile. </summary>
    public class Tile : Model
    {
        /// <summary> Stores map center coordinate in lat/lon. </summary>
        public GeoCoordinate RelativeNullPoint { get; private set; }

        /// <summary> Stores tile center coordinate in world coordinates. </summary>
        public Vector2d MapCenter { get; private set; }

        /// <summary> Render mode. </summary>
        public RenderMode RenderMode { get; private set; }

        /// <summary> Gets or sets tile canvas. </summary>
        public Canvas Canvas { get; private set; }

        /// <summary> Gets bounding box for current tile. </summary>
        public BoundingBox BoundingBox { get; private set; }

        /// <summary> Gets or sets game object which is used to represent this tile. </summary>
        public IGameObject GameObject { get; set; }

        /// <summary> Gets ModelRegistry of given tile. </summary>
        internal TileRegistry Registry { get; private set; }

        /// <summary> Gets map rectangle. </summary>
        public Rectangle2d Rectangle { get; private set; }

        /// <inheritdoc />
        public override bool IsClosed { get { return false; } }

        /// <summary> Creates tile. </summary>
        /// <param name="relativeNullPoint">Relative null point.</param>
        /// <param name="mapCenter">Center of map.</param>
        /// <param name="renderMode">Render mode.</param>
        /// <param name="canvas">Map canvas.</param>
        /// <param name="width">Tile width in meters.</param>
        /// <param name="height">Tile height in meters.</param>
        public Tile(GeoCoordinate relativeNullPoint, Vector2d mapCenter, RenderMode renderMode,
            Canvas canvas, double width, double height)
        {
            RelativeNullPoint = relativeNullPoint;
            MapCenter = mapCenter;
            RenderMode = renderMode;
            Canvas = canvas;

            var geoCenter = GeoProjection.ToGeoCoordinate(relativeNullPoint, mapCenter);
            BoundingBox = BoundingBox.Create(geoCenter, width, height);

            Rectangle = new Rectangle2d(MapCenter.X - width / 2, MapCenter.Y - height / 2, width, height);

            Registry = new TileRegistry(renderMode);
        }

        /// <summary> Checks whether absolute position locates in tile with bound offset. </summary>
        /// <param name="position">Absolute position in game.</param>
        /// <param name="offset">offset from bounds.</param>
        /// <returns>Tres if position in tile</returns>
        public bool Contains(Vector2d position, double offset)
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