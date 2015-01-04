using System;
using System.Collections.Generic;
using System.Linq;
using ActionStreetMap.Infrastructure.Primitives;

namespace ActionStreetMap.Core.Scene.World.Roads
{
    /// <summary>
    ///     Represents road graph of concrete tile.
    /// </summary>
    public sealed class RoadGraph
    {
        internal const float Offset = 2;
        
        // road elements
        private readonly List<RoadElement> _elements = new List<RoadElement>(64);

        // key is point which is shared between different road elements ("junction")
        private readonly Dictionary<MapPoint, RoadJunction> _junctionsMap = new Dictionary<MapPoint, RoadJunction>(32);

        // point to index in RoadElement/index in Element.Points tuple
        private readonly Dictionary<MapPoint, Tuple<RoadElement, int>> _pointsMap = new Dictionary<MapPoint, Tuple<RoadElement, int>>(256);

        #region Public methods

        /// <summary>
        ///     Gets collection of processed road elements (graph edges).
        /// </summary>
        public IEnumerable<RoadElement> Elements { get { return _elements; } }

        /// <summary>
        ///     Gets collection of detected junctions (graph vertices).
        /// </summary>
        public IEnumerable<RoadJunction> Junctions { get { return _junctionsMap.Values; } }


        public void Add(RoadElement element)
        {
            //Contract.Requires(element != null);
            //Contract.Requires(element.Points.Count > 1, "Cannot add road element with less that 2 points!");

            _elements.Add(element);

            // TODO ignore for non-car types
            if (element.Type != RoadType.Car)
                return;

            RoadElement el = element;
            for (int i = 0; i < el.Points.Count; i++)
            {
                var point = el.Points[i];
                if (!_pointsMap.ContainsKey(point) && !_junctionsMap.ContainsKey(point))
                    _pointsMap.Add(point, new Tuple<RoadElement, int>(el, i));
                else
                {
                    var pointCount = el.Points.Count;
                    el = ProcessJunction(el, i);
                    // different count means that element was split
                    if (el.Points.Count != pointCount) i = 0;
                }
            }
        }

        private RoadElement ProcessJunction(RoadElement element, int junctionPointIndex)
        {
            var junctionPoint = element.Points[junctionPointIndex];

            if (_pointsMap.ContainsKey(junctionPoint))
            {
                var pointUsage = _pointsMap[junctionPoint];
                // NOTE road contains the same points
                if (pointUsage.Item1 == element)
                    return element;
            }

            if (!_junctionsMap.ContainsKey(junctionPoint))
                _junctionsMap.Add(junctionPoint, new RoadJunction(junctionPoint));

            var junction = _junctionsMap[junctionPoint];

            // split old usages
            if (_pointsMap.ContainsKey(junctionPoint))
            {
                var pointUsage = _pointsMap[junctionPoint];
                _pointsMap.Remove(junctionPoint);
                SplitElement(pointUsage.Item1, pointUsage.Item2, junction);
            }

            // split source element
            return SplitElement(element, junctionPointIndex, junction);
        }

        private RoadElement SplitElement(RoadElement element, int splitPointIndex, RoadJunction junction)
        {
            var splitPoint = element.Points[splitPointIndex];
            // case 1: in the middle - need to split to two elements
            if (splitPointIndex != 0 && splitPointIndex != element.Points.Count - 1)
            {
                // TODO use object pools for point lists?
                var points = new List<MapPoint>(element.Points);
                var secondElementPart = Clone(element);
                
                // insert offset point as last
                element.Points = points.Take(splitPointIndex).ToList();
                element.Points.Add(CalculatePoint(splitPoint, element.Points.Last(), Offset));
                junction.Connections.Add(new RoadJunction
                    .Connection(element.Points.Last(), element.Type, element.Width));

                // insert offset point as first
                secondElementPart.Points = points.Skip(splitPointIndex + 1).ToList();

                // TODO shift all indicies for secondElementPart references inside _pointsMap
                for (int i = 0; i < secondElementPart.Points.Count; i++)
                {
                    var point = secondElementPart.Points[i];
                    // this situation happens when we process current adding element
                    if (!_pointsMap.ContainsKey(point)) break;
                    var usage = _pointsMap[point];
                    usage.Item1 = secondElementPart;
                    usage.Item2 = i + 1;
                }

                secondElementPart.Points.Insert(0, CalculatePoint(splitPoint, secondElementPart.Points.First(), Offset));
                junction.Connections.Add(new RoadJunction
                    .Connection(secondElementPart.Points.First(), element.Type, element.Width));

                _elements.Add(secondElementPart);
                return secondElementPart;
            }

            // case 2: No need to split element
            // replace first or last point with point which has some offset from junction
            var nextPointIndex = splitPointIndex == 0 ? 1 : element.Points.Count - 2;
            var nextPoint = element.Points[nextPointIndex];
            var offsetPoint = CalculatePoint(splitPoint, nextPoint, Offset);
            element.Points[splitPointIndex] = offsetPoint;

            junction.Connections.Add(new RoadJunction
                .Connection(offsetPoint, element.Type, element.Width));

            return element;
        }

        #endregion

        #region Static methods

        private static RoadElement Clone(RoadElement element)
        {
            return new RoadElement
            {
                Id = element.Id,
                Type = element.Type,
                Address = element.Address,
                Width = element.Width,
                ZIndex = element.ZIndex,
                Points = element.Points
            };
        }

        /// <summary>
        ///     Gets point along AB at given distance from A
        /// </summary>
        public static MapPoint CalculatePoint(MapPoint a, MapPoint b, float distance)
        {
            // TODO ensure that generated point has valid direction:
            // AB' + B'B = AB It's possible that "distance" variable is greater than AB 

            // a. calculate the vector from o to g:
            float vectorX = b.X - a.X;
            float vectorY = b.Y - a.Y;

            // b. calculate the proportion of hypotenuse
            var factor = (float) (distance / Math.Sqrt(vectorX * vectorX + vectorY * vectorY));

            // c. factor the lengths
            vectorX *= factor;
            vectorY *= factor;

            // d. calculate and Draw the new vector,
            return new MapPoint(a.X + vectorX, a.Y + vectorY, a.Elevation);
        }

        #endregion

        public void Clear()
        {
            _elements.Clear();
            _junctionsMap.Clear();
            _pointsMap.Clear();
        }
    }
}
