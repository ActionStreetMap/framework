using System;
using ActionStreetMap.Core.MapCss.Domain;
using ActionStreetMap.Infrastructure.Primitives;
using Antlr.Runtime.Tree;

namespace ActionStreetMap.Core.MapCss.Visitors
{
    /// <summary>  Selector visitor. </summary>
    internal class SelectorMapCssVisitor : MapCssVisitorBase
    {
        /// <inheritdoc />
        public override Selector VisitSelector(CommonTree selectorTree, string selectorType)
        {
            Selector selector;
            switch (selectorType)
            {
                case MapCssStrings.NodeSelector:
                    selector = new NodeSelector();
                    break;
                case MapCssStrings.WaySelector:
                    selector = new WaySelector();
                    break;
                case MapCssStrings.AreaSelector:
                    selector = new AreaSelector();
                    break;
                case MapCssStrings.CanvasSelector:
                    selector = new CanvasSelector();
                    break;
                default:
                    throw new MapCssFormatException(selectorTree,
                        String.Format(Strings.MapCssUnknownSelectorType, selectorType));
            }

            var operation = selectorTree.Children[0].Text;
            ParseOperation(selectorTree, selector, operation);

            return selector;
        }

        /// <summary> Processes selector definition. </summary>
        private void ParseOperation(CommonTree selectorTree, Selector selector, string operation)
        {
            selector.Operation = String.Intern(operation);

            // special pseudo selector class like area[building]:closed
            if (selectorTree.Text == MapCssStrings.PseudoClassSelector)
            {
                var pseudoClass = selectorTree.Children[1].Text;
                if (pseudoClass == "closed")
                {
                    selector.IsClosed = true;
                }
            }
            // existing selector case
            else if (operation == MapCssStrings.OperationExist ||
                operation == MapCssStrings.OperationNotExist)
            {
                if (selectorTree.ChildCount != 2)
                {
                    throw new MapCssFormatException(selectorTree, Strings.MapCssInvalidExistOperation);
                }
                selector.Tag = String.Intern(selectorTree.Children[1].Text);
            }
            // zoom selector
            else if (selectorTree.Text == MapCssStrings.ZoomSelector)
            {
                var min = int.Parse(selectorTree.Children[0].Text);
                var max = int.Parse(selectorTree.Children[1].Text);
                selector.Zoom = new Range<int>(min, max);
                selector.Operation = MapCssStrings.OperationZoom;
            }
            // Various selector operation like equals
            else
            {
                if (selectorTree.ChildCount != 3)
                {
                    throw new MapCssFormatException(selectorTree,
                        String.Format(Strings.MapCssInvalidSelectorOperation, operation));
                }

                selector.Tag = String.Intern(selectorTree.Children[1].Text);
                selector.Value = String.Intern(selectorTree.Children[2].Text);
            }
        }
    }
}