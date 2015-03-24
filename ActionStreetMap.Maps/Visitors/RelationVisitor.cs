using System.Collections.Generic;
using ActionStreetMap.Core.Tiling;
using ActionStreetMap.Core.Tiling.Models;
using ActionStreetMap.Core.Utils;
using ActionStreetMap.Infrastructure.Utilities;
using ActionStreetMap.Maps.Helpers;

namespace ActionStreetMap.Maps.Visitors
{
    /// <summary> Relation visitor.</summary>
    internal class RelationVisitor : ElementVisitor
    {
        /// <inheritdoc />
        public RelationVisitor(Tile tile, IModelLoader modelLoader, IObjectPool objectPool)
            : base(tile, modelLoader, objectPool)
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
            ModelLoader.LoadRelation(Tile, modelRelation);
        }
    }
}


