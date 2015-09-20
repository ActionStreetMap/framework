using System;
using ActionStreetMap.Explorer.Utils;
using ActionStreetMap.Unity.Wrappers;
using UnityEngine;

namespace ActionStreetMap.Explorer.Scene.Facades
{
    internal abstract class WallBuilder
    {
        #region External parameters

        protected int VertStartIndex;
        protected float Height = 10;
        protected float MinHeight = 0;
        protected float MinStepWidth = 2f;
        protected float MinStepHeight = 2;
        protected GradientWrapper Gradient;

        protected Vector3[] Vertices;
        protected int[] Triangles;
        protected Color[] Colors;
        protected Vector2[] UVs;

        protected int HalfVertCount;
        protected MeshData MeshData;

        #endregion

        #region Internal parameters

        protected int XIterCount;
        protected int YIterCount;

        protected float XStep;
        protected float YStep;

        protected Vector3 Start;
        protected Vector3 End;

        protected Vector3 Direction;
        protected float Distance;


        #endregion

        /// <summary> Precalculates vertex count. Should be used before Build method is called. </summary>
        public abstract int CalculateVertexCount(Vector3 start, Vector3 end);

        /// <summary> Builds segment. </summary>
        protected abstract void BuildSegment(int i, int j, float yStart);

        /// <summary> Builds wall between points. </summary>
        public virtual void Build(Vector3 start, Vector3 end)
        {
            Start = start;
            End = end;

            Direction = (end - start);
            Direction.Normalize();

            Distance = Vector3.Distance(start, end);

            XIterCount = (int)Math.Ceiling(Distance / MinStepWidth);
            YIterCount = (int)Math.Ceiling(Height / MinStepHeight);

            XStep = Distance / XIterCount;
            YStep = Height / YIterCount;

            OnParametersCalculated();

            for (int j = 0; j < YIterCount; j++)
            {
                var yStart = MinHeight + j * YStep;

                for (int i = 0; i < XIterCount; i++)
                    BuildSegment(i, j, yStart);
            }
        }

        /// <summary> Called after wall parameters are calculated. </summary>
        protected virtual void OnParametersCalculated()
        {
        }

        /// <summary> Returns color for given vertex. </summary>
        protected Color GetColor(Vector3 point)
        {
            var value = (Noise.Perlin3D(point, 0.0f) + 1f) / 2f;
            return Gradient.Evaluate(value);
        }

        #region Setters

        public WallBuilder SetMeshData(MeshData meshData)
        {
            Vertices = meshData.Vertices;
            Triangles = meshData.Triangles;
            Colors = meshData.Colors;
            UVs = meshData.UVs;

            MeshData = meshData;
            HalfVertCount = Vertices.Length / 2;

            return this;
        }

        public WallBuilder SetStartIndex(int index)
        {
            VertStartIndex = index;
            return this;
        }

        public WallBuilder SetHeight(float height)
        {
            Height = height;
            return this;
        }

        public WallBuilder SetMinHeight(float minHeight)
        {
            MinHeight = minHeight;
            return this;
        }

        public WallBuilder SetStepWidth(float stepWidth)
        {
            MinStepWidth = stepWidth;
            return this;
        }

        public WallBuilder SetStepHeight(float stepHeight)
        {
            MinStepHeight = stepHeight;
            return this;
        }

        public WallBuilder SetGradient(GradientWrapper gradient)
        {
            Gradient = gradient;
            return this;
        }

        #endregion
    }
}
