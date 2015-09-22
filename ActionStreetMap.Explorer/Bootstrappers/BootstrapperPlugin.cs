using ActionStreetMap.Explorer.Customization;
using ActionStreetMap.Explorer.Infrastructure;
using ActionStreetMap.Infrastructure.Bootstrap;
using ActionStreetMap.Infrastructure.Config;
using ActionStreetMap.Infrastructure.Dependencies;
using ActionStreetMap.Infrastructure.Diagnostic;
using ActionStreetMap.Infrastructure.IO;

namespace ActionStreetMap.Explorer.Bootstrappers
{
    /// <summary> Represents a bootstrapper plugin. </summary>
    public abstract class BootstrapperPlugin: IBootstrapperPlugin
    {
        /// <summary> Gets or sets DI container. </summary>
        [Dependency]
        public IContainer Container { get; set; }

        /// <summary> Gets or sets global configuration section. </summary>
        [Dependency]
        public IConfigSection GlobalConfigSection { get; set; }

        /// <summary> Gets or sets file system service. </summary>
        [Dependency]
        public IFileSystemService FileSystemService { get; set; }

        /// <summary> Gets or sets trace. </summary>
        [Dependency]
        public ITrace Trace { get; set; }

        /// <summary> Gets extension provider. </summary>
        public CustomizationService CustomizationService
        {
            get { return Container.Resolve<CustomizationService>(); }
        }

        /// <inheritdoc />
        public abstract string Name { get; }

        #region IBootstrapperPlugin members

        /// <inheritdoc />
        public abstract bool Run();

        /// <inheritdoc />
        public virtual bool Update()
        {
            return false;
        }

        /// <inheritdoc />
        public virtual bool Stop()
        {
            return false;
        }

        #endregion
    }
}
