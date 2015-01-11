using System;
using System.Collections.Generic;
using System.Linq;
using ActionStreetMap.Infrastructure.Dependencies;
using ActionStreetMap.Infrastructure.Diagnostic;
using ActionStreetMap.Infrastructure.Primitives;

namespace ActionStreetMap.Core.Scene.World.Roads
{
    /// <summary>
    ///     Defines API for building road graph.
    /// </summary>
    public interface IRoadGraphBuilder
    {
        /// <summary>
        ///     Adds road element to graph.
        /// </summary>
        /// <param name="element">Road element.</param>
        void Add(RoadElement element);

        /// <summary>
        ///    Builds road graph and cleanups internal buffers to make object ready to reuse.
        /// </summary>
        /// <returns>Road graph.</returns>
        RoadGraph Build();
    }

    /// <summary>
    ///     Default implementation of <see cref="IRoadGraphBuilder"/>.
    /// </summary>
    public sealed class RoadGraphBuilder: IRoadGraphBuilder
    {
        // map which is used for merging of split elements
        private readonly Dictionary<long, List<RoadElement>> _elements = new Dictionary<long, List<RoadElement>>(64);

        // key is point which is shared between different road elements ("junction")
        private readonly Dictionary<MapPoint, RoadJunction> _junctionsMap = new Dictionary<MapPoint, RoadJunction>(32);

        // point to index in RoadElement/index in Element.Points tuple
        private readonly Dictionary<MapPoint, SortedList<RoadType, Tuple<RoadElement, int>>> _pointsMap = 
            new Dictionary<MapPoint, SortedList<RoadType, Tuple<RoadElement, int>>>(256);

        /// <summary>
        ///     Gets or sets trace.
        /// </summary>
        [Dependency]
        public ITrace Trace { get; set; }

        #region Public methods

        /// <inheritdoc />
        public RoadGraph Build()
        {
            MergeRoads();

            var roads = _elements.Select(kv => new Road { Elements = kv.Value }).ToArray();
            var junctions = _junctionsMap.Values.ToArray();

            // clear buffers
            _elements.Clear();
            _junctionsMap.Clear();
            _pointsMap.Clear();

            return new RoadGraph(roads, junctions);
        }

        /// <inheritdoc />
        public void Add(RoadElement element)
        {
            _elements.Add(element.Id, new List<RoadElement>(1) { element });

            RoadElement el = element;
            for (int i = 0; i < el.Points.Count; i++)
            {
                var point = el.Points[i];

                if (!_pointsMap.ContainsKey(point))
                    _pointsMap[point] = new SortedList<RoadType, Tuple<RoadElement, int>>();

                if (!_pointsMap[point].ContainsKey(el.Type) && !_junctionsMap.ContainsKey(point))
                    _pointsMap[point].Add(el.Type, new Tuple<RoadElement, int>(el, i));
                else if (ShouldBeSplit(point, el))
                {
                    var pointCount = el.Points.Count;
                    el = ProcessJunction(el, i);
                    // different count means that element was split
                    if (el.Points.Count != pointCount) i = 0;
                }
            }
        }

        #endregion

        #region Merge roads

        /// <summary>
        ///     Merges roads from junctions with only two connection of same road
        /// </summary>
        private void MergeRoads()
        {
            var toBeRemovedJunctionKeys = new List<MapPoint>();
            foreach (var pair in _junctionsMap)
            {
                var junction = pair.Value;
                if (junction.Connections.Count != 2)
                    continue;

                var first = junction.Connections[0].Element;
                var second = junction.Connections[1].Element;

                // modified junction
                // NOTE: seems to happen in circular list
                if (first.Id == second.Id)
                {
                    RemoveJunction(first, junction);
                    RemoveJunction(second, junction);

                    toBeRemovedJunctionKeys.Add(pair.Key);
                    continue;
                }

                var elementFirstList = _elements[first.Id];
                var elementSecondList = _elements[second.Id];

                // <e---ss---e> 
                if (first.Start == junction && first.Start == second.Start)
                {
                    ReverseElementList(elementSecondList);
                    MergeElementLists(first.Id, second.Id, elementFirstList, elementSecondList, true);
                    first.Start = null;
                    second.End = null;
                } 
                // s---e><e---s
                else if (first.End == junction && first.End == second.End)
                {
                    ReverseElementList(elementSecondList);
                    MergeElementLists(first.Id, second.Id, elementFirstList, elementSecondList, false);
                    first.End = null;
                    second.Start = null;
                }
                // s---e>s---e>
                else if (first.End == junction && first.End == second.Start)
                {
                    MergeElementLists(first.Id, second.Id, elementFirstList, elementSecondList, false);
                    first.End = null;
                    second.Start = null;
                }
                else if (first.Start == junction && first.Start == second.End)
                {
                    MergeElementLists(first.Id, second.Id, elementFirstList, elementSecondList, true);
                    first.Start = null;
                    second.End = null;
                }
                else
                {
                    RemoveJunction(first, junction);
                    RemoveJunction(second, junction);
                }

                toBeRemovedJunctionKeys.Add(pair.Key);

            }
            toBeRemovedJunctionKeys.ForEach(p => _junctionsMap.Remove(p));
        }

        private void ReverseElementList(List<RoadElement> elements)
        {
            foreach (var roadElement in elements)
            {
                roadElement.Points.Reverse();
                var tmp = roadElement.Start;
                roadElement.Start = roadElement.End;
                roadElement.End = tmp;
            }
            elements.Reverse();
        }

        private void MergeElementLists(long firstId, long secondId, List<RoadElement> firstList, List<RoadElement> secondList, bool shouldBeFirst)
        {
            if (shouldBeFirst)
                firstList.InsertRange(0, secondList);
            else
                firstList.AddRange(secondList);
            _elements.Remove(secondId);
            Trace.Output("road.graph", String.Format("merge {0} with {1}", secondId, firstId));
            secondList.ForEach(e => e.Id = firstId);
        }

        private void RemoveJunction(RoadElement element, RoadJunction junction)
        {
            if (element.Start == junction) element.Start = null;
            else element.End = null;
        }

        #endregion

        #region Split elements

        private bool ShouldBeSplit(MapPoint point, RoadElement element)
        {
            if (_pointsMap.ContainsKey(point))
                return _pointsMap[point].ContainsKey(element.Type);

            // should be the same type
            return _junctionsMap[point].Connections.First().Element.Type == element.Type;
        }

        private RoadElement ProcessJunction(RoadElement element, int junctionPointIndex)
        {
            var junctionPoint = element.Points[junctionPointIndex];

            // road contains the same points
            if (_pointsMap.ContainsKey(junctionPoint) && _pointsMap[junctionPoint][element.Type].Item1 == element)
                return element;

            if (!_junctionsMap.ContainsKey(junctionPoint))
                _junctionsMap.Add(junctionPoint, new RoadJunction(junctionPoint));

            var junction = _junctionsMap[junctionPoint];

            // split old usage
            if (_pointsMap.ContainsKey(junctionPoint) && 
                _pointsMap[junctionPoint].ContainsKey(element.Type))
            {
                var pointUsage = _pointsMap[junctionPoint][element.Type];
                _pointsMap.Remove(junctionPoint);
                SplitElement(pointUsage.Item1, pointUsage.Item2, junction);
            }

            // split source element
            return SplitElement(element, junctionPointIndex, junction);
        }

        private RoadElement SplitElement(RoadElement element, int splitPointIndex, RoadJunction junction)
        {
            // case 1: in the middle - need to split to two elements
            if (splitPointIndex != 0 && splitPointIndex != element.Points.Count - 1)
            {
                // TODO use object pools for point lists?
                var points = new List<MapPoint>(element.Points);
                var secondElementPart = Clone(element);

                // insert offset point as last
                element.Points = points.Take(splitPointIndex + 1).ToList();
                junction.Connections.Add(new RoadJunction.Connection(element.Points[element.Points.Count - 1], element));
                element.End = junction;

                // insert offset point as first
                secondElementPart.Points = points.Skip(splitPointIndex).ToList();

                // shift all indicies for secondElementPart references inside _pointsMap
                for (int i = 1; i < secondElementPart.Points.Count; i++)
                {
                    var point = secondElementPart.Points[i];
                    // this situation happens when we try to split current element (and, probably in other rare cases)
                    if (!_pointsMap.ContainsKey(point) || !_pointsMap[point].ContainsKey(element.Type)) 
                        continue;
                    var usage = _pointsMap[point][element.Type];
                    if (usage.Item1 == element)
                    {
                        usage.Item1 = secondElementPart;
                        usage.Item2 = i;
                    }
                }

                junction.Connections.Add(new RoadJunction.Connection(secondElementPart.Points[0], secondElementPart));
                secondElementPart.Start = junction;

                _elements[secondElementPart.Id].Add(secondElementPart);
                return secondElementPart;
            }

            // case 2: No need to split element
            if (splitPointIndex == 0) element.Start = junction;
            else element.End = junction;

            junction.Connections.Add(new RoadJunction.Connection(element.Points[splitPointIndex], element));

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
                Address = element.Address,
                Width = element.Width,
                ZIndex = element.ZIndex,
                Points = element.Points,
                Start = element.Start,
                End = element.End
            };
        }

        #endregion
    }
}
