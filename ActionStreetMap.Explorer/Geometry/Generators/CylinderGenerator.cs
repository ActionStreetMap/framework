using System;
using UnityEngine;

namespace ActionStreetMap.Explorer.Geometry.Generators
{
    /// <summary> Creates cylinder. </summary>
    internal class CylinderGenerator: AbstractGenerator
    {
        private float _height = 10f;
        private float _radius = 2f;
        private float _maxSegmentHeight = 2f;
        private int _radialSegments = 5;
        private Vector3 _center;

        public CylinderGenerator(MeshData meshData)
            : base(meshData)
        {
        }

        public CylinderGenerator SetCenter(Vector3 center)
        {
            _center = center;
            return this;
        }

        public CylinderGenerator SetHeight(float height)
        {
            _height = height;
            return this;
        }

        public CylinderGenerator SetRadius(float radius)
        {
            _radius = radius;
            return this;
        }

        public CylinderGenerator SetRadialSegments(int radialSegments)
        {
            _radialSegments = radialSegments;
            return this;
        }

        public CylinderGenerator SetMaxSegmentHeight(float maxSegmentHeight)
        {
            _maxSegmentHeight = maxSegmentHeight;
            return this;
        }

        public override void Build()
        {
            int heightSegments = (int)Math.Ceiling(_height / _maxSegmentHeight);

            float heightStep = _height / heightSegments;
            float angleStep = 2 * Mathf.PI / _radialSegments;

            for (int j = 0; j < _radialSegments; j++)
            {
                float firstAngle = j * angleStep;
                float secondAngle = (j == _radialSegments - 1 ? 0 : j + 1) * angleStep;

                var first = new Vector2(_radius * Mathf.Cos(firstAngle), _radius * Mathf.Sin(firstAngle));
                var second = new Vector2(_radius * Mathf.Cos(secondAngle), _radius * Mathf.Sin(secondAngle));

                // bottom cap
                AddTriangle(_center,
                            new Vector3(second.x, 0, second.y),
                            new Vector3(first.x, 0, first.y));

                // top cap
                AddTriangle(new Vector3(_center.x, _center.y + _height, _center.z),
                            new Vector3(first.x, _height, first.y),
                            new Vector3(second.x, _height, second.y));

                for (int i = 0; i < heightSegments; i++)
                {
                    var bottomHeight = i * heightStep + _center.y;
                    var topHeight = (i + 1) * heightStep + _center.y;

                    var v0 = new Vector3(first.x + _center.x, bottomHeight, first.y + _center.y);
                    var v1 = new Vector3(second.x + _center.x, bottomHeight, second.y + _center.y);
                    var v2 = new Vector3(second.x + _center.x, topHeight, second.y + _center.y);
                    var v3 = new Vector3(first.x + _center.x, topHeight, first.y + _center.y);

                    AddTriangle(v0, v1, v2);
                    AddTriangle(v3, v0, v2);
                }
            }
        }
    }
}
