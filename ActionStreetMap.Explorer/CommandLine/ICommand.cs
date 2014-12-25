
namespace ActionStreetMap.Explorer.CommandLine
{
    /// <summary>
    ///     Command line command.
    /// </summary>
    public interface ICommand
    {
        /// <summary>
        ///     Gets unique name of command.
        /// </summary>
        string Name { get; }

        /// <summary>
        ///     Gets description of command.
        /// </summary>
        string Description { get; }

        /// <summary>
        ///     Executes command.
        /// </summary>
        /// <param name="args">Argument list.</param>
        /// <returns>Output string.</returns>
        string Execute(params string[] args);
    }
}
