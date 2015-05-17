using System;
using System.Linq;
using System.Text;
using ActionStreetMap.Core;
using ActionStreetMap.Core.Tiling;
using ActionStreetMap.Core.Utils;
using ActionStreetMap.Infrastructure.Dependencies;
using ActionStreetMap.Infrastructure.Reactive;
using ActionStreetMap.Infrastructure.Utilities;
using ActionStreetMap.Maps.Data;
using ActionStreetMap.Maps.Data.Search;
using ActionStreetMap.Maps.Entities;

namespace ActionStreetMap.Explorer.Commands
{
    /// <summary> Search  command. </summary>
    internal class SearchCommand : ICommand
    {
        private readonly ITilePositionObserver _tilePositionObserver;
        private readonly IPositionObserver<GeoCoordinate> _geoPositionObserver;
        private readonly ISearchEngine _searchEngine;

        /// <inheritdoc />
        public string Name { get { return "search"; } }

        /// <inheritdoc />
        public string Description { get { return Strings.SearchCommand; } }

        /// <summary> Returns geocoordinate which is used for search. </summary>
        internal GeoCoordinate SearchCenter { get { return _geoPositionObserver.Current; } }

        /// <summary> Creates instance of <see cref="SearchCommand" />. </summary>
        /// <param name="positionObserver">Position listener.</param>
        /// <param name="searchEngine">Search engine instance.</param>
        [Dependency]
        public SearchCommand(ITilePositionObserver positionObserver, ISearchEngine searchEngine)
        {
            _tilePositionObserver = positionObserver;
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
                    var query = commandLine["q"];
                    var type = commandLine["f"];
                    var radius = commandLine["r"] == null ? 0 : float.Parse(commandLine["r"]);
                    IObservable<Element> results;
                    if (query.Contains("="))
                    {
                        var tagQuery = query.Split("=".ToCharArray());
                        var key = tagQuery[0];
                        var value = tagQuery[1];
                        results = GetElementsByTag(key, value, radius, type);
                    }
                    else
                        results = GetElementsByText(query, radius, type);

                    foreach (var element in results.ToArray().Wait())
                        response.AppendLine(element.ToString());
                }

                o.OnNext(response.ToString());
                o.OnCompleted();
                return Disposable.Empty;
            });
        }

        #region Actual search methods

        /// <summary> Gets elements for specific tag. </summary>
        internal IObservable<Element> GetElementsByTag(string key, string value, float radius, string type = null)
        {
            var bbox = _tilePositionObserver.CurrentTile.BoundingBox;
            return _searchEngine
                .SearchByTag(key, value, bbox)
                .Where(e => IsElementMatch(type, e) && (radius <= 0 || IsInCircle(radius, e)));
        }

        /// <summary> Gets elements for specific tag. </summary>
        internal IObservable<Element> GetElementsByText(string text, float radius, 
            string type = null, int zoomLevel = MapConsts.MaxZoomLevel)
        {
            var bbox = _tilePositionObserver.CurrentTile.BoundingBox;
            return _searchEngine
                .SearchByText(text, bbox, zoomLevel)
                .Where(e => IsElementMatch(type, e) && (radius <= 0 || IsInCircle(radius, e)));
        }

        #endregion

        private bool IsElementMatch(string type, Element element)
        {
            return String.IsNullOrEmpty(type) ||
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
            response.AppendLine("Usage: search [/h|/H]");
            response.AppendLine("       search /q:tag_key=tag_value [/f:element_type] [/r:radius_in_meters");
            response.AppendLine("       search /q:any_text [/f:element_type] /r:radius_in_meters");
                                         
        }
    }
}