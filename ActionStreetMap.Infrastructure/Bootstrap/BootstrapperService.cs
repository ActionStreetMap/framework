using System.Collections.Generic;
using System.Linq;
using ActionStreetMap.Infrastructure.Dependencies;

namespace ActionStreetMap.Infrastructure.Bootstrap
{
    /// <summary> Provides default functionality to execute startup plugins. </summary>
    public class BootstrapperService : IBootstrapperService
    {
        private readonly IEnumerable<IBootstrapperPlugin> _plugins;

        /// <summary> Gets or sets DI container. </summary>
        [Dependency]
        public IContainer Container { get; set; }

        /// <summary> Creates BootstrapperService. </summary>
        [Dependency]
        public BootstrapperService()
        {
        }

        /// <summary> Creates BootstrapperService from plugin list. </summary>
        /// <param name="container">Container.</param>
        /// <param name="plugins">Plugin list.</param>
        public BootstrapperService(Container container, IEnumerable<IBootstrapperPlugin> plugins)
        {
            Container = container;
            _plugins = plugins;
        }

        #region IBootstrapperService members

        /// <inheritdoc />
        public bool Run()
        {
            var plugins = _plugins ?? Container.ResolveAll<IBootstrapperPlugin>();
            return plugins
                .ToList().Aggregate(true, (current, task) => current & task.Run());
        }

        /// <inheritdoc />
        public bool Update()
        {
            var plugins = _plugins ?? Container.ResolveAll<IBootstrapperPlugin>();
            return plugins
                .ToList().Aggregate(true, (current, task) => current & task.Update());
        }

        /// <inheritdoc />
        public bool Stop()
        {
            var plugins = _plugins ?? Container.ResolveAll<IBootstrapperPlugin>();
            return plugins
                .ToList().Aggregate(true, (current, task) => current & task.Stop());
        }

        #endregion
    }
}