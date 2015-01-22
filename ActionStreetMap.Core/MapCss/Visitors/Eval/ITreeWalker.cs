using ActionStreetMap.Core.Tiling.Models;

namespace ActionStreetMap.Core.MapCss.Visitors.Eval
{
    /// <summary>
    ///     Defines logic for processing of parse tree for given model and returning result.
    /// </summary>
    public interface ITreeWalker
    {
        /// <summary>
        ///     "Walks" tree.
        /// </summary>
        /// <typeparam name="T">Result type.</typeparam>
        /// <param name="model">model.</param>
        /// <returns>Result.</returns>
        T Walk<T>(Model model);
    }
}
