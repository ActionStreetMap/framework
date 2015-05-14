using System.Collections.Generic;
using System.Linq;
using ActionStreetMap.Core.MapCss.Domain;
using Antlr.Runtime.Tree;

namespace ActionStreetMap.Core.MapCss.Visitors
{
    /// <summary> Defines defaul mapcss visitor. </summary>
    internal class MapCssVisitor : IMapCssVisitor
    {
        private readonly List<IMapCssVisitor> _visitors;

        /// <summary> Creates mapcss visitor. </summary>
        /// <param name="canUseExprTree">True if current API supports expression trees.</param>
        public MapCssVisitor(bool canUseExprTree)
        {
            _visitors = new List<IMapCssVisitor>
            {
                new SelectorMapCssVisitor(),
                new DeclarationMapCssVisitor(canUseExprTree)
            };
        }

        /// <inheritdoc />
        public Stylesheet Visit(CommonTree tree)
        {
            var stylesheet = new Stylesheet();
            foreach (CommonTree child in tree.Children)
            {
                if (child.Text == MapCssStrings.Rule)
                {
                    var style = VisitStyle(child);
                    stylesheet.AddStyle(style);
                }
            }

            return stylesheet;
        }

        /// <inheritdoc />
        public Style VisitStyle(CommonTree ruleTree)
        {
            var style = new Style();
            for (int i = 0; i < ruleTree.Children.Count; i++)
            {
                var tree = ruleTree.Children[i] as CommonTree;
                if (tree == null)
                    throw new MapCssFormatException(Strings.StyleVisitNullTree);

                if (tree.Text == MapCssStrings.SimpleSelector)
                {
                    var selectorType = (tree.Children[0] as CommonTree).Text;

                    // NOTE canvas is special case: it doesn't have selectors
                    // but we want to use it later to be consistent
                    if (selectorType != MapCssStrings.CanvasSelector)
                    {
                        int selectorIdx = 1;
                        var selectors = new List<Selector>();
                        while (tree.ChildCount > selectorIdx)
                        {
                            var selectorTree = tree.Children[selectorIdx] as CommonTree;
                            selectors.Add(VisitSelector(selectorTree, selectorType));
                            selectorIdx++;
                        }
                        style.Selectors.Add(selectors.Count > 1
                            ? new AndSelector(selectors)
                            : selectors.Single());
                    }
                    else
                    {
                         style.Selectors.Add(new CanvasSelector());
                    }
                }
                else
                {
                    style.MatchAll = i == 1;
                    
                    // declarations
                    if (tree.Text == "{")
                    {
                        int declarationSelectorIdx = 0;
                        while (tree.ChildCount > declarationSelectorIdx &&
                            tree.Children[declarationSelectorIdx].Text == MapCssStrings.Declaration)
                        {
                            var declarationTree = tree.Children[declarationSelectorIdx] as CommonTree;
                            var declaration = VisitDeclaration(declarationTree);
                            if (declaration.Value == MapCssStrings.ValueListDeclaration)
                            {
                                ListDeclaration listDeclaration;
                                if (style.Declarations.ContainsKey(declaration.Qualifier))
                                    // NOTE we don't expect duplications of list declaration without VALUE_LIST
                                    listDeclaration = (ListDeclaration) style.Declarations[declaration.Qualifier];
                                else
                                {
                                    listDeclaration = new ListDeclaration();
                                    style.Declarations.Add(declaration.Qualifier, listDeclaration);
                                }
                                listDeclaration.Items.Add(declaration);
                            }
                            else
                            {
                                style.Declarations.Add(declaration.Qualifier, declaration);
                            }
                            declarationSelectorIdx++;
                        }
                    }
                }
            }
            return style;
        }

        /// <inheritdoc />
        public Selector VisitSelector(CommonTree selectorTree, string selectorType)
        {
            return _visitors.Select(visitor => visitor.VisitSelector(selectorTree, selectorType))
                            .FirstOrDefault(declaration => declaration != null);
        }

        /// <inheritdoc />
        public Declaration VisitDeclaration(CommonTree declarationTree)
        {
            return _visitors.Select(visitor => visitor.VisitDeclaration(declarationTree))
                            .FirstOrDefault(declaration => declaration != null);
        }
    }
}
