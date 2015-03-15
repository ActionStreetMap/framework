
namespace ActionStreetMap.Core.Unity
{
    /// <summary> Creates GameObjects. </summary>
    public interface IGameObjectFactory
    {
        // TODO add object pool logic?

        /// <summary> Creates new game object with given name. </summary>
        /// <param name="name">Name.</param>
        /// <returns>Game object wrapper.</returns>
        IGameObject CreateNew(string name);

        /// <summary> Creates new game object with given name and parent. </summary>
        /// <param name="name">Name.</param>
        /// <param name="parent">Parent.</param>
        /// <returns>Game object wrapper.</returns>
        IGameObject CreateNew(string name, IGameObject parent);
    }
}