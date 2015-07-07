using System;

namespace ActionStreetMap.Maps.Entities
{
    /// <summary> Represents simple relation member. </summary>
    public class RelationMember
    {
        /// <summary> Relation member. </summary>
        public Element Member;

        /// <summary> Relation member id. </summary>
        public long MemberId;

        /// <summary> Relation member type id. </summary>
        public int TypeId;

        /// <summary> Relation member role. </summary>
        public string Role;

        /// <summary> Offset in external storage. </summary>
        internal uint Offset;

        /// <inheritdoc />
        public override string ToString()
        {
            return String.Format("{0}[{1}]:{2}", Role, MemberId, Member);
        }
    }
}