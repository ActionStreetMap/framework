using System.Collections.Generic;
using ActionStreetMap.Maps.Visitors;

namespace ActionStreetMap.Maps.Entities
{
    /// <summary> Represents a simple relation. </summary>
    public class Relation : Element
    {
        /// <summary> Gets or sets relation members. </summary>
        public List<RelationMember> Members { get; set; }

        /// <inheritdoc />
        public override void Accept(IElementVisitor elementVisitor)
        {
            elementVisitor.VisitRelation(this);
        }
    }
}