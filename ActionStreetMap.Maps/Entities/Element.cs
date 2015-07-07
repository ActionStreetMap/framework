using ActionStreetMap.Core;
using ActionStreetMap.Maps.Visitors;
using ActionStreetMap.Core.Tiling.Models;

namespace ActionStreetMap.Maps.Entities
{
    /// <summary>
    ///     Primive used as a base class for any osm object that has a meaning on the map 
    ///     (NodeIds, Ways and Relations).
    /// </summary>
    public abstract class Element
    {
        /// <summary> Gets or sets id. </summary>
        public long Id { get; set; }

        /// <summary> Gets or sets tags. </summary>
        public TagCollection Tags { get; set; }

        /// <summary> Safe adds tag to collection. </summary>
        /// <param name="key">Tag key.</param>
        /// <param name="value">Tag value.</param>
        public virtual void AddTag(string key, string value)
        {
            if (Tags == null) Tags = new TagCollection();
            Tags.Add(key, value);
        }

        /// <summary> Accepts visitor. </summary>
        /// <param name="elementVisitor">Element visitor.</param>
        public abstract void Accept(IElementVisitor elementVisitor);

        /// <summary> Checks whether element is inside bounding box. </summary>
        /// <param name="bbox">Bounding box.</param>
        public abstract bool IsInside(BoundingBox bbox);

        /// <inheritdoc />
        public override string ToString()
        {
            string tags = "{no tags}";
            if (Tags != null && Tags.Count > 0)
            {
                tags = "tags:{";
                foreach (var tag in Tags)
                    tags += string.Format("{0}:{1},", tag.Key, tag.Value);
                tags += "}";
            }
            return string.Format("{0}[{1}]{2}", GetType().Name, Id, tags);
        }
    }
}