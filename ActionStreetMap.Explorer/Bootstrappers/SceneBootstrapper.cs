using ActionStreetMap.Core.MapCss;
using ActionStreetMap.Core.Tiling;
using ActionStreetMap.Explorer.Customization;
using ActionStreetMap.Explorer.Scene.Builders;
using ActionStreetMap.Explorer.Tiling;
using ActionStreetMap.Infrastructure.Dependencies;
using ActionStreetMap.Explorer.Scene.Facades;
using ActionStreetMap.Explorer.Scene.Roofs;

namespace ActionStreetMap.Explorer.Bootstrappers
{
    /// <summary> Register scene specific classes. </summary>
    public class SceneBootstrapper: BootstrapperPlugin
    {
        /// <inheritdoc />
        public override string Name { get { return "scene"; } }

        /// <inheritdoc />
        public override bool Run()
        {
            Container.Register(Component.For<IModelLoader>().Use<TileModelLoader>().Singleton());

            // register stylesheet provider
            Container.Register(Component
                .For<IStylesheetProvider>()
                .Use<StylesheetProvider>()
                .SetConfig(GlobalConfigSection)
                .Singleton());

            // register model processing extensions.
            Container.RegisterInstance(new CustomizationService(Container)
                .RegisterBuilder("building", typeof (BuildingModelBuilder))
                .RegisterBuilder("sphere", typeof (SphereModelBuilder))
                .RegisterBuilder("cylinder", typeof (CylinderModelBuilder))
                .RegisterBuilder("water", typeof (WaterModelBuilder))
                .RegisterBuilder("barrier", typeof (BarrierModelBuilder))
                .RegisterBuilder("info", typeof (InfoModelBuilder))
                .RegisterBuilder("tree", typeof (TreeModelBuilder))
                .RegisterBuilder("road", typeof (RoadModelBuilder))
                .RegisterBuilder("surface", typeof (SurfaceModelBuilder)));
           
            // facades
            Container.Register(Component.For<IFacadeBuilder>().Use<FacadeBuilder>().Named("default").Singleton());
            
            // roofs
            Container.Register(Component.For<IRoofBuilder>().Use<GabledRoofBuilder>().Named("gabled").Singleton());
            Container.Register(Component.For<IRoofBuilder>().Use<HippedRoofBuilder>().Named("hipped").Singleton());
            Container.Register(Component.For<IRoofBuilder>().Use<DomeRoofBuilder>().Named("dome").Singleton());
            Container.Register(Component.For<IRoofBuilder>().Use<PyramidalRoofBuilder>().Named("pyramidal").Singleton());
            Container.Register(Component.For<IRoofBuilder>().Use<MansardRoofBuilder>().Named("mansard").Singleton());
            Container.Register(Component.For<IRoofBuilder>().Use<FlatRoofBuilder>().Named("flat").Singleton());
         
            return true;
        }
    }
}
