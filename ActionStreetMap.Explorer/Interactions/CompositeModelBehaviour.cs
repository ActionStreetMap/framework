using System;
using System.Collections.Generic;
using ActionStreetMap.Core.Tiling;
using ActionStreetMap.Core.Tiling.Models;
using ActionStreetMap.Core.Unity;
using UnityEngine;

namespace ActionStreetMap.Explorer.Interactions
{
    /// <summary>
    ///     Defines model behavior which consists of list of mono behaviors.
    /// </summary>
    public class CompositeModelBehaviour : IModelBehaviour
    {
        private readonly IEnumerable<Type> _behaviourTypes;

        /// <inheritdoc />
        public string Name { get; private set; }

        /// <summary>
        ///     Creates CompositeModelBehaviour
        /// </summary>
        /// <param name="name">Name.</param>
        /// <param name="behaviourTypes">List of mono behaviors.</param>
        public CompositeModelBehaviour(string name, IEnumerable<Type> behaviourTypes)
        {
            Name = name;
            _behaviourTypes = behaviourTypes;
        }

        /// <inheritdoc />
        public void Apply(IGameObject gameObject, Model model)
        {
            foreach (var behaviour in _behaviourTypes)
            {
                // NOTE: behavior should implement Unity's MonoBehavior
                var component = gameObject.GetComponent<GameObject>()
                    .AddComponent(behaviour);

                // NOTE inject gameObject and model in case of our IModelBehavior
                // which allows us to use ASM's Model inside Unity's MonoBehavior
                var modelBehaviour = component as IModelBehaviour;
                if (modelBehaviour != null)
                {
                    modelBehaviour.Apply(gameObject, model);
                }
            }
        }
    }
}