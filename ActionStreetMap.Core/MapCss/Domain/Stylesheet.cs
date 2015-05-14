using ActionStreetMap.Core.Tiling.Models;

namespace ActionStreetMap.Core.MapCss.Domain
{
    /// <summary> Represents Stylesheet - collection of styles. </summary>
    public class Stylesheet
    {
        private readonly StyleCollection _styles = new StyleCollection();

        /// <summary> Adds style to collection. </summary>
        /// <param name="style">Style.</param>
        public void AddStyle(Style style)
        {
            _styles.Add(style);
        }

        /// <summary> Count of styles in collection. </summary>
        public int Count { get { return _styles.Count; } }

        /// <summary> Gets rule for model. </summary>
        /// <param name="model">Model.</param>
        /// <param name="zoomLevel">Current zoom level.</param>
        /// <returns>Rule.</returns>
        public Rule GetModelRule(Model model, int zoomLevel = CoreConsts.MaxZoomLevel)
        {
            return _styles.GetMergedRule(model, zoomLevel);
        }

        /// <summary> Gets Rule for canvas. </summary>
        /// <param name="canvas">Canvas.</param>
        /// <returns>Rule.</returns>
        public Rule GetCanvasRule(Canvas canvas)
        {
            // NOTE zoom level should be ignored for canvas
            return _styles.GetCollectedRule(canvas, -1);
        }
    }
}
