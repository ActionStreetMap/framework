
using System;
using System.Collections.Generic;
using System.Linq;
using ActionStreetMap.Core.Tiling.Models;
using ActionStreetMap.Core.Utilities;

namespace ActionStreetMap.Core.MapCss.Domain
{
    /// <summary>
    ///     Represents MapCSS selector.
    /// </summary>
    public abstract class Selector
    {
        /// <summary>
        ///     True if it's used for closed polygon. Applicable only for way.
        /// </summary>
        public bool IsClosed { get; set; }

        /// <summary>
        ///     Gets or sets tag.
        /// </summary>
        public string Tag { get; set; }

        /// <summary>
        ///     Gets or sets value.
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        ///     Gets or sets operation on tag.
        /// </summary>
        public string Operation { get; set; }

        /// <summary>
        ///     Checks whether model can be used with this selector.
        /// </summary>
        /// <param name="model">Model.</param>
        /// <returns> True if model can be used with this selector.</returns>
        public abstract bool IsApplicable(Model model);

        /// <summary>
        ///     Checks model.
        /// </summary>
        /// <typeparam name="T">Type of model.</typeparam>
        /// <param name="model">Model.</param>
        /// <returns>True if model can be used.</returns>
        protected bool CheckModel<T>(Model model) where T: Model
        {
            if (!(model is T))
                return false;

            return MatchTags(model);
        }

        /// <summary>
        ///     Mathes tags of given model.
        /// </summary>
        /// <param name="model">Model.</param>
        /// <returns>True if model is matched.</returns>
        protected bool MatchTags(Model model)
        {
            switch (Operation)
            {
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
                    throw new MapCssFormatException(String.Format("Unsupported selector operation: {0}", Operation));
            }
        }
    }

    #region Concret trivial implementations

    /// <summary>
    ///     Selector for Node.
    /// </summary>
    public class NodeSelector : Selector
    {
        /// <inheritdoc />
        public override bool IsApplicable(Model model)
        {
            return CheckModel<Node>(model);
        }
    }

    /// <summary>
    ///     Selector for Area.
    /// </summary>
    public class AreaSelector : Selector
    {
        /// <inheritdoc />
        public override bool IsApplicable(Model model)
        {
            return CheckModel<Area>(model);
        }
    }

    /// <summary>
    ///     Selector for Way.
    /// </summary>
    public class WaySelector : Selector
    {
        /// <inheritdoc />
        public override bool IsApplicable(Model model)
        {
            return IsClosed ? model.IsClosed: CheckModel<Way>(model);
        }
    }

    /// <summary>
    ///     Selector for canvas.
    /// </summary>
    public class CanvasSelector : Selector
    {
        /// <inheritdoc />
        public override bool IsApplicable(Model model)
        {
            return model is Canvas;
        }
    }

    /// <summary>
    ///     Composite selector which compares list of selectors using logical AND.
    /// </summary>
    public class AndSelector: Selector
    {
        private readonly IList<Selector> _selectors;

        /// <summary>
        ///     Creates AndSelector.
        /// </summary>
        /// <param name="selectors">List of selectors.</param>
        public AndSelector(IList<Selector> selectors)
        {
            _selectors = selectors;
        }

        /// <inheritdoc />
        public override bool IsApplicable(Model model)
        {
            return _selectors.All(s => s.IsApplicable(model));
        }
    }

    #endregion
}
