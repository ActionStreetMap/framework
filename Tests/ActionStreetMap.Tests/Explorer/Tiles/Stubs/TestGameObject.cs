using System;
using ActionStreetMap.Core.Unity;

namespace ActionStreetMap.Tests.Explorer.Tiles.Stubs
{
    class TestGameObject: IGameObject
    {
        public T AddComponent<T>(T component)
        {
            return component;
        }

        public T GetComponent<T>()
        {
            throw new NotSupportedException();
        }

        public string Name { get; set; }
        public IGameObject Parent { set; private get; }
    }
}
