using ActionStreetMap.Core.Geometry.Clipping;
using ActionStreetMap.Infrastructure.Utilities;

namespace ActionStreetMap.Core.Scene.InDoor
{
    internal sealed class InDoorGeneratorSettings
    {
        public double MinimalWidthStep = 10;
        public double PreferedWidthStep = 100;
        public double VaaSizeHeight = 20;
        public double VaaSizeWidth = 40; // along skeleton edge

        public readonly double HalfTransitAreaWidth;
        public readonly double TransitAreaWidth;
        public readonly double MinimalArea;

        public readonly IObjectPool ObjectPool;
        public readonly Clipper Clipper;

        public InDoorGeneratorSettings(IObjectPool objectPool, Clipper clipper,
            double transitAreaWidth)
        {
            ObjectPool = objectPool;
            Clipper = clipper;

            TransitAreaWidth = transitAreaWidth;
            HalfTransitAreaWidth = transitAreaWidth/2;
            MinimalArea = TransitAreaWidth*TransitAreaWidth;
        }
    }
}