using System;
using System.Collections.Generic;
using ActionStreetMap.Core.Tiling.Models;
using ActionStreetMap.Core.Utils;
using ActionStreetMap.Infrastructure.Primitives;

namespace ActionStreetMap.Core.MapCss.Domain
{
    /// <summary> Represents MapCSS selector. </summary>
    public abstract class Selector
    {
        /// <summary> True if it's used for closed polygon. Applicable only for way. </summary>
        public bool IsClosed { get; set; }

        /// <summary> Zoom level. </summary>
        public Range<int> Zoom { get; set; }

        /// <summary> Gets or sets tag. </summary>
        public string Tag { get; set; }

        /// <summary> Gets or sets value. </summary>
        public string Value { get; set; }

        /// <summary> Gets or sets operation on tag. </summary>
        public string Operation { get; set; }

        /// <summary> Checks whether model can be used with this selector. </summary>
        /// <param name="model">Model.</param>
        /// <param name="zoomLevel">Current zoom level.</param>
        /// <returns> True if model can be used with this selector.</returns>
        public abstract bool IsApplicable(Model model, int zoomLevel);

        /// <summary> Checks model. </summary>
        /// <typeparam name="T">Type of model.</typeparam>
        /// <param name="model">Model.</param>
        /// <param name="zoomLevel">Current zoom level.</param>
        /// <returns>True if model can be used.</returns>
        protected bool CheckModel<T>(Model model, int zoomLevel) where T : Model
        {
            if (!(model is T))
                return false;

            return MatchTags(model, zoomLevel);
        }

        /// <summary> Matches tags of given model. </summary>
        /// <param name="model">Model.</param>
        /// <param name="zoomLevel">Current zoom level.</param>
        /// <returns>True if model is matched.</returns>
        protected bool MatchTags(Model model, int zoomLevel)
        {
            switch (Operation)
            {
                case MapCssStrings.OperationZoom:
                    return Zoom.Contains(zoomLevel);
                case MapCssStrings.OperationExist:
                    return model.Tags.ContainsKey(Tag);
                case MapCssStrings.OperationNotExist:
                    return !model.Tags.ContainsKey(Tag);
                case MapCssStrings.OperationEquals:
                    return model.Tags.ContainsKeyValue(Tag, Value);
                case MapCssStrings.OperationNotEquals:
                    return model.Tags.IsNotEqual(Tag, Value);
                case MapCssStrings.OperationLess:
                    return model.Tags.IsLess(Tag, Value);
                case MapCssStrings.OperationGreater:
                    return model.Tags.IsGreater(Tag, Value);
                default:
                    throw new MapCssFormatException(
                        String.Format(Strings.MapCssUnsupportedSelectorOperation, Operation));
            }
        }
    }

    #region Concrete trivial implementations

    /// <summary> Selector for Node. </summary>
    internal class NodeSelector : Selector
    {
        /// <inheritdoc />
        public override bool IsApplicable(Model model, int zoomLevel)
        {
            return CheckModel<Node>(model, zoomLevel);
        }
    }

    /// <summary> Selector for Area. </summary>
    internal class AreaSelector : Selector
    {
        /// <inheritdoc />
        public override bool IsApplicable(Model model, int zoomLevel)
        {
            return CheckModel<Area>(model, zoomLevel);
        }
    }

    /// <summary> Selector for Way. </summary>
    internal class WaySelector : Selector
    {
        /// <inheritdoc />
        public override bool IsApplicable(Model model, int zoomLevel)
        {
            return IsClosed ? model.IsClosed : CheckModel<Way>(model, zoomLevel);
        }
    }

    /// <summary> Selector for canvas. </summary>
    internal class CanvasSelector : Selector
    {
        /// <inheritdoc />
        public override bool IsApplicable(Model model, int zoomLevel)
        {
            return model is Canvas;
        }
    }

    /// <summary> Composite selector which compares list of selectors using logical AND. </summary>
    internal class AndSelector: Selector
    {
        internal readonly List<Selector> Selectors;

        /// <summary> Creates AndSelector. </summary>
        /// <param name="selectors">List of selectors.</param>
        public AndSelector(List<Selector> selectors)
        {
            Selectors = selectors;
        }

        /// <inheritdoc />
        public override bool IsApplicable(Model model, int zoomLevel)
        {
            // just the same as Selectors.All(s => s.IsApplicable(model));
            foreach (var selector in Selectors)
                if (!selector.IsApplicable(model, zoomLevel))
                    return false;

            return true;
        }
    }

    #endregion
}
