using System.Collections.Generic;
using System.Linq;
using ActionStreetMap.Infrastructure.Primitives;

namespace ActionStreetMap.Core.Scene.World.Roads
{
    /// <summary>
    ///     Responsible for road graph building.
    /// </summary>
    public sealed class RoadGraphBuilder
    {
        // map which is used for merging of split elements
        private readonly Dictionary<long, List<RoadElement>> _elements = new Dictionary<long, List<RoadElement>>(64);

        // key is point which is shared between different road elements ("junction")
        private readonly Dictionary<MapPoint, RoadJunction> _junctionsMap = new Dictionary<MapPoint, RoadJunction>(32);

        // point to index in RoadElement/index in Element.Points tuple
        private readonly Dictionary<MapPoint, Tuple<RoadElement, int>> _pointsMap = new Dictionary<MapPoint, Tuple<RoadElement, int>>(256);

        #region Public methods

        /// <summary>
        ///    Builds road graph and cleanups internal buffers to make object ready to reuse.
        /// </summary>
        /// <returns>Road graph.</returns>
        public RoadGraph Build()
        {
            var roads = _elements.Select(kv => new Road() { Elements = kv.Value }).ToArray();
            var junctions = _junctionsMap.Values.ToArray();

            // clear buffers
            _elements.Clear();
            _junctionsMap.Clear();
            _pointsMap.Clear();

            return new RoadGraph(roads, junctions);
        }

        /// <summary>
        ///     Adds road element to graph.
        /// </summary>
        /// <param name="element">Road element.</param>
        public void Add(RoadElement element)
        {
            _elements.Add(element.Id, new List<RoadElement>(1) { element });

            RoadElement el = element;
            for (int i = 0; i < el.Points.Count; i++)
            {
                var point = el.Points[i];
                if (!_pointsMap.ContainsKey(point) && !_junctionsMap.ContainsKey(point))
                    _pointsMap.Add(point, new Tuple<RoadElement, int>(el, i));
                else if (GetRoadElement(point).Type == element.Type)
                {
                    var pointCount = el.Points.Count;
                    el = ProcessJunction(el, i);
                    // different count means that element was split
                    if (el.Points.Count != pointCount) i = 0;
                }
            }
        }

        private RoadElement GetRoadElement(MapPoint point)
        {
            if (_pointsMap.ContainsKey(point))
                return _pointsMap[point].Item1;

            // should be the same type
            return _junctionsMap[point].Connections.First().Element;
        }

        private RoadElement ProcessJunction(RoadElement element, int junctionPointIndex)
        {
            var junctionPoint = element.Points[junctionPointIndex];

            // road contains the same points
            if (_pointsMap.ContainsKey(junctionPoint) && _pointsMap[junctionPoint].Item1 == element)
                return element;

            if (!_junctionsMap.ContainsKey(junctionPoint))
                _junctionsMap.Add(junctionPoint, new RoadJunction(junctionPoint));

            var junction = _junctionsMap[junctionPoint];

            // split old usage: we expect only one
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
                element.Points.Add(CalculatePoint(splitPoint, element.Points.Last(), element.Width));
                junction.Connections.Add(new RoadJunction.Connection(element.Points.Last(), element));
                element.End = junction;

                // insert offset point as first
                secondElementPart.Points = points.Skip(splitPointIndex + 1).ToList();

                // shift all indicies for secondElementPart references inside _pointsMap
                for (int i = 0; i < secondElementPart.Points.Count; i++)
                {
                    var point = secondElementPart.Points[i];
                    // this situation happens when we try to split current element
                    if (!_pointsMap.ContainsKey(point)) break;
                    var usage = _pointsMap[point];
                    usage.Item1 = secondElementPart;
                    usage.Item2 = i + 1;
                }

                secondElementPart.Points.Insert(0, CalculatePoint(splitPoint, secondElementPart.Points.First(), element.Width));
                junction.Connections.Add(new RoadJunction.Connection(secondElementPart.Points.First(), secondElementPart));
                secondElementPart.Start = junction;

                _elements[secondElementPart.Id].Add(secondElementPart);
                return secondElementPart;
            }

            // case 2: No need to split element
            // replace first or last point with point which has some offset from junction
            int nextPointIndex;
            if (splitPointIndex == 0)
            {
                nextPointIndex = 1;
                element.Start = junction;
            }
            else
            {
                nextPointIndex = element.Points.Count - 2;
                element.End = junction;
            }

            var nextPoint = element.Points[nextPointIndex];
            var offsetPoint = CalculatePoint(splitPoint, nextPoint, element.Width);
            element.Points[splitPointIndex] = offsetPoint;

            junction.Connections.Add(new RoadJunction.Connection(offsetPoint, element));

            return element;
        }

        #endregion

        #region Static methods

        /// <summary>
        ///     Creates clone of given road element.
        /// </summary>
        private static RoadElement Clone(RoadElement element)
        {
            return new RoadElement
            {
                Id = element.Id,
                Type = element.Type,
                //Address = element.Address,
                Width = element.Width,
                ZIndex = element.ZIndex,
                Points = element.Points,
                Start = element.Start,
                End = element.End
            };
        }

        /// <summary>
        ///     Gets point along AB at given distance from A.
        /// </summary>
        private static MapPoint CalculatePoint(MapPoint a, MapPoint b, float width)
        {
            // TODO calculate distance based on width
            var distance = width;

            // TODO ensure that generated point has valid direction:
            // AB' + B'B = AB It's possible that "distance" variable is greater than AB 

            // a. calculate the vector from o to g:
            float vectorX = b.X - a.X;
            float vectorY = b.Y - a.Y;

            // b. calculate the proportion of hypotenuse
            var factor = (float)(distance / System.Math.Sqrt(vectorX * vectorX + vectorY * vectorY));

            // c. factor the lengths
            vectorX *= factor;
            vectorY *= factor;

            // d. calculate and Draw the new vector,
            return new MapPoint(a.X + vectorX, a.Y + vectorY, a.Elevation);
        }

        #endregion
    }
}
