
using UnityEngine;

namespace ActionStreetMap.Unity.Wrappers
{
    /// <summary> Wraps unity gradient. </summary>
    public class GradientWrapper
    {
#if !CONSOLE
        private Gradient _gradient;

        public Color Evaluate(float time)
        {
            return _gradient.Evaluate(time);
        }

        public static GradientWrapper CreateFrom()
        {
            // TODO create unity gradient from args
            return new GradientWrapper();
        } 
#else
        public Color Evaluate(float time)
        {
            return Color.red;
        }

        public static GradientWrapper CreateFrom()
        {
            // TODO add valid args
            return new GradientWrapper();
        }
#endif
    }
}
