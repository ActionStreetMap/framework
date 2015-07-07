using System.Collections.Generic;
using System.Linq;
using ActionStreetMap.Core;
using ActionStreetMap.Maps.Visitors;

namespace ActionStreetMap.Maps.Entities
{
    /// <summary> Represents a simple relation. </summary>
    public class Relation : Element
    {
        /// <summary> Relation members. </summary>
        public List<RelationMember> Members;

        /// <inheritdoc />
        public override void Accept(IElementVisitor elementVisitor)
        {
            elementVisitor.VisitRelation(this);
        }

        /// <inheritdoc />
        public override bool IsInside(BoundingBox bbox)
        {
            return Members.Any(relationMember => relationMember.Member.IsInside(bbox));
        }
    }
}