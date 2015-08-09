namespace ActionStreetMap.Explorer.Scene.Indices
{
    /// <summary> Composes multiply mesh indices. </summary>
    internal class CompositeMeshIndex : IMeshIndex
    {
        private readonly IMeshIndex[] _indices;

        private int _nextIndex;

        /// <summary> Creates instance of <see cref="CompositeMeshIndex"/>. </summary>
        public CompositeMeshIndex(int count)
        {
            _indices = new IMeshIndex[count];
        }

        /// <inheritdoc />
        public void Build()
        {
        }

        /// <inheritdoc />
        public MeshQuery.Result Modify(MeshQuery query)
        {
            var result = new MeshQuery.Result(query.Vertices);
            foreach (var meshIndex in _indices)
            {
                var intermediateResult = meshIndex.Modify(query);
                result.ModifiedVertices += intermediateResult.ModifiedVertices;
                result.DestroyedVertices += intermediateResult.DestroyedVertices;
                result.ScannedTriangles += intermediateResult.ScannedTriangles;
                result.IsDestroyed |= intermediateResult.IsDestroyed;
            }
            return result;
        }

        /// <summary> Adds mesh index. </summary>
        public CompositeMeshIndex AddMeshIndex(IMeshIndex index)
        {
            _indices[_nextIndex++] = index;
            return this;
        }
    }
}