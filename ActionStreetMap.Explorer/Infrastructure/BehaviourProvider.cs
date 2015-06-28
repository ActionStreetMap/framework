using System;
using System.Collections.Generic;

namespace ActionStreetMap.Explorer.Infrastructure
{
    /// <summary> Maintains list of global behaviours. </summary>
    public class BehaviourProvider
    {
        private readonly Dictionary<string, Type> _behaviours = new Dictionary<string, Type>(4);

        /// <summary> Registers behaviour. </summary>
        public BehaviourProvider Register(string name, Type modelBehaviourType)
        {
            _behaviours.Add(name, modelBehaviourType);
            return this;
        }

        /// <summary> Gets behaviour type by name. </summary>
        public Type GetBehaviour(string name)
        {
            return _behaviours[name];
        }
    }
}
