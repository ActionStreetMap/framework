using ActionStreetMap.Core.MapCss.Domain;
using Antlr.Runtime.Tree;

namespace ActionStreetMap.Core.MapCss.Visitors
{
    /// <summary> Visitor for MapCSS parser result. </summary>
    public interface IMapCssVisitor
    {
        /// <summary> Visits tree. </summary>
        /// <param name="tree"></param>
        /// <returns></returns>
        Stylesheet Visit(CommonTree tree);

        /// <summary> Visits rule tree. </summary>
        /// <param name="ruleTree">Rule tree.</param>
        /// <returns>Style.</returns>
        Style VisitStyle(CommonTree ruleTree);

        /// <summary> Visits selector tree. </summary>
        /// <param name="selectorTree">Selector tree.</param>
        /// <param name="selectorType">Selector type.</param>
        /// <returns>Selector.</returns>
        Selector VisitSelector(CommonTree selectorTree, string selectorType);

        /// <summary> Visits declaration tree. </summary>
        /// <param name="declarationTree">Declaration tree.</param>
        /// <returns>Declaration.</returns>
        Declaration VisitDeclaration(CommonTree declarationTree);
    }
}
