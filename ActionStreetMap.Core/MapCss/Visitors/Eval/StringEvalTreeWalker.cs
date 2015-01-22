using System;
using System.Collections.Generic;
using System.Linq;
using ActionStreetMap.Core.Tiling.Models;
using ActionStreetMap.Core.Utilities;
using Antlr.Runtime.Tree;

namespace ActionStreetMap.Core.MapCss.Visitors.Eval
{
    /// <summary>
    ///     Provides workaround to process eval expressions for platforms which doesn't support expression trees (e.g. web player).
    /// </summary>
    public class StringEvalTreeWalker: ITreeWalker
    {
        private readonly CommonTree _tree;
        private OperationStack _opStack;

        /// <summary>
        ///     Creates StringEvalTreeWalker
        /// </summary>
        /// <param name="tree">Parse tree.</param>
        public StringEvalTreeWalker(CommonTree tree)
        {
            _tree = tree;
        }

        /// <inheritdoc />
        public T Walk<T>(Model model)
        {
            _opStack = new OperationStack(model);
            var operation = _tree.Children[0] as CommonTree;
            
            VisitOperation(operation);

            return (T) _opStack.Pop();
        }

        private void VisitOperation(CommonTree tree)
        {
            switch (tree.ChildCount)
            {
                case 0:
                    VisitLeaf(tree);
                    break;
                case 1:
                    VisitUnary(tree);
                    break;
                case 2:
                    VisitBinary(tree);
                    break;
                default:
                    VisitMulti(tree);
                    break;
            }
        }

        private void VisitUnary(CommonTree tree)
        {
            if (tree.ChildCount > 0)
            {
                VisitOperation(tree.Children[0] as CommonTree);
                _opStack.PushUnary(String.Intern(tree.Text));
            }
            else
            {
                VisitLeaf(tree);
            }
        }

        private void VisitBinary(CommonTree binaryTree)
        {
            VisitOperation(binaryTree.Children[0] as CommonTree);
            var previousOperation = _opStack.Pop();
            VisitOperation(binaryTree.Children[1] as CommonTree);
            _opStack.Push(previousOperation);
            _opStack.PushBinary(String.Intern(binaryTree.Text));
        }


        private void VisitLeaf(CommonTree tree)
        {
            _opStack.PushConstant(String.Intern(tree.Text));
        }

        private void VisitMulti(CommonTree tree)
        {
            throw new NotImplementedException();
        }

        #region Operation stack

        private class OperationStack
        {
            private readonly Model _model;
            private readonly Stack<object> _expressions = new Stack<object>();

            public OperationStack(Model model)
            {
                _model = model;
            }

            public void Push(object expression)
            {
                _expressions.Push(expression);
            }

            public void PushUnary(string opName)
            {
                switch (opName)
                {
                    case "tag":
                        PushTagSelector();
                        break;
                    case "num":
                        PushToNum();
                        break;
                    case "color":
                        PushToColor();
                        break;
                    default:
                        throw new NotSupportedException(String.Format("Unary operation {0} is not supported", opName));
                }
            }

            public void PushBinary(string opName)
            {
                var left =  Pop();
                var right = Pop();
                switch (opName)
                {
                    case "*":
                        PushMult(left, right);
                        break;
                    case "+":
                        PushAdd(left, right);
                        break;
                    case "OP_MINUS":
                        PushSub(left, right);
                        break;
                    default:
                        throw new NotSupportedException(String.Format("Binary operation {0} is not supported", opName));
                }
            }

            public void PushConstant(string value)
            {
                _expressions.Push(value);
            }

            public object Pop()
            {
                return _expressions.Pop();
            }

            #region Supported Operations

            private void PushMult(object left, object right)
            {
                var binaryExpr = SanitizeHelper.ParseFloat(left) * SanitizeHelper.ParseFloat(right);
                Push(binaryExpr);
            }

            private void PushSub(object left, object right)
            {
                var binaryExpr = SanitizeHelper.ParseFloat(left) - SanitizeHelper.ParseFloat(right);
                Push(binaryExpr);
            }

            private void PushAdd(object left, object right)
            {
                var binaryExpr = SanitizeHelper.ParseFloat(left) + SanitizeHelper.ParseFloat(right);
                Push(binaryExpr);
            }

            private void PushTagSelector()
            {
                var tagKey = (string) _expressions.Pop();
                _expressions.Push(_model.Tags.First(t => t.Key == tagKey).Value);
            }

            private void PushToNum()
            {
                _expressions.Push(SanitizeHelper.ParseFloat(Pop()));
            }

            private void PushToColor()
            {
                _expressions.Push(ColorUtility.FromUnknown((string)Pop()));
            }

            #endregion
        }

        #endregion
    }
}
