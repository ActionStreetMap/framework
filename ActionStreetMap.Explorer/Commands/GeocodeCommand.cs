using System;
using System.Text;
using ActionStreetMap.Infrastructure.Reactive;
using ActionStreetMap.Infrastructure.Utilities;
using ActionStreetMap.Maps.GeoCoding;

namespace ActionStreetMap.Explorer.Commands
{
    /// <summary> Represents reverse geocoding command. </summary>
    public class GeocodeCommand: ICommand
    {
        private readonly IGeocoder _geoCoder;

        /// <inheritdoc />
        public string Name { get { return "geocode"; } }

        /// <inheritdoc />
        public string Description { get { return "Preforms reverse geocoding."; } }

        /// <summary>
        ///     Creates instance of <see cref="GeocodeCommand"/>.
        /// </summary>
        /// <param name="geoCoder">Geocoder.</param>
        public GeocodeCommand(IGeocoder geoCoder)
        {
            _geoCoder = geoCoder;
        }

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
            var query = commandLine["q"];

            var observable = _geoCoder.Search(query);
            observable.Subscribe(r => response.AppendFormat("{0}:{1}\n", r.Coordinate, r.DisplayName));
            observable.Wait();
            return response.ToString();
        }

        private bool ShouldPrintHelp(Arguments commandLine)
        {
            return commandLine["q"] == null;
        }

        private void PrintHelp(StringBuilder response)
        {
            response.AppendLine("Usage: geocode /q=<place name>");
        }
    }
}
