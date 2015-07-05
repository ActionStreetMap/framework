using System;

namespace ActionStreetMap.Core.Geometry.Triangle
{
    /// <summary> Controls the behavior of the meshing software. </summary>
    internal class Behavior
    {
        private bool poly;
        private bool quality;
        private bool varArea;
        private bool convex;
        private bool jettison;
        private bool boundaryMarkers = true;
        private bool noHoles;
        private bool conformDel;

        private Func<Topology.Triangle, double, bool> usertest;

        private int noBisect;
        private int steiner = -1;

        private double minAngle;
        private double maxAngle;
        private double maxArea = -1.0;

        internal bool fixedArea;
        internal bool useSegments = true;
        internal double goodAngle;
        internal double maxGoodAngle;
        internal double offconstant;

        /// <summary> Creates an instance of the Behavior class. </summary>
        public Behavior(bool quality = false, double minAngle = 20.0)
        {
            if (quality)
            {
                this.quality = true;
                this.minAngle = minAngle;

                Update();
            }
        }

        /// <summary> Update quality options dependencies. </summary>
        private void Update()
        {
            quality = true;

            if (minAngle < 0 || minAngle > 60)
            {
                minAngle = 0;
                quality = false;
            }

            if ((maxAngle != 0.0) && maxAngle < 90 || maxAngle > 180)
            {
                maxAngle = 0;
                quality = false;
            }

            useSegments = Poly || Quality || Convex;
            goodAngle = Math.Cos(MinAngle*Math.PI/180.0);
            maxGoodAngle = Math.Cos(MaxAngle*Math.PI/180.0);

            offconstant = goodAngle == 1.0 ? 0.0 : 
                0.475*Math.Sqrt((1.0 + goodAngle)/(1.0 - goodAngle));

            goodAngle *= goodAngle;
        }

        #region Static properties

        /// <summary> No exact arithmetic. </summary>
        public static bool NoExact { get; set; }

        #endregion

        #region Public properties

        /// <summary> Quality mesh generation. </summary>
        public bool Quality
        {
            get { return quality; }
            set
            {
                quality = value;
                if (quality)
                    Update();
            }
        }

        /// <summary> Minimum angle constraint. </summary>
        public double MinAngle
        {
            get { return minAngle; }
            set
            {
                minAngle = value;
                Update();
            }
        }

        /// <summary> Maximum angle constraint. </summary>
        public double MaxAngle
        {
            get { return maxAngle; }
            set
            {
                maxAngle = value;
                Update();
            }
        }

        /// <summary> Maximum area constraint. </summary>
        public double MaxArea
        {
            get { return maxArea; }
            set
            {
                maxArea = value;
                fixedArea = value > 0.0;
            }
        }

        /// <summary> Apply a maximum triangle area constraint. </summary>
        public bool VarArea
        {
            get { return varArea; }
            set { varArea = value; }
        }

        /// <summary> Input is a Planar Straight Line Graph. </summary>
        public bool Poly
        {
            get { return poly; }
            set { poly = value; }
        }

        /// <summary> Apply a user-defined triangle constraint. </summary>
        public Func<Topology.Triangle, double, bool> UserTest
        {
            get { return usertest; }
            set { usertest = value; }
        }

        /// <summary> Enclose the convex hull with segments. </summary>
        public bool Convex
        {
            get { return convex; }
            set { convex = value; }
        }

        /// <summary> Conforming Delaunay (all triangles are truly Delaunay). </summary>
        public bool ConformingDelaunay
        {
            get { return conformDel; }
            set { conformDel = value; }
        }

        /// <summary> Suppresses boundary segment splitting. </summary>
        /// <remarks>
        ///     0 = split segments
        ///     1 = no new vertices on the boundary
        ///     2 = prevent all segment splitting, including internal boundaries
        /// </remarks>
        public int NoBisect
        {
            get { return noBisect; }
            set
            {
                noBisect = value;
                if (noBisect < 0 || noBisect > 2)
                    noBisect = 0;
            }
        }

        /// <summary> Use maximum number of Steiner points. </summary>
        public int SteinerPoints
        {
            get { return steiner; }
            set { steiner = value; }
        }

        /// <summary> Compute boundary information. </summary>
        public bool UseBoundaryMarkers
        {
            get { return boundaryMarkers; }
            set { boundaryMarkers = value; }
        }

        /// <summary> Ignores holes in polygons. </summary>
        public bool NoHoles
        {
            get { return noHoles; }
            set { noHoles = value; }
        }

        /// <summary> Jettison unused vertices from output. </summary>
        public bool Jettison
        {
            get { return jettison; }
            set { jettison = value; }
        }

        #endregion

        internal void Reset()
        {
            poly = false;
            quality = false;
            varArea = false;
            convex = false;
            jettison = false;
            boundaryMarkers = true;
            noHoles = false;
            conformDel = false;

            usertest = null;
            noBisect = 0;
            steiner = -1;
            minAngle = 0;
            maxAngle = 0;
            maxArea = -1;

            fixedArea = false;
            useSegments = true;
            goodAngle = 0;
            maxGoodAngle = 0;
            offconstant = 0;
        }
    }
}