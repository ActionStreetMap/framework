namespace ActionStreetMap.Core.Scene.Models
{
    /// <summary>
    ///     Represents canvas (terrain).
    /// </summary>
    public class Canvas : Model
    {
        /// <inheritdoc />
        public override bool IsClosed { get { return false; } }

        /// <inheritdoc />
        public override void Accept(IModelVisitor visitor)
        {
            visitor.VisitCanvas(this);
        }
    }
}
