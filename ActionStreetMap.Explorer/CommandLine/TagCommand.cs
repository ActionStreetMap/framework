using System;
using System.Text;
using ActionStreetMap.Infrastructure.Dependencies;
using ActionStreetMap.Osm.Entities;
using ActionStreetMap.Osm.Index.Search;

namespace ActionStreetMap.Explorer.CommandLine
{
    /// <summary>
    ///     Tags search command.     
    /// </summary>
    public class TagCommand: ICommand
    {
        private readonly ISearchEngine _searchEngine;

        /// <summary>
        ///     Creates instance of <see cref="TagCommand"/>
        /// </summary>
        /// <param name="searchEngine">Search engine instance.</param>
        [Dependency]
        public TagCommand(ISearchEngine searchEngine)
        {
            _searchEngine = searchEngine;
        }

        /// <inheritdoc />
        public string Name { get { return "tag"; }}

        /// <inheritdoc />
        public string Description { get { return Strings.TagCommand; } }

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

            var query = commandLine["q"].Split("=".ToCharArray());
            var key = query[0];
            var value = query[1];
            var type = commandLine["f"];
            foreach (var element in _searchEngine.SearchByTag(key, value))
            {
                if (IsElementMatch(type, element))
                    response.AppendLine(element.ToString());
            }

            return response.ToString();
        }

        private bool IsElementMatch(string type, Element element)
        {
            return type == null ||
                   (element is Node && type == "n" || type == "node") ||
                   (element is Way && type == "w" || type == "way") ||
                   (element is Relation && type == "r" || type == "relation");
        }

        private bool ShouldPrintHelp(Arguments commandLine)
        {
            return commandLine["h"] != null || commandLine["H"] != null ||
                   commandLine["q"] == null || commandLine["q"].Split("=".ToCharArray()).Length < 2;
        }

        private void PrintHelp(StringBuilder response)
        {
            response.AppendLine("Usage: tag [/h|/H]");
            response.AppendLine("       tag /q:key=value [/f:element_type]");
        }
    }
}
