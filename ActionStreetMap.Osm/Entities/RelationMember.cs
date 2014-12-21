using System;

namespace ActionStreetMap.Osm.Entities
{
    /// <summary>
    ///     Represents simple relation member.
    /// </summary>
    public class RelationMember
    {
        /// <summary>
        ///     Member.
        /// </summary>
        public Element Member { get; set; }

        /// <summary>
        ///     Member id.
        /// </summary>
        public long MemberId { get; set; }

        /// <summary>
        ///     Type id.
        /// </summary>
        public int TypeId { get; set; }

        /// <summary>
        ///     Member role.
        /// </summary>
        public string Role { get; set; }

        public uint Offset { get; set; }

        /// <inheritdoc />
        public override string ToString()
        {
            return String.Format("{0}[{1}]:{2}", Role, MemberId, Member);
        }
    }
}