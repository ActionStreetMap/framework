namespace ActionStreetMap.Infrastructure.Bootstrap
{
    /// <summary> Provides the functionality to manage startup plugins. </summary>
    public interface IBootstrapperService
    {
        /// <summary> Run all registred bootstrappers. </summary>
        bool Run();

        /// <summary> Updates all registred bootstrappers. </summary>
        bool Update();

        /// <summary> Stops all registred bootstrappers. </summary>
        bool Stop();
    }
}