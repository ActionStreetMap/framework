using System.Collections.Generic;
using ActionStreetMap.Core.Tiling.Models;

namespace ActionStreetMap.Core.MapCss.Domain
{
    /// <summary> Represents MapCSS style. </summary>
    public class Style
    {
        /// <summary> True if all selectors should be applicable to given model. </summary>
        public bool MatchAll { get; set; }

        /// <summary> List of selectors. </summary>
        public List<Selector> Selectors { get; set; }

        /// <summary> List of declarations. </summary>
        public Dictionary<string, Declaration> Declarations { get; set; }

        /// <summary> Creates empty Style. </summary>
        public Style()
        {
            Selectors = new List<Selector>();
            Declarations = new Dictionary<string, Declaration>(4);
        }

        /// <summary> Checks whether model is defined in style. </summary>
        /// <param name="model">Model.</param>
        /// <param name="zoomLevel">Current zoom level.</param>
        /// <returns>True if model is applicable.</returns>
        public bool IsApplicable(Model model, int zoomLevel)
        {
            // NOTE don't use LINQ here as it's performance critical code
            // we want to decrease heap allocations
            if (MatchAll)
            {
                // Selectors.All(s => s.IsApplicable(model));
                for (int i = 0; i < Selectors.Count; i++)
                {
                    if (!Selectors[i].IsApplicable(model, zoomLevel))
                        return false;
                }
                return true;
            }

            // any is applicable or closed as special case
            for (int i = 0; i < Selectors.Count; i++)
            {
                var selector = Selectors[i];
                if (selector.IsClosed && !selector.IsApplicable(model, zoomLevel))
                    return false;
                if (selector.IsApplicable(model, zoomLevel))
                    return true;
            }

            return false;
        }
    }
}