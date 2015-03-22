using System;
using System.Collections.Generic;
using System.Linq;
using ActionStreetMap.Core.Polygons;
using ActionStreetMap.Infrastructure.Primitives;
using ActionStreetMap.Infrastructure.Utilities;

using Path = System.Collections.Generic.List<ActionStreetMap.Core.Polygons.IntPoint>;
using Paths = System.Collections.Generic.List<System.Collections.Generic.List<ActionStreetMap.Core.Polygons.IntPoint>>;

namespace ActionStreetMap.Core.Scene.Roads
{
    /// <summary> Provides ability to build road graph. </summary>
    internal sealed class RoadGraphBuilder
    {
        // map which is used for merging of split elements
        private readonly Dictionary<long, List<RoadElement>> _elements = new Dictionary<long, List<RoadElement>>(1024);

        // key is point which is shared between different road elements ("junction")
        private readonly Dictionary<MapPoint, SortedList<RoadType, RoadJunction>> _junctionsMap = new Dictionary<MapPoint, SortedList<RoadType, RoadJunction>>(256);

        // point to index in RoadElement/index in Element.Points tuple
        private readonly Dictionary<MapPoint, SortedList<RoadType, MutableTuple<RoadElement, int>>> _pointsMap = 
            new Dictionary<MapPoint, SortedList<RoadType, MutableTuple<RoadElement, int>>>(2056);

        #region Public methods

        /// <inheritdoc />
        public RoadGraph Build(IObjectPool objectPool)
        {
            MergeRoads();

            var roads = _elements
                .Select(kv => new Road { Elements = kv.Value })
                .ToArray();

            var junctions = _junctionsMap.Values
                .SelectMany(j => j.Values)
                .Select(r => RoadJunctionUtils.Complete(r, objectPool)).ToArray();

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
                    _pointsMap[point] = new SortedList<RoadType, MutableTuple<RoadElement, int>>(1);

                if (!_pointsMap[point].ContainsKey(el.Type) && !_junctionsMap.ContainsKey(point))
                    _pointsMap[point].Add(el.Type, new MutableTuple<RoadElement, int>(el, i));
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
            var toBeRemovedJunctionKeys = new List<MutableTuple<MapPoint, RoadType>>();
            foreach (var pair in _junctionsMap)
            {
                var list = pair.Value;
                foreach (var lPair in list)
                {
                    var type = lPair.Key;
                    var junction = lPair.Value;
                    if (junction.Connections.Count != 2)
                        continue;

                    var first = junction.Connections[0];
                    var second = junction.Connections[1];

                    if (first.Id == second.Id)
                    {
                        RemoveJunction(first, junction);
                        RemoveJunction(second, junction);
                    }
                    else
                    {
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
                    }

                    toBeRemovedJunctionKeys.Add(new MutableTuple<MapPoint, RoadType>(pair.Key, type));
                }
               
            }
            toBeRemovedJunctionKeys.ForEach(p => _junctionsMap[p.Item1].Remove(p.Item2));
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
            if (_pointsMap.ContainsKey(point) && _pointsMap[point].ContainsKey(element.Type))
                return true;
            // should be the same type
            return _junctionsMap[point].ContainsKey(element.Type);
        }

        private RoadElement ProcessJunction(RoadElement element, int junctionPointIndex)
        {
            var junctionPoint = element.Points[junctionPointIndex];

            // road contains the same points
            if (_pointsMap.ContainsKey(junctionPoint))
            {
                var list = _pointsMap[junctionPoint];
                if (list.ContainsKey(element.Type) && list[element.Type].Item1 == element)
                    return element;
            }

            if (!_junctionsMap.ContainsKey(junctionPoint))
                _junctionsMap.Add(junctionPoint, new SortedList<RoadType, RoadJunction>());

            var junctionMapList = _junctionsMap[junctionPoint];

            if(!junctionMapList.ContainsKey(element.Type))
                junctionMapList.Add(element.Type, new RoadJunction(junctionPoint));

            var junction = junctionMapList[element.Type];

            // split old usage
            if (_pointsMap.ContainsKey(junctionPoint) && 
                _pointsMap[junctionPoint].ContainsKey(element.Type))
            {
                var pointUsage = _pointsMap[junctionPoint][element.Type];
                if (pointUsage.Item1 != element)
                {
                    _pointsMap[junctionPoint].Remove(element.Type);
                    SplitElement(pointUsage.Item1, pointUsage.Item2, junction);
                }
            }

            // split source element
            return SplitElement(element, junctionPointIndex, junction);
        }

        private RoadElement SplitElement(RoadElement element, int splitPointIndex, RoadJunction junction)
        {
            // case 1: in the middle - need to split to two elements and modify affected junction
            if (splitPointIndex != 0 && splitPointIndex != element.Points.Count - 1)
            {
                var elements = _elements[element.Id];
                int insertIndex = 0;
                while (elements[insertIndex++] != element);

                // TODO use object pools for point lists?
                var points = new List<MapPoint>(element.Points);
                var secondElementPart = Clone(element);

                // insert offset point as last
                element.Points = points.Take(splitPointIndex + 1).ToList();
                junction.Connections.Add(element);

                // we have to modify end junction: it should point to the second element part
                if (element.End != null)
                {
                    var changeIndex = -1;
                    while (element.End.Connections[++changeIndex] != element);
                    element.End.Connections[changeIndex] = secondElementPart;
                }

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

                junction.Connections.Add(secondElementPart);
                secondElementPart.Start = junction;

                _elements[secondElementPart.Id].Insert(insertIndex, secondElementPart);
                return secondElementPart;
            }

            // case 2: No need to split element
            if (splitPointIndex == 0) element.Start = junction;
            else element.End = junction;

            junction.Connections.Add(element);

            return element;
        }

        #endregion

        #region Static methods

        /// <summary> Creates clone of given road element. </summary>
        private static RoadElement Clone(RoadElement element)
        {
            return new RoadElement
            {
                Id = element.Id,
                Type = element.Type,
                Address = element.Address,
                Width = element.Width,
                Points = element.Points,
                Start = element.Start,
                End = element.End
            };
        }

        #endregion
    }
}
