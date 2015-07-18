using System.Collections.Generic;
using ActionStreetMap.Core.Geometry;
using ActionStreetMap.Core.Geometry.Clipping;
using ActionStreetMap.Core.Geometry.StraightSkeleton;
using ActionStreetMap.Infrastructure.Utilities;

namespace ActionStreetMap.Core.Scene.InDoor
{
    internal sealed class InDoorGeneratorSettings
    {
        public double MinimalWidthStep = 1;
        public double PreferedWidthStep = 5;
        public double VaaSizeHeight = 2;
        public double VaaSizeWidth = 4; // along skeleton edge

        public double HalfTransitAreaWidth;
        public double TransitAreaWidth;
        public double MinimalArea;

        public readonly IObjectPool ObjectPool;
        public readonly Clipper Clipper;

        public Skeleton Skeleton;
        public List<Vector2d> Footprint;
        public List<List<Vector2d>> Holes;
        public List<KeyValuePair<int, double>> Doors;

        public InDoorGeneratorSettings(IObjectPool objectPool, Clipper clipper, Skeleton skeleton, 
            List<Vector2d> footprint, List<List<Vector2d>> holes, List<KeyValuePair<int, double>> doors, 
            double transitAreaWidth)
        {
            ObjectPool = objectPool;
            Clipper = clipper;

            Skeleton = skeleton;
            Footprint = footprint;
            Holes = holes;
            Doors = doors;

            TransitAreaWidth = transitAreaWidth;
            HalfTransitAreaWidth = transitAreaWidth/2;
            MinimalArea = TransitAreaWidth*TransitAreaWidth;
        }
    }
}