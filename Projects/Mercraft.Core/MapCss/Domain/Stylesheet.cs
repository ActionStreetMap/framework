﻿using Mercraft.Core.Scene.Models;

namespace Mercraft.Core.MapCss.Domain
{
    /// <summary>
    ///     Represents Stylesheet - collection of styles.
    /// </summary>
    public class Stylesheet
    {
        private readonly StyleCollection _styles = new StyleCollection();

        /// <summary>
        ///     Adds style to collection.
        /// </summary>
        /// <param name="style">Style.</param>
        public void AddStyle(Style style)
        {
            _styles.Add(style);
        }

        /// <summary>
        ///     Count of styles in collection.
        /// </summary>
        public int Count
        {
            get
            {
                return _styles.Count;
            }
        }

        /// <summary>
        ///     Gets rule for model.
        /// </summary>
        /// <param name="model">Model.</param>
        /// <returns>Rule.</returns>
        public Rule GetModelRule(Model model)
        {
            return _styles.GetMergedRule(model);
        }

        /// <summary>
        ///     Gets Rule for canvas.
        /// </summary>
        /// <param name="canvas">Canvas.</param>
        /// <returns>Rule.</returns>
        public Rule GetCanvasRule(Canvas canvas)
        {
            return _styles.GetCollectedRule(canvas);
        }

        /// <summary>
        ///     Returns Rule to object pool.
        /// </summary>
        /// <param name="rule">Rule.</param>
        public void StoreRule(Rule rule)
        {
            _styles.StoreRule(rule);
        }
    }
}
