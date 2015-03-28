﻿// ----------------------------------------------------------------------- 
// <copyright file="VoronoiBase.cs">
//     Original Triangle code by Jonathan Richard Shewchuk,
//     http: //www.cs.cmu.edu/~quake/triangle.html Triangle.NET code by Christian Woltering, http://triangle.codeplex.com/
// </copyright>
// ----------------------------------------------------------------------- 

using System.Collections.Generic;
using ActionStreetMap.Core.Geometry.Triangle.Geometry;
using ActionStreetMap.Core.Geometry.Triangle.Topology;
using ActionStreetMap.Core.Geometry.Triangle.Topology.DCEL;
using Vertex = ActionStreetMap.Core.Geometry.Triangle.Topology.DCEL.Vertex;

namespace ActionStreetMap.Core.Geometry.Triangle.Voronoi
{
    /// <summary> The Voronoi diagram is the dual of a pointset triangulation. </summary>
    public abstract class VoronoiBase : DcelMesh
    {
        // List of infinite half-edges, i.e. half-edges that start at circumcenters of triangles
        // which lie on the domain boundary.
        protected List<HalfEdge> rays;
        protected Mesh mesh;

        /// <summary> Initializes a new instance of the <see cref="VoronoiBase"/> class. </summary>
        /// <param name="mesh"> Triangle mesh. </param>
        /// <param name="generate">
        /// If set to true, the constuctor will call the Generate method, which builds the Voronoi diagram.
        /// </param>
        protected VoronoiBase(Mesh mesh, bool generate)
            : base(false)
        {
            this.mesh = mesh;
            if (generate)
            {
                Generate();
            }
        }

        /// <summary> Generate the Voronoi diagram from given triangle mesh.. </summary>
        protected void Generate()
        {
            mesh.Renumber();

            base.edges = new List<HalfEdge>();
            this.rays = new List<HalfEdge>();

            // Allocate space for Voronoi diagram. 
            var vertices = new Vertex[mesh.triangles.Count + mesh.hullsize];
            var faces = new Face[mesh.vertices.Count];

            // Compute triangles circumcenters. 
            var map = ComputeVertices(mesh, vertices);

            // Create all Voronoi faces. 
            foreach (var vertex in mesh.vertices.Values)
            {
                faces[vertex.id] = new Face(vertex);
            }

            ComputeEdges(mesh, vertices, faces, map);

            // At this point all edges are computed, but the (edge.next) pointers aren't set. 
            ConnectEdges(map);

            base.vertices = new List<Vertex>(vertices);
            base.faces = new List<Face>(faces);
        }

        /// <summary> Compute the Voronoi vertices (the circumcenters of the triangles). </summary>
        /// <returns> An empty map, which will map all vertices to a list of leaving edges. </returns>
        protected List<HalfEdge>[] ComputeVertices(Mesh mesh, Vertex[] vertices)
        {
            Otri tri = default(Otri);
            double xi = 0, eta = 0;
            Vertex vertex;
            Point pt;
            int id;

            // Maps all vertices to a list of leaving edges. 
            var map = new List<HalfEdge>[mesh.triangles.Count];

            // Compue triangle circumcenters 
            foreach (var t in mesh.triangles.Values)
            {
                id = t.id;
                tri.tri = t;

                pt = mesh.robustPredicates.FindCircumcenter(tri.Org(), tri.Dest(), tri.Apex(), ref xi, ref eta);

                vertex = new Vertex(pt.x, pt.y);
                vertex.id = id;

                vertices[id] = vertex;
                map[id] = new List<HalfEdge>();
            }

            return map;
        }

        /// <summary> Compute the edges of the Voronoi diagram. </summary>
        /// <param name="mesh"></param>
        /// <param name="vertices"></param>
        /// <param name="faces"></param>
        /// <param name="map"> Empty vertex map. </param>
        protected void ComputeEdges(Mesh mesh, Vertex[] vertices, Face[] faces, List<HalfEdge>[] map)
        {
            Otri tri, neighbor = default(Otri);
            Geometry.Vertex org, dest;

            double px, py;
            int id, nid, count = mesh.triangles.Count;

            Face face, neighborFace;
            HalfEdge edge, twin;
            Vertex vertex, end;

            // Count infinte edges (vertex id for their endpoints). 
            int j = 0;

            // Count half-edges (edge ids). 
            int k = 0;

            // To loop over the set of edges, loop over all triangles, and look at the three edges
            // of each triangle. If there isn't another triangle adjacent to the edge, operate on
            // the edge. If there is another adjacent triangle, operate on the edge only if the
            // current triangle has a smaller id than its neighbor. This way, each edge is
            // considered only once.
            foreach (var t in mesh.triangles.Values)
            {
                id = t.id;

                tri.tri = t;

                for (int i = 0; i < 3; i++)
                {
                    tri.orient = i;
                    tri.Sym(ref neighbor);

                    nid = neighbor.tri.id;

                    if (id < nid || nid < 0)
                    {
                        // Get the endpoints of the current triangle edge. 
                        org = tri.Org();
                        dest = tri.Dest();

                        face = faces[org.id];
                        neighborFace = faces[dest.id];

                        vertex = vertices[id];

                        // For each edge in the triangle mesh, there's a corresponding edge in the
                        // Voronoi diagram, i.e. two half-edges will be created.
                        if (nid < 0)
                        {
                            // Unbounded edge, direction perpendicular to the boundary edge,
                            // pointing outwards.
                            px = dest.y - org.y;
                            py = org.x - dest.x;

                            end = new Vertex(vertex.x + px, vertex.y + py);
                            end.id = count + j++;

                            vertices[end.id] = end;

                            edge = new HalfEdge(end, face);
                            twin = new HalfEdge(vertex, neighborFace);

                            // Make (face.edge) always point to an edge that starts at an infinite
                            // vertex. This will allow traversing of unbounded faces.
                            face.edge = edge;
                            face.bounded = false;

                            map[id].Add(twin);

                            rays.Add(twin);
                        }
                        else
                        {
                            end = vertices[nid];

                            // Create half-edges. 
                            edge = new HalfEdge(end, face);
                            twin = new HalfEdge(vertex, neighborFace);

                            // Add to vertex map. 
                            map[nid].Add(edge);
                            map[id].Add(twin);
                        }

                        vertex.leaving = twin;
                        end.leaving = edge;

                        edge.twin = twin;
                        twin.twin = edge;

                        edge.id = k++;
                        twin.id = k++;

                        this.edges.Add(edge);
                        this.edges.Add(twin);
                    }
                }
            }
        }

        /// <summary> Connect all edges of the Voronoi diagram. </summary>
        /// <param name="map"> Maps all vertices to a list of leaving edges. </param>
        protected virtual void ConnectEdges(List<HalfEdge>[] map)
        {
            int length = map.Length;

            // For each half-edge, find its successor in the connected face. 
            foreach (var edge in this.edges)
            {
                var face = edge.face.generator.id;

                // The id of the dest vertex of current edge. 
                int id = edge.twin.origin.id;

                // The edge origin can also be an infinite vertex. Sort them out by checking the id. 
                if (id < length)
                {
                    // Look for the edge that is connected to the current face. Each Voronoi vertex
                    // has degree 3, so this loop is actually O(1).
                    foreach (var next in map[id])
                    {
                        if (next.face.generator.id == face)
                        {
                            edge.next = next;
                            break;
                        }
                    }
                }
            }
        }

        protected override IEnumerable<IEdge> EnumerateEdges()
        {
            var edges = new List<IEdge>(this.edges.Count / 2);

            foreach (var edge in this.edges)
            {
                var twin = edge.twin;

                // Report edge only once. 
                if (twin == null)
                {
                    edges.Add(new Edge(edge.origin.id, edge.next.origin.id));
                }
                else if (edge.id < twin.id)
                {
                    edges.Add(new Edge(edge.origin.id, twin.origin.id));
                }
            }

            return edges;
        }
    }
}