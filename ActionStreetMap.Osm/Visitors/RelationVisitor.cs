using System.Collections.Generic;
using ActionStreetMap.Core.Tiling;
using ActionStreetMap.Core.Tiling.Models;
using ActionStreetMap.Infrastructure.Utilities;
using ActionStreetMap.Osm.Helpers;

namespace ActionStreetMap.Osm.Visitors
{
    /// <summary>
    ///     Relation visitor.
    /// </summary>
    public class RelationVisitor : ElementVisitor
    {
        /// <inheritdoc />
        public RelationVisitor(IModelVisitor modelVisitor, IObjectPool objectPool)
            : base(modelVisitor, objectPool)
        {
        }

        /// <inheritdoc />
        public override void VisitRelation(Entities.Relation relation)
        {
            string actualValue;
            var modelRelation = new Relation()
            {
                Id = relation.Id,
                Tags = relation.Tags,
            };

            if (relation.Tags != null && relation.Tags.TryGetValue("type", out actualValue) &&
                actualValue == "multipolygon")
            {
                // TODO use object pool
                modelRelation.Areas = new List<Area>(relation.Members.Count);
                MultipolygonProcessor.FillAreas(relation, modelRelation.Areas);
            }    
            ModelVisitor.VisitRelation(modelRelation);
        }
    }
}


