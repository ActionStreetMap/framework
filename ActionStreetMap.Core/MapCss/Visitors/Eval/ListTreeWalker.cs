using System.Collections.Generic;
using ActionStreetMap.Core.Tiling.Models;
using Antlr.Runtime.Tree;

namespace ActionStreetMap.Core.MapCss.Visitors.Eval
{
    /// <summary> Provides logic for getting comma-separated list of values. </summary>
    internal class ListTreeWalker : ITreeWalker
    {
        private readonly List<string> _values = new List<string>();

        /// <summary> Creates ListTreeWalker. </summary>
        /// <param name="tree">Parse tree.</param>
        public ListTreeWalker(CommonTree tree)
        {
            foreach (var child in tree.Children)
            {
                _values.Add(child.Text);
            }
        }

        /// <inheritdoc />
        public T Walk<T>(Model model)
        {
            return (T) (object) _values;
        }
    }
}