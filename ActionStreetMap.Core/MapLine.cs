using System;
using ActionStreetMap.Core.Geometry;

namespace ActionStreetMap.Core
{
    public struct MapLine
    {
        public Vector2d Start;
        public Vector2d End;

        public MapLine(Vector2d start, Vector2d end)
        {
            Start = start;
            End = end;
        }

        public override string ToString()
        {
            return String.Format("[({0},{1}) ({2},{3})]",
                Start.X, Start.Y, End.X, End.Y);
        }
    }
}
