using System;
using System.Text;

using ActionStreetMap.Core;
using ActionStreetMap.Infrastructure.Dependencies;

namespace ActionStreetMap.Explorer.Commands
{
    /// <summary>
    ///     Location command.     
    /// </summary>
    public class LocateCommand : ICommand
    {
        private readonly IPositionObserver<GeoCoordinate> _geoPositionObserver;
        private readonly IPositionObserver<MapPoint> _mapPositionObserver;
        
        /// <summary>
        ///     Creates instance of <see cref="LocateCommand"/>
        /// </summary>
        /// <param name="geoPositionObserver">Position listener.</param>
        [Dependency]
        public LocateCommand(IPositionObserver<GeoCoordinate> geoPositionObserver,
            IPositionObserver<MapPoint> mapPositionObserver)
        {
            _geoPositionObserver = geoPositionObserver;
            _mapPositionObserver = mapPositionObserver;
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

            response.AppendLine(String.Format("map: {0}", _geoPositionObserver.Current));
            response.AppendLine(String.Format("geo: {0}", _mapPositionObserver.Current));

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
