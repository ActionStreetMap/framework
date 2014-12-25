using System;
using ActionStreetMap.Infrastructure.Dependencies;
using ActionStreetMap.Osm.Index.Search;

namespace ActionStreetMap.Explorer.CommandLine
{
    /// <summary>
    ///     Tags search command.     
    /// </summary>
    public class TagCommand: ICommand
    {
        private readonly SearchEngine _searchEngine;

        /// <summary>
        ///     Creates instance of <see cref="TagCommand"/>
        /// </summary>
        /// <param name="searchEngine">Search engine instance.</param>
        [Dependency]
        public TagCommand(SearchEngine searchEngine)
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
            throw new NotImplementedException();
        }
    }
}
