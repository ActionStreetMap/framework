using ActionStreetMap.Unity.Wrappers;
using UnityEngine;

namespace ActionStreetMap.Explorer.Scene.Generators
{
    /// <summary> Builds tree. </summary>
    internal class TreeGenerator : AbstractGenerator
    {
        private Vector3 _position;
        private GradientWrapper _trunkGradient;
        private GradientWrapper _foliageGradient;
        private float _trunkHeight = 2f;
        private float _trunkRadius = 0.2f;
        private float _foliageRadius = 2.5f;

        private IcoSphereGenerator _foliageGen;
        private CylinderGenerator _trunkGen;


        public TreeGenerator SetPosition(Vector3 position)
        {
            _position = position;
            return this;
        }

        public TreeGenerator SetTrunkGradient(GradientWrapper gradient)
        {
            _trunkGradient = gradient;
            return this;
        }

        public TreeGenerator SetFoliageGradient(GradientWrapper gradient)
        {
            _foliageGradient = gradient;
            return this;
        }

        public TreeGenerator SetTrunkHeight(float height)
        {
            _trunkHeight = height;
            return this;
        }

        public TreeGenerator SetTrunkRadius(float radius)
        {
            _trunkRadius = radius;
            return this;
        }

        public TreeGenerator SetFoliageRadius(float radius)
        {
            _foliageRadius = radius;
            return this;
        }

        public override int CalculateVertexCount()
        {
            _trunkGen = new CylinderGenerator();
            var trunkCount = _trunkGen.SetCenter(_position)
                .SetHeight(_trunkHeight)
                .SetRadius(_trunkRadius)
                .SetRadialSegments(7)
                .SetVertexNoiseFreq(0.4f)
                .SetGradient(_trunkGradient)
                .CalculateVertexCount();

            _foliageGen = new IcoSphereGenerator();
            var foliageCount = _foliageGen.SetCenter(
                 new Vector3(_position.x, _position.y + _trunkHeight + _foliageRadius * 0.9f, _position.z))
                .SetRadius(_foliageRadius)
                .SetRecursionLevel(1)
                .SetVertexNoiseFreq(2f)
                .SetGradient(_foliageGradient)
                .CalculateVertexCount();

            return trunkCount + foliageCount;
        }

        public override void Build(MeshData meshData)
        {
            // generate trunk
            _trunkGen.Build(meshData);

            // generate foliage
            _foliageGen.Build(meshData);
        }
    }
}