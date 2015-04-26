using ActionStreetMap.Core.Tiling;
using ActionStreetMap.Core.Tiling.Models;
using ActionStreetMap.Core.Unity;

namespace ActionStreetMap.Explorer.Interactions
{
    /// <summary> Workaround for empty list of behaviours. </summary>
    internal class DummyBehaviour: IModelBehaviour
    {
        /// <inheritdoc />
        public string Name { get { return "dummy"; } }

        /// <inheritdoc />
        public void Apply(IGameObject gameObject, Model model)
        {
        }
    }
}
