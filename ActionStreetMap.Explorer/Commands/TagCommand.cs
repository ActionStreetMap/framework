using System.Linq;
using System.Text;
using ActionStreetMap.Core;
using ActionStreetMap.Core.Tiling;
using ActionStreetMap.Core.Utils;
using ActionStreetMap.Infrastructure.Dependencies;
using ActionStreetMap.Infrastructure.Reactive;
using ActionStreetMap.Infrastructure.Utilities;
using ActionStreetMap.Maps.Data.Search;
using ActionStreetMap.Maps.Entities;

namespace ActionStreetMap.Explorer.Commands
{
    /// <summary> Tags search command. </summary>
    public class TagCommand : ICommand
    {
        private readonly IPositionObserver<GeoCoordinate> _geoPositionObserver;
        private readonly ISearchEngine _searchEngine;

        /// <inheritdoc />
        public string Name { get { return "tag"; } }

        /// <inheritdoc />
        public string Description { get { return Strings.TagCommand; } }

        /// <summary> Creates instance of <see cref="TagCommand" />. </summary>
        /// <param name="positionObserver">Position listener.</param>
        /// <param name="searchEngine">Search engine instance.</param>
        [Dependency]
        public TagCommand(ITilePositionObserver positionObserver, ISearchEngine searchEngine)
        {
            _geoPositionObserver = positionObserver;
            _searchEngine = searchEngine;
        }

        /// <inheritdoc />
        public IObservable<string> Execute(params string[] args)
        {
            return Observable.Create<string>(o =>
            {
                var response = new StringBuilder();
                var commandLine = new Arguments(args);
                if (ShouldPrintHelp(commandLine))
                    PrintHelp(response);
                else
                {
                    var query = commandLine["q"].Split("=".ToCharArray());
                    var key = query[0];
                    var value = query[1];
                    var type = commandLine["f"];
                    var radius = commandLine["r"] == null ? 0 : float.Parse(commandLine["r"]);
                    foreach (var element in _searchEngine.SearchByTag(key, value))
                    {
                        if (!IsElementMatch(type, element))
                            continue;

                        if (radius <= 0 || IsInCircle(radius, element))
                            response.AppendLine(element.ToString());
                    }
                }

                o.OnNext(response.ToString());
                o.OnCompleted();
                return Disposable.Empty;
            });
        }

        private bool IsElementMatch(string type, Element element)
        {
            return type == null ||
                   (element is Node && type == "n" || type == "node") ||
                   (element is Way && type == "w" || type == "way") ||
                   (element is Relation && type == "r" || type == "relation");
        }

        #region Radius check

        private bool IsInCircle(float radius, Element element)
        {
            return CheckElement(radius, element);
        }

        private bool CheckElement(float radius, Element element)
        {
            if (element is Node)
                return Check(radius, element as Node);
            if (element is Way)
                return Check(radius, element as Way);
            return Check(radius, element as Relation);
        }

        private bool Check(float radius, Node node)
        {
            return GeoProjection.Distance(node.Coordinate, _geoPositionObserver.Current) <= radius;
        }

        private bool Check(float radius, Way way)
        {
            return way.Coordinates.Any(geoCoordinate =>
                GeoProjection.Distance(geoCoordinate, _geoPositionObserver.Current) <= radius);
        }

        private bool Check(float radius, Relation relation)
        {
            return relation.Members.Any(member => CheckElement(radius, member.Member));
        }

        #endregion

        private bool ShouldPrintHelp(Arguments commandLine)
        {
            return commandLine["h"] != null || commandLine["H"] != null ||
                   commandLine["q"] == null || commandLine["q"].Split("=".ToCharArray()).Length < 2;
        }

        private void PrintHelp(StringBuilder response)
        {
            response.AppendLine("Usage: tag [/h|/H]");
            response.AppendLine("       tag /q:key=value [/f:element_type] [/r:radius_in_meters");
        }
    }
}