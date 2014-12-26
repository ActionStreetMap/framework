using System.Collections.Generic;
using System.Text;
using ActionStreetMap.Infrastructure.Dependencies;

namespace ActionStreetMap.Explorer.CommandLine
{
    /// <summary>
    ///     Provides the way to work with command line commands.
    /// </summary>
    public class CommandController
    {
        private Dictionary<string, ICommand> _cmdTable = new Dictionary<string, ICommand>();

        /// <summary>
        ///     Returns list of registered commands.
        /// </summary>
        public IEnumerable<string> CommandNames { get { return _cmdTable.Keys; } }

        /// <summary>
        ///     Creates instance of <see cref="CommandController"/>.
        /// </summary>
        /// <param name="commands">Collection of commands.</param>
        [Dependency]
        public CommandController(IEnumerable<ICommand> commands)
        {
            foreach (var command in commands)
                Register(command);
            // provide default implementation of help command
            if(!Contains("help"))
                Register(new Command("help", "prints help", CmdHelp));
        }

        /// <summary>
        ///     Registers command.
        /// </summary>
        /// <param name="command">Command.</param>
        public void Register(ICommand command)
        {
            _cmdTable[command.Name] = command;
        }

        /// <summary>
        ///     Unregisters command.
        /// </summary>
        /// <param name="command">Command.</param>
        public void Unregister(ICommand command)
        {
            _cmdTable.Remove(command.Name);
        }

        /// <summary>
        ///     Checks whether command is registered.
        /// </summary>
        /// <param name="command">Command.</param>
        /// <returns>True if command registered.</returns>
        public bool Contains(string command)
        {
            return _cmdTable.ContainsKey(command);
        }

        /// <summary>
        ///     Gets command by name.
        /// </summary>
        /// <param name="name">Name of command.</param>
        /// <returns>Command</returns>
        public ICommand this[string name]
        {
            get
            {
                return _cmdTable[name];
            }
        }

        private string CmdHelp(params string[] args)
        {
            var output = new StringBuilder();
            output.AppendLine(":: Command List ::");

            foreach (string name in CommandNames)
            {
                var command = _cmdTable[name];
                output.AppendFormat("{0}: {1}\n", name, command.Description);
            }

            output.AppendLine(" ");
            return output.ToString();
        }
    }
}
