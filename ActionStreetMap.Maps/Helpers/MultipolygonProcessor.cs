using System;
using System.Collections.Generic;
using System.Linq;
using ActionStreetMap.Core;
using ActionStreetMap.Core.Utilities;
using ActionStreetMap.Maps.Entities;
using Area = ActionStreetMap.Core.Tiling.Models.Area;
using TagCollection = ActionStreetMap.Core.Tiling.Models.TagCollection;

namespace ActionStreetMap.Maps.Helpers
{
    /// <summary>
    ///     Implements algorithm to build Model areas from multipolygon relation.
    /// </summary>
    internal static class MultipolygonProcessor
    {
        /// <summary>
        ///     Fills area list by processing multipolygons from given relation.
        /// </summary>
        /// <param name="relation">Relation.</param>
        /// <param name="areas">List of areas.</param>
        public static void FillAreas(Relation relation, List<Area> areas)
        {
            // see http://wiki.openstreetmap.org/wiki/Relation:multipolygon/Algorithm
            bool allClosed = true;
            int memberCount = relation.Members.Count;
            var outerIndecies = new List<int>(memberCount/2);
            var innerIndecies = new List<int>(memberCount/2);
            var sequences = new List<CoordinateSequence>(relation.Members.Count / 2);
            foreach (var member in relation.Members)
            {
                var way = member.Member as Way;
                if (way == null || !way.Coordinates.Any())
                    continue;

                if (member.Role == "outer")
                    outerIndecies.Add(sequences.Count);
                else if (member.Role == "inner")
                    innerIndecies.Add(sequences.Count);
                else 
                    continue;
              
                // TODO what should be used as Id?
                var sequence = new CoordinateSequence(relation.Id, way);
                if (!sequence.IsClosed)
                    allClosed = false;
                sequences.Add(sequence);
            }

            if (outerIndecies.Count == 1 && allClosed)
                SimpleCase(relation, areas, sequences, outerIndecies, innerIndecies);
            else
                ComplexCase(relation, areas, sequences);
        }

        private static void SimpleCase(Relation relation, List<Area> areas, List<CoordinateSequence> sequences,
            List<int> outerIndecies, List<int> innerIndecies)
        {
            // TODO set correct tags!
            var outer = sequences[outerIndecies[0]];
            
            // TODO investigate case of null/empty tags
            var tags = GetTags(relation, outer);
            if (tags == null || !tags.Any())
                return;

            areas.Add(new Area()
            {
                Id = outer.Id,
                Tags = tags,
                Points = outer.Coordinates,
                Holes = innerIndecies.Select(i => sequences[i].Coordinates).ToList()
            });
        }

        private static void ComplexCase(Relation relation, List<Area> areas, List<CoordinateSequence> sequences)
        {
            var rings = CreateRings(sequences);
            if (rings == null)
                return;
            FillAreas(relation, rings, areas);
        }

        private static List<CoordinateSequence> CreateRings(List<CoordinateSequence> sequences)
        {
            var closedRings = new List<CoordinateSequence>();
            CoordinateSequence currentRing = null;
            while (sequences.Any())
            {
                if (currentRing == null)
                {
                    // start a new ring with any remaining node sequence
                    var lastIndex = sequences.Count - 1;
                    currentRing = sequences[lastIndex];
                    sequences.RemoveAt(lastIndex);
                }
                else
                {
                    // try to continue the ring by appending a node sequence
                    CoordinateSequence assignedSequence = null;
                    foreach (CoordinateSequence sequence in sequences)
                    {
                        if (!currentRing.TryAdd(sequence)) continue;
                        assignedSequence = sequence;
                        break;
                    }

                    if (assignedSequence != null)
                        sequences.Remove(assignedSequence);
                    else
                        return null;
                }

                // check whether the ring under construction is closed
                if (currentRing != null && currentRing.IsClosed)
                {
                    // TODO check that it isn't self-intersecting!
                    closedRings.Add(new CoordinateSequence(currentRing));
                    currentRing = null;
                }
            }

            return currentRing != null ? null : closedRings;
        }

        private static void FillAreas(Relation relation, List<CoordinateSequence> rings, List<Area> areas)
        {
            while (rings.Any())
            {
                // find an outer ring
                CoordinateSequence outer = null;
                foreach (CoordinateSequence candidate in rings)
                {
                    bool containedInOtherRings = false;
                    foreach (CoordinateSequence other in rings)
                    {
                        if (other != candidate && other.ContainsRing(candidate))
                        {
                            containedInOtherRings = true;
                            break;
                        }
                    }
                    if (containedInOtherRings) 
                        continue;

                    outer = candidate;
                    break;
                }

                // find inner rings of that ring
                var inners = new List<CoordinateSequence>();
                foreach (CoordinateSequence ring in rings)
                {
                    if (ring != outer && outer.ContainsRing(ring))
                    {
                        bool containedInOthers = false;
                        foreach (CoordinateSequence other in rings)
                        {
                            if (other != ring && other != outer && other.ContainsRing(ring))
                            {
                                containedInOthers = true;
                                break;
                            }
                        }
                        if (!containedInOthers)
                            inners.Add(ring);
                    }
                }

                // create a new area and remove the used rings
                var holes = new List<List<GeoCoordinate>>(inners.Count);
                foreach (CoordinateSequence innerRing in inners)
                    holes.Add(innerRing.Coordinates);

                // TODO investigate case of null/empty tags
                var tags = GetTags(relation, outer);
                if (tags != null && tags.Any())
                {
                    areas.Add(new Area()
                    {
                        Id = outer.Id,
                        Tags = tags,
                        Points = outer.Coordinates,
                        Holes = holes
                    });
                }

                rings.Remove(outer);
                // remove all innerRings
                foreach (var nodeSequence in inners)
                    rings.Remove(nodeSequence);
            }
        }

        private static TagCollection GetTags(Relation relation, CoordinateSequence outer)
        {
            // TODO tag processing
            return relation.Tags.Count > 1 ? relation.Tags : outer.Tags;
        }

        #region Nested classes

        private class CoordinateSequence
        {
            private readonly List<GeoCoordinate> _nodes;
            private List<GeoCoordinate> _coordinates;
            private long _id;

            public TagCollection Tags { get; set; } 

            public CoordinateSequence(long id, Way way)
            {
                _nodes = new List<GeoCoordinate>(way.Coordinates);
                _id = id;
                Tags = way.Tags.AllowAdd();
            }

            public CoordinateSequence(CoordinateSequence sequence)
            {
                _id = sequence.Id;
                _nodes = new List<GeoCoordinate>(sequence._nodes);
            }

            private void AddAll(CoordinateSequence other) { _nodes.AddRange(other._nodes); }

            private void AddAll(int index, CoordinateSequence other) { _nodes.InsertRange(index, other._nodes); }

            private void Reverse() { _nodes.Reverse(); }

            /// <summary>
            ///  Tries to add another sequence onto the start or end of this one.
            ///  If it succeeds, the other sequence may also be modified and
            ///  should be considered "spent".
            /// </summary>
            /// <param name="other">CoordinateSequence.</param>
            public bool TryAdd(CoordinateSequence other)
            {
                if (LastNode == other.FirstNode)
                {
                    //add the sequence at the end
                    _nodes.RemoveAt(_nodes.Count - 1);
                    AddAll(other);
                    MergeTags(other.Tags);
                    return true;
                }
                if (LastNode == other.LastNode)
                {
                    //add the sequence backwards at the end
                    _nodes.RemoveAt(_nodes.Count - 1);
                    other.Reverse();
                    AddAll(other);
                    MergeTags(other.Tags);
                    return true;
                }
                if (FirstNode == other.LastNode)
                {
                    //add the sequence at the beginning
                    _nodes.RemoveAt(0);
                    AddAll(0, other);
                    MergeTags(other.Tags);
                    return true;
                }
                if (FirstNode == other.FirstNode)
                {
                    //add the sequence backwards at the beginning
                    _nodes.RemoveAt(0);
                    other.Reverse();
                    AddAll(0, other);
                    MergeTags(other.Tags);
                    return true;
                }
                return false;
            }

            private GeoCoordinate FirstNode { get { return _nodes.First(); } }

            private GeoCoordinate LastNode { get { return _nodes.Last(); } }

            public bool IsClosed { get { return _nodes.First() == _nodes.Last(); } }

            public long Id { get { return _id; }}

            public List<GeoCoordinate> Coordinates
            {
                get { return _coordinates ?? (_coordinates = _nodes.Select(n => n).ToList()); }
            }

            public bool ContainsRing(CoordinateSequence other)
            {
                return other.Coordinates.All(c => IsPointInPolygon(c, Coordinates));
            }

            private void MergeTags(TagCollection other)
            {
                Tags = Tags ?? new TagCollection();
                if (other != null)
                {
                    foreach (var kv in other)
                    {
                        if (!Tags.ContainsKey(kv.Key))
                            Tags.Add(kv.Key, kv.Value);
                    }
                }
            }

            /// <summary>
            ///     Checks whether point is in polygon. Define this function here so far
            /// </summary>
            private static bool IsPointInPolygon(GeoCoordinate point, List<GeoCoordinate> verts)
            {
                int i, j, nvert = verts.Count;
                bool c = false;
                for (i = 0, j = nvert - 1; i < nvert; j = i++)
                {
                    if (((verts[i].Latitude > point.Latitude) != (verts[j].Latitude > point.Latitude)) &&
                     (point.Longitude < (verts[j].Longitude - verts[i].Longitude) * (point.Latitude - verts[i].Latitude) / 
                     (verts[j].Latitude - verts[i].Latitude) + verts[i].Longitude))
                        c = !c;
                }
                return c;
            }
        }
        #endregion
    }
}
