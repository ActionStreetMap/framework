namespace ActionStreetMap.Explorer.Scene
{
    /// <summary> Represents index of mesh vertices for quick search. </summary>
    public interface IMeshIndex
    {
        /// <summary> Builds index. </summary>
        void Build();

        /// <summary> Modifies mesh using query provided. </summary>
        bool Modify(MeshQuery query);
    }
}
