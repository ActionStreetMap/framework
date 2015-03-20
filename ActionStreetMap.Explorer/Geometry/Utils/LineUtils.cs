using System.Collections.Generic;
using ActionStreetMap.Core;

namespace ActionStreetMap.Explorer.Geometry.Utils
{
    internal static class LineUtils
    {
        #region Intermediate points

        public static List<MapPoint> DividePolyline(IElevationProvider elevationProvider,
            List<MapPoint> original, float maxDistance)
        {
            return DividePolyline(elevationProvider, original, maxDistance, 5f);
        }

        public static List<MapPoint> DividePolyline(IElevationProvider elevationProvider,
            List<MapPoint> original, float maxDistance, float threshold)
        {
            var result = new List<MapPoint>(original.Count);
            for (int i = 1; i < original.Count; i++)
            {
                var point1 = original[i - 1];
                var point2 = original[i];

                point1.Elevation = elevationProvider.GetElevation(point1);
                result.Add(point1);

                var distance = point1.DistanceTo(point2);
                while (distance > maxDistance)
                {
                    var ration = maxDistance / distance;
                    point1 = new MapPoint(
                        point1.X + ration * (point2.X - point1.X),
                        point1.Y + ration * (point2.Y - point1.Y));

                    point1.Elevation = elevationProvider.GetElevation(point1);

                    distance = point1.DistanceTo(point2);
                    // we should prevent us to have small distances between points when we have turn
                    if (distance < threshold)
                        break;

                    result.Add(point1);
                }

            }
            // add last as we checked previous item in cycle
            var last = original[original.Count - 1];
            last.Elevation = elevationProvider.GetElevation(last);
            result.Add(last);
            return result;
        }

        public static void DivideLine(IElevationProvider elevationProvider, MapPoint start,
            MapPoint end, List<MapPoint> result, float maxDistance)
        {
            var point1 = start;
            var point2 = end;

            point1.Elevation = elevationProvider.GetElevation(point1);
            result.Add(point1);

            var distance = point1.DistanceTo(point2);
            while (distance > maxDistance)
            {
                var ration = maxDistance / distance;
                point1 = new MapPoint(
                    point1.X + ration * (point2.X - point1.X),
                    point1.Y + ration * (point2.Y - point1.Y));

                point1.Elevation = elevationProvider.GetElevation(point1);
                distance = point1.DistanceTo(point2);
                result.Add(point1);
            }

            end.Elevation = elevationProvider.GetElevation(end);
            result.Add(end);
        }

        public static MapPoint GetNextIntermediatePoint(IElevationProvider elevationProvider, MapPoint point1,
            MapPoint point2, float maxDistance)
        {
            var distance = point1.DistanceTo(point2);
            if (distance > maxDistance)
            {
                var ration = maxDistance / distance;
                var next = new MapPoint(point1.X + ration * (point2.X - point1.X),
                            point1.Y + ration * (point2.Y - point1.Y));

                next.Elevation = elevationProvider.GetElevation(point1);
                return next;
            }

            return point2; // NOTE should we return point2?
        }

        #endregion
    }
}
