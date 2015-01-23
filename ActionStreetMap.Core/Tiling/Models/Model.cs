using System.Collections.Generic;

namespace ActionStreetMap.Core.Tiling.Models
{
    /// <summary>
    ///     Represents abstract model which can be represented via OSM data.
    /// </summary>
    public abstract class Model
    {
        /// <summary>
        ///    Gets or sets model id.
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        ///     Gets or sets osm tags.
        /// </summary>
        public Dictionary<string, string> Tags { get; set; }

        /// <summary>
        ///     Gets or sets application specific properties to model.
        /// </summary>
        public Dictionary<string, object> Properties { get; set; }        

        /// <summary>
        ///     True if model's points defines closed polygon.
        /// </summary>
        public abstract bool IsClosed { get; }

        /// <summary>
        ///     Accepts IModelVisitor (Visitor pattern).
        /// </summary>
        /// <param name="visitor">IModelVisitor instance.</param>
        public abstract void Accept(IModelVisitor visitor);

        /// <summary>
        ///     Returns string representation of this object.
        /// </summary>
        public override string ToString()
        {
            string tags = "{no tags}";
            if (Tags != null && Tags.Count > 0)
            {
                tags = "tags:{";
                foreach (var tag in Tags)
                {
                    tags += string.Format("{0}:{1},", tag.Key, tag.Value);
                }
                tags += "}";
            }
            return string.Format("{0}[{1}]:{2} {3}", GetType().Name, Id, tags, IsClosed ? "closed" : "open");
        }
    }
}
