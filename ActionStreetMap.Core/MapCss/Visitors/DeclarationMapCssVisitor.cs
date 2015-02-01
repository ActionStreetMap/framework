using System;
using ActionStreetMap.Core.MapCss.Domain;
using ActionStreetMap.Core.MapCss.Visitors.Eval;
using Antlr.Runtime.Tree;

namespace ActionStreetMap.Core.MapCss.Visitors
{
    /// <summary> Provides logic to parse declarations. </summary>
    internal class DeclarationMapCssVisitor: MapCssVisitorBase
    {
        private readonly bool _canUseExprTree;

        /// <summary> Creates DeclarationMapCssVisitor. </summary>
        /// <param name="canUseExprTree">True if platform supports expression trees.</param>
        public DeclarationMapCssVisitor(bool canUseExprTree)
        {
            _canUseExprTree = canUseExprTree;
        }

        /// <inheritdoc />
        public override Declaration VisitDeclaration(CommonTree declarationTree)
        {
            var declaration = new Declaration();
            if (declarationTree == null)
            {
                throw new MapCssFormatException("Declaration tree not valid!");
            }

            declaration.Qualifier = String.Intern(declarationTree.Children[0].Text);
            declaration.Value = String.Intern(declarationTree.Children[1].Text);

            if (declaration.Value == "EVAL_CALL")
            {
                declaration.IsEval = true;
                declaration.Evaluator =_canUseExprTree ?
                     new ExpressionEvalTreeWalker(declarationTree.Children[1] as CommonTree) :
                     (ITreeWalker) new StringEvalTreeWalker(declarationTree.Children[1] as CommonTree);
            }

            if (declaration.Value == "VALUE_RGB")
            {
                declaration.IsEval = true;
                declaration.Evaluator = new ColorTreeWalker(declarationTree.Children[1] as CommonTree);
            }

            if (declaration.Value == "VALUE_LIST")
            {
                declaration.IsEval = true;
                declaration.Evaluator = new ListTreeWalker(declarationTree.Children[1] as CommonTree);
            }

            return declaration;
        }
    }
}
