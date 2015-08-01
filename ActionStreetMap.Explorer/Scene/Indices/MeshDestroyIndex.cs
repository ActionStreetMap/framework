namespace ActionStreetMap.Explorer.Scene.Indices
{
    /// <summary> Defines index which marks mesh as destroyed if modify query is performed. </summary>
    internal sealed class MeshDestroyIndex: IMeshIndex
    {
        public static readonly MeshDestroyIndex Default = new MeshDestroyIndex();

        private MeshDestroyIndex() { }

        /// <inheritdoc />
        public void Build()
        {
        }

        /// <inheritdoc />
        public MeshQuery.Result Modify(MeshQuery query)
        {
            return new MeshQuery.Result(query.Vertices)
            {
                IsDestroyed = true
            };
        }
    }
}
