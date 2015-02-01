using ActionStreetMap.Core.MapCss.Domain;
using Antlr.Runtime.Tree;

namespace ActionStreetMap.Core.MapCss.Visitors
{
    /// <summary>  Provides null realization of IMapCssVisitor. </summary>
    internal class MapCssVisitorBase : IMapCssVisitor
    {
        /// <inheritdoc />
        public virtual Stylesheet Visit(CommonTree tree)
        {
            return null;
        }

        /// <inheritdoc />
        public virtual Style VisitStyle(CommonTree ruleTree)
        {
            return null;
        }

        /// <inheritdoc />
        public virtual Selector VisitSelector(CommonTree selectorTree, string selectorType)
        {
            return null;
        }

        /// <inheritdoc />
        public virtual Declaration VisitDeclaration(CommonTree declarationTree)
        {
            return null;
        }
    }
}