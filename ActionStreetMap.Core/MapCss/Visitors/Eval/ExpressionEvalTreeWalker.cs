using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using ActionStreetMap.Core.Tiling.Models;
using ActionStreetMap.Core.Unity;
using ActionStreetMap.Core.Utils;
using Antlr.Runtime.Tree;

namespace ActionStreetMap.Core.MapCss.Visitors.Eval
{
    /// <summary>
    ///     Naive implementation of Eval expression builder
    ///     Internally, builds expression from common tree which represents operations in prefix notation
    ///     Unfortunately, it can be used only on platform which support such features
    /// </summary>
    internal sealed class ExpressionEvalTreeWalker: ITreeWalker
    {
        private readonly object _lockObj = new object();

        private OperationStack _opStack;
        private ParameterExpression _param = Expression.Parameter(typeof(Model), "model");
        private CommonTree _tree;
        private object _compiledLambda;

        /// <summary> Creates ExpressionEvalTreeWalker. </summary>
        /// <param name="tree">Parse tree.</param>
        public ExpressionEvalTreeWalker(CommonTree tree)
        {
            _tree = tree;
            _opStack = new OperationStack(_param);
        }

        /// <inheritdoc />
        public T Walk<T>(Model model)
        {
            if (_compiledLambda != null)
                return (_compiledLambda as Func<Model, T>).Invoke(model);

            lock (_lockObj)
            {
                 if (_compiledLambda != null)
                     return (_compiledLambda as Func<Model, T>).Invoke(model);

                var operation = _tree.Children[0] as CommonTree;

                VisitOperation(operation);

                var expression = _opStack.Pop();

                Expression<Func<Model, T>> lambda = Expression.Lambda<Func<Model, T>>(
                    expression, new[] {_param});

                var compiledLambda = lambda.Compile();

                // cache compiled lambda expression
                _compiledLambda = compiledLambda;
                // clear state
                _tree = null;
                _param = null;
                _opStack = null;
                return compiledLambda.Invoke(model);
            }
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

        #region Operation stack which uses expressions

        private class OperationStack
        {
            private readonly ParameterExpression _param;
            private readonly Stack<Expression> _expressions = new Stack<Expression>();

            public OperationStack(ParameterExpression param)
            {
                _param = param;
            }

            public void Push(Expression expression)
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
                    case "gradient":
                        PushToGradient();
                        break;
                    default:
                        throw new NotSupportedException(String.Format("Unary operation {0} is not supported", opName));
                }

                bool wrap = Count() > 1;
                if (wrap)
                {
                    var outer = Pop();
                    var inner = Pop();
                    Push(Expression.Invoke(outer, inner));
                }
            }

            public void PushBinary(string opName)
            {
                var left = Pop();
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
                _expressions.Push(Expression.Constant(value));
            }

            public Expression Pop()
            {
                return _expressions.Pop();
            }

            private int Count()
            {
                return _expressions.Count;
            }

            #region Supported Operations

            private void PushMult(Expression left, Expression right)
            {
                var binaryExpr = Expression.Multiply(left, right);
                Push(binaryExpr);
            }

            private void PushSub(Expression left, Expression right)
            {
                var binaryExpr = Expression.Subtract(left, right);
                Push(binaryExpr);
            }

            private void PushAdd(Expression left, Expression right)
            {
                var binaryExpr = Expression.Add(left, right);
                Push(binaryExpr);
            }

            private void PushTagSelector()
            {
                var tagKey = (_expressions.Pop() as ConstantExpression).Value.ToString();
                Expression<Func<Model, string>> tagSelector = m => m.Tags.First(t => t.Key == tagKey).Value;
                _expressions.Push(Expression.Invoke(tagSelector, _param));
            }

            private void PushToNum()
            {
                Expression<Func<string, float>> toFloat = s => float.Parse(SanitizeHelper.SanitizeFloat(s));
                _expressions.Push(toFloat);
            }

            private void PushToColor()
            {
                Expression<Func<string, Color32>> toColor = s => ColorUtils.FromUnknown(s);
                _expressions.Push(toColor);
            }

            private void PushToGradient()
            {
                Expression<Func<string, string>> toGradient = s => ColorUtils.ColorToGradient(s);
                _expressions.Push(toGradient);
            }

            #endregion
        }

        #endregion
    }
}
