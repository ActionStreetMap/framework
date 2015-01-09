using System;
using System.Text;

using ActionStreetMap.Core;
using ActionStreetMap.Core.Utilities;
using ActionStreetMap.Infrastructure.Dependencies;

namespace ActionStreetMap.Explorer.Commands
{
    /// <summary>
    ///     Location command.     
    /// </summary>
    public class LocateCommand : ICommand
    {
        private readonly IPositionListener _positionListener;
        
        /// <summary>
        ///     Creates instance of <see cref="LocateCommand"/>
        /// </summary>
        /// <param name="positionListener">Position listener.</param>
        [Dependency]
        public LocateCommand(IPositionListener positionListener)
        {
            _positionListener = positionListener;
        }

        /// <inheritdoc />
        public string Name { get { return "locate"; }}

        /// <inheritdoc />
        public string Description { get { return Strings.LocateCommand; } }

        /// <inheritdoc />
        public string Execute(params string[] args)
        {
            var response = new StringBuilder();
            var commandLine = new Arguments(args);
            if (ShouldPrintHelp(commandLine))
            {
                PrintHelp(response);
                return response.ToString();
            }

            response.AppendLine(String.Format("map: {0}", _positionListener.CurrentPoint));
            response.AppendLine(String.Format("geo: {0}", _positionListener.CurrentPosition));

            return response.ToString();
        }

        private bool ShouldPrintHelp(Arguments commandLine)
        {
            return commandLine["me"] == null;
        }

        private void PrintHelp(StringBuilder response)
        {
            response.AppendLine("Usage: locate /me");
        }
    }
}
