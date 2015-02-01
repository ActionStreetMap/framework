using System;

namespace ActionStreetMap.Maps.Entities
{
    /// <summary> Represents simple relation member. </summary>
    public class RelationMember
    {
        /// <summary> Gets or sets relation member. </summary>
        public Element Member { get; set; }

        /// <summary> Gets or sets relation member id. </summary>
        public long MemberId { get; set; }

        /// <summary> Gets or sets relation member type id. </summary>
        public int TypeId { get; set; }

        /// <summary> Gets or sets relation member role. </summary>
        public string Role { get; set; }

        /// <summary>  Offset in external storage. </summary>
        internal uint Offset { get; set; }

        /// <inheritdoc />
        public override string ToString()
        {
            return String.Format("{0}[{1}]:{2}", Role, MemberId, Member);
        }
    }
}