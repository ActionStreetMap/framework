using ActionStreetMap.Explorer.Utils;
using ActionStreetMap.Unity.Wrappers;
using UnityEngine;

namespace ActionStreetMap.Explorer.Terrain.Flat
{
    /// <summary> Provides the way to build mesh which represents terrain part. </summary>
    internal class TerrainCellBuilder
    {
        private readonly float _size;
        private readonly int _resolution;

        private readonly Vector3[] _vertices;
        private readonly Color[] _colors;
        private readonly int[] _triangles;

        private Vector2 _position;

        /// <summary> Creates instance of <see cref="TerrainCellBuilder"/>. </summary>
        /// <param name="size">Size of cell.</param>
        /// <param name="resolution">Resolution of cell.</param>
        public TerrainCellBuilder(float size, int resolution)
        {
            _size = size;
            _resolution = resolution;

            // NOTE all vertecies are duplicated as it's only one way to create 
            // flat shading effect (except geometric shader)
            int vertexCount = _resolution * _resolution * 6;
            _vertices = new Vector3[vertexCount];
            _colors = new Color[vertexCount];
            _triangles = new int[vertexCount];
        }

        /// <summary> Builds terrain sub title data at given position with given terrain data. Non thread-safe. </summary>
        /// <param name="position">Left bottom position. </param>
        /// <param name="heightmap">Heightmap data. </param>
        /// <param name="xOffset">X-axis offset in heightmap array.</param>
        /// <param name="yOffset">Y-axis offset in heightmap array.</param>
        /// <param name="defaultGradient">Initial grid color.</param>
        public void Move(Vector2 position, float[,] heightmap, int xOffset, int yOffset, GradientWrapper defaultGradient)
        {
            _position = position;
            Create(heightmap, xOffset, yOffset, defaultGradient);
        }

        /// <summary> Populates given mesh with terrain data. </summary>
        public void Update(Mesh meshData)
        {
            meshData.vertices = _vertices;
            meshData.triangles = _triangles;
            meshData.colors = _colors;
            meshData.RecalculateNormals();
        }

        private void Create(float[,] heightmap, int xOffset, int yOffset, GradientWrapper defaultGradient)
        {
            int index = 0;
            var stepSize = _size / _resolution;
            for (int y = 0; y < _resolution; y++)
            {
                for (int x = 0; x < _resolution; x++)
                {
                    // get elevation data
                    // add axis offset as heightmap is created for whole tile.
                    var height00 = heightmap[y + yOffset, x + xOffset];
                    var height10 = heightmap[y + yOffset, x + 1 + xOffset];
                    var height01 = heightmap[y + yOffset + 1, x + xOffset];
                    var height11 = heightmap[y + yOffset + 1, x + 1 + xOffset];

                    // first triangle
                    var firstValue = (Noise.Perlin3D(new Vector3(_position.x + x * stepSize, height00, _position.y + y * stepSize), 0.2f) + 1f) * 0.5f;
                    var firstGradient = defaultGradient.Evaluate(firstValue);

                    _vertices[index] = new Vector3(_position.x + x * stepSize, height00, _position.y + y * stepSize);
                    _triangles[index] = index;
                    _colors[index++] = firstGradient;

                    _vertices[index] = new Vector3(_position.x + x * stepSize, height01, _position.y + (y + 1) * stepSize);
                    _triangles[index] = index;
                    _colors[index++] = firstGradient;

                    _vertices[index] = new Vector3(_position.x + (x + 1) * stepSize, height10, _position.y + y * stepSize);
                    _triangles[index] = index;
                    _colors[index++] = firstGradient;

                    // second triangle
                    var secondValue = (Noise.Perlin3D(new Vector3(_position.x + (x + 1) * stepSize, height00, _position.y + y * stepSize), 0.2f) + 1f) * 0.5f;
                    var secondGradient = defaultGradient.Evaluate(secondValue);

                    _vertices[index] = new Vector3(_position.x + (x + 1) * stepSize, height10, _position.y + y * stepSize);
                    _triangles[index] = index;
                    _colors[index++] = secondGradient;

                    _vertices[index] = new Vector3(_position.x + x * stepSize, height01, _position.y + (y + 1) * stepSize);
                    _triangles[index] = index;
                    _colors[index++] = secondGradient;

                    _vertices[index] = new Vector3(_position.x + (x + 1) * stepSize, height11, _position.y + (y + 1) * stepSize);
                    _triangles[index] = index;
                    _colors[index++] = secondGradient;
                }
            }
        }

        /// <summary> Fills color array using gradient provided. </summary>
        public void Fill(GradientWrapper gradient, int line, int start, int end)
        {
            var anchorY = line * _resolution * 6;
            for (int i = start; i <= end && i != _resolution; i++)
            {
                var anchorX = anchorY + i * 6;

                var value = (Noise.Perlin3D(new Vector3(start, line, end), 0.2f) + 1f) * 0.5f;
                var color = gradient.Evaluate(value);

                _colors[anchorX] = color;
                _colors[anchorX + 1] = color;
                _colors[anchorX + 2] = color;
                _colors[anchorX + 3] = color;
                _colors[anchorX + 4] = color;
                _colors[anchorX + 5] = color;
            }
        }
    }
}
