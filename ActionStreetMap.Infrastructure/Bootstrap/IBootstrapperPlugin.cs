namespace ActionStreetMap.Infrastructure.Bootstrap
{
    /// <summary> Represents a startup plugin. </summary>
    public interface IBootstrapperPlugin
    {
        /// <summary> The name of plugin. </summary>
        string Name { get; }

        /// <summary> Runs registrations. </summary>
        /// <returns>True if no errors.</returns>
        bool Run();

        /// <summary> Called when we need to update registered services. </summary>
        /// <returns>True if no errors.</returns>
        bool Update();

        /// <summary> Called when we need to stop registered services. </summary>
        /// <returns>True if no errors.</returns>
        bool Stop();
    }
}