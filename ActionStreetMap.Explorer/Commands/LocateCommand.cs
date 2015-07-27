using System;
using System.Text;
using ActionStreetMap.Core;
using ActionStreetMap.Core.Geometry;
using ActionStreetMap.Core.Tiling;
using ActionStreetMap.Infrastructure.Dependencies;
using ActionStreetMap.Infrastructure.Reactive;
using ActionStreetMap.Infrastructure.Utilities;

namespace ActionStreetMap.Explorer.Commands
{
    /// <summary> Location command. </summary>
    internal class LocateCommand : ICommand
    {
        private readonly IPositionObserver<GeoCoordinate> _geoPositionObserver;
        private readonly IPositionObserver<Vector2d> _mapPositionObserver;

        /// <inheritdoc />
        public string Name { get { return "locate"; } }

        /// <inheritdoc />
        public string Description { get { return Strings.LocateCommand; } }

        /// <summary> Creates instance of <see cref="LocateCommand" />. </summary>
        /// <param name="controller">Position listener.</param>
        [Dependency]
        public LocateCommand(ITileController controller)
        {
            _geoPositionObserver = controller;
            _mapPositionObserver = controller;
        }

        /// <inheritdoc />
        public IObservable<string> Execute(params string[] args)
        {
            return Observable.Create<string>(o =>
            {
                var response = new StringBuilder();
                var commandLine = new Arguments(args);
                if (ShouldPrintHelp(commandLine))
                {
                    PrintHelp(response);
                }
                else
                {
                    response.AppendLine(String.Format("geo: {0}", _geoPositionObserver.CurrentPosition));
                    response.AppendLine(String.Format("map: {0}", _mapPositionObserver.CurrentPosition));
                }

                o.OnNext(response.ToString());
                o.OnCompleted();
                return Disposable.Empty;
            });
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