using System.Collections.Generic;
using System.Linq;
using ActionStreetMap.Core.Scene.Roads;

namespace ActionStreetMap.Explorer.Scene.Roads
{
    /// <summary> Defines road style provider logic. </summary>
    public interface IRoadStyleProvider
    {
        /// <summary> Gets road style for given road. </summary>
        /// <param name="road">Road.</param>
        /// <returns>Road style.</returns>
        RoadStyle Get(Road road);

        /// <summary> Gets road style for given road junction. </summary>
        /// <param name="junction">Road junction.</param>
        /// <returns>Road style.</returns>
        RoadStyle Get(RoadJunction junction);
    }

    /// <summary> Default road style provider. </summary>
    internal class RoadStyleProvider : IRoadStyleProvider
    {
        private readonly Dictionary<string, List<RoadStyle>> _roadTypeStyleMapping;

        /// <summary> Creates RoadStyleProvider. </summary>
        /// <param name="roadTypeStyleMapping">Road type to style mapping.</param>
        public RoadStyleProvider(Dictionary<string, List<RoadStyle>> roadTypeStyleMapping)
        {
            _roadTypeStyleMapping = roadTypeStyleMapping;
        }

        /// <inheritdoc />
        public RoadStyle Get(Road road)
        {
            // NOTE use first element's type
            //var type = road.Elements[0].Type;

            // TODO use smart logic to choose road style
            var type = _roadTypeStyleMapping.Keys.First();

            return _roadTypeStyleMapping[type][0];
        }

        /// <inheritdoc />
        public RoadStyle Get(RoadJunction junction)
        {
            // NOTE use first element's type
            // TODO use smart logic to choose road style
            var type = _roadTypeStyleMapping.Keys.First();
            return _roadTypeStyleMapping[type][0];
        }
    }
}
