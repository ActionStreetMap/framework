using System;
using System.Collections.Generic;

namespace ActionStreetMap.Core.Tiling.Models
{
    /// <summary>
    ///     Represents relation between different models. This abstraction is introduced
    ///     as we need to linke some models together (part of buildings, outline of buildings).
    /// </summary>
    public class Relation: Model
    {
        /// <summary>
        ///     This map keeps information about roles between elements.
        /// </summary>
        public Dictionary<string, HashSet<long>> RoleMap { get; set; }

        /// <summary>
        ///     Areas which are defined in relation.
        /// </summary>
        public List<Area> Areas { get; set; }

        /// <inheritdoc />
        public override bool IsClosed
        {
            get { return false; }
        }

        /// <inheritdoc />
        public override void Accept(IModelVisitor visitor)
        {
            visitor.VisitRelation(this);
        }
    }
}
