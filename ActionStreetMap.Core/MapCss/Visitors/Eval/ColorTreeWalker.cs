using System;
using ActionStreetMap.Core.Tiling.Models;
using ActionStreetMap.Core.Unity;
using Antlr.Runtime.Tree;

namespace ActionStreetMap.Core.MapCss.Visitors.Eval
{
    /// <summary> Builds color from RGB representation. </summary>
    internal class ColorTreeWalker : ITreeWalker
    {
        private readonly byte _r;
        private readonly byte _g;
        private readonly byte _b;

        /// <summary> Creats instance of <see cref="ColorTreeWalker"/>. </summary>
        /// <param name="tree">Parse tree.</param>
        public ColorTreeWalker(CommonTree tree)
        {
            _r = byte.Parse(String.Intern(tree.Children[0].Text));
            _g = byte.Parse(String.Intern(tree.Children[1].Text));
            _b = byte.Parse(String.Intern(tree.Children[2].Text));
        }

        /// <inheritdoc />
        public T Walk<T>(Model model)
        {
            // TODO this looks ugly
            return (T) (object) new Color32(_r, _g, _b, 255);
        }
    }
}