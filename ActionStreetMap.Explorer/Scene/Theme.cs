using System;
using System.Collections.Generic;
using ActionStreetMap.Core.Scene.Buildings;
using ActionStreetMap.Core.Scene.Infos;
using ActionStreetMap.Explorer.Scene.Buildings;
using ActionStreetMap.Explorer.Scene.Infos;

namespace ActionStreetMap.Explorer.Scene
{
    /// <summary> Represents game theme. Bridge to style providers for different models. </summary>
    public class Theme
    {
        private readonly IInfoStyleProvider _infoStyleProvider;
        private readonly Dictionary<Type, object> _providers;

        /// <summary> Name of theme. </summary>
        public string Name { get; set; }

        /// <summary> Creates instance of <see cref="Theme"/>. </summary>
        /// <param name="infoStyleProvider">Info style provider.</param>
        public Theme(IInfoStyleProvider infoStyleProvider)
        {
            _infoStyleProvider = infoStyleProvider;
            _providers = new Dictionary<Type, object>
            {
                {typeof (IInfoStyleProvider), _infoStyleProvider}
            };
        }

        /// <summary> Gets info style. </summary>
        /// <param name="info">Info.</param>
        /// <returns>InfoStyle.</returns>
        public InfoStyle GetInfoStyle(Info info)
        {
            return _infoStyleProvider.Get(info);
        }

        /// <summary> Gets style provider by type. </summary>
        /// <typeparam name="T">Type.</typeparam>
        /// <returns>Style provider.</returns>
        public T GetStyleProvider<T>() where T:class
        {
            return (T) _providers[typeof(T)];
        }
    }
}
