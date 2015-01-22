using System.Collections.Generic;
using System.Linq;
using ActionStreetMap.Core.Tiling.Models;

namespace ActionStreetMap.Core.MapCss.Domain
{
    /// <summary>
    ///     Contains some performance optimizations for rule processing
    /// </summary>
    internal class StyleCollection
    {
        private readonly List<Style> _canvasStyles = new List<Style>(1);
        private readonly List<Style> _areaStyles = new List<Style>(16);
        private readonly List<Style> _wayStyles = new List<Style>(16);
        private readonly List<Style> _nodeStyles = new List<Style>(64);

        private readonly List<Style>  _combinedStyles = new List<Style>(16);

        private int _count;
        public int Count { get { return _count; } }

        public void Add(Style style)
        {
            // NOTE store different styles in different collections to increase
            // lookup performance. However, there are two limitations:
            // 1. style order is resorted by type
            // 2. Combined styles (logical AND variations) in new collection now and will be processed last
            if (style.Selectors.All(s => s is NodeSelector))
                _nodeStyles.Add(style);
            else if (style.Selectors.All(s => s is AreaSelector))
                _areaStyles.Add(style);
            else if (style.Selectors.All(s => s is WaySelector))
                _wayStyles.Add(style);
            else if (style.Selectors.All(s => s is CanvasSelector))
                _canvasStyles.Add(style);
            else
                _combinedStyles.Add(style);

            _count++;
        }

        public Rule GetMergedRule(Model model)
        {
            var styles = GetModelStyles(model);
            var rule = new Rule(model);
            for (int i = 0; i < styles.Count; i++)
                MergeDeclarations(styles[i], rule, model);

            for (int i = 0; i < _combinedStyles.Count; i++)
                MergeDeclarations(_combinedStyles[i], rule, model);

            return rule;
        }

        public Rule GetCollectedRule(Model model)
        {
            var styles = GetModelStyles(model);
            var rule = new Rule(model);
            for (int i = 0; i < styles.Count; i++)
                CollectDeclarations(styles[i], rule, model);

            for (int i = 0; i < _combinedStyles.Count; i++)
                CollectDeclarations(_combinedStyles[i], rule, model);

            return rule;
        }

        private List<Style> GetModelStyles(Model model)
        {
            if (model is Node)
                return _nodeStyles;

            if (model is Area)
                return _areaStyles;

            if (model is Way)
                return _wayStyles;

            return _canvasStyles;
        }

        #region Declaration processing

        private void MergeDeclarations(Style style, Rule rule, Model model)
        {
            if (!style.IsApplicable(model))
                return;

            // NOTE This can be nicely done by LINQ intesection extension method
            // but this peace of code is performance critical
            // TODO check whether we have here allocations due to foreach
            foreach (var key in style.Declarations.Keys)
            {
                var styleDeclaration = style.Declarations[key];
                if (rule.Declarations.ContainsKey(key))
                {
                    var declaration = rule.Declarations[key];
                    declaration.Value = styleDeclaration.Value;
                    declaration.Evaluator = styleDeclaration.Evaluator;
                    declaration.IsEval = styleDeclaration.IsEval;
                }
                else
                {
                    // Should copy Declaration
                    rule.Declarations.Add(key, new Declaration
                    {
                        Qualifier = styleDeclaration.Qualifier,
                        Value = styleDeclaration.Value,
                        Evaluator = styleDeclaration.Evaluator,
                        IsEval = styleDeclaration.IsEval
                    });
                }
            }
        }

        private void CollectDeclarations(Style style, Rule rule, Model model)
        {
            if (!style.IsApplicable(model))
                return;

            foreach (var keyValue in style.Declarations)
                rule.Declarations.Add(keyValue.Key, keyValue.Value);
        }

        #endregion
    }
}
