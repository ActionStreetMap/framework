using UnityEngine;

namespace ActionStreetMap.Explorer.Scene.Indices
{
    /// <summary> Mesh index for multiply planes in single mesh. </summary>
    internal sealed class MultiplyPlaneMeshIndex : PlaneMeshIndex
    {
        private Vector3[] _normals;
        private float[] _magnitudes;
        private float[] _coeffDs;
        private int[] _planeIndices;

        private int _vertexCount;
        private int _currentPlaneIndex;

        public MultiplyPlaneMeshIndex(int planeCount, int vertexCount)
        {
            _normals = new Vector3[planeCount];
            _magnitudes = new float[planeCount];
            _coeffDs = new float[planeCount];
            _planeIndices = new int[planeCount];
            _vertexCount = vertexCount;
        }

        /// <inheritdoc />
        public override MeshQuery.Result Modify(MeshQuery query)
        {
            int startIndex = 0;
            int destroyed = 0;
            int modified = 0;
            for (int j = 0; j < _planeIndices.Length; j++)
            {
                var endIndex = j == _planeIndices.Length - 1 ? _vertexCount : _planeIndices[j + 1];
                var n = _normals[j];
                var magnitude = _magnitudes[j];
                var d = _coeffDs[j];
                var result = base.Modify(query, startIndex, endIndex, n, magnitude, d);
                destroyed += result.DestroyedVertices;
                modified += result.ModifiedVertices;
            }

            return new MeshQuery.Result(query.Vertices)
            {
                ModifiedVertices = modified,
                DestroyedVertices = destroyed
            };
        }

        public void AddPlane(Vector3 v0, Vector3 v1, Vector3 v2, int startIndex)
        {
            Vector3 n;
            float magnitude;
            float d;
            CalculateParams(v0, v1, v2, out n, out magnitude, out d);

            _normals[_currentPlaneIndex] = n;
            _magnitudes[_currentPlaneIndex] = magnitude;
            _coeffDs[_currentPlaneIndex] = d;
            _planeIndices[_currentPlaneIndex] = startIndex;
            _currentPlaneIndex++;
        }
    }
}
