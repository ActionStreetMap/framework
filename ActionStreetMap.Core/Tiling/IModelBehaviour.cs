using ActionStreetMap.Core.Tiling.Models;
using ActionStreetMap.Core.Unity;

namespace ActionStreetMap.Core.Tiling
{
    /// <summary>
    ///     Defines ModelBehavior whcih can implement any additional logic associated with given model 
    ///     and its game object.
    /// </summary>
    public interface IModelBehaviour
    {
        /// <summary> Gets name of model builder. Should be unique. </summary>
        string Name { get; }

        /// <summary>
        ///     This method is called once model behavior is created in order to attach the behavior 
        ///     to given game object and model.
        /// </summary>
        /// <param name="gameObject">GameObject wrapper.</param>
        /// <param name="model">model.</param>
        void Apply(IGameObject gameObject, Model model);
    }
}