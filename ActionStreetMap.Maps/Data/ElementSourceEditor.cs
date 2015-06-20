using System;
using System.Collections.Generic;
using ActionStreetMap.Core;
using ActionStreetMap.Core.Tiling.Models;
using ActionStreetMap.Infrastructure.Reactive;
using ActionStreetMap.Maps.Entities;
using Node = ActionStreetMap.Maps.Entities.Node;
using Way = ActionStreetMap.Maps.Entities.Way;

namespace ActionStreetMap.Maps.Data
{
    /// <summary> Defines behavior of element source editor. </summary>
    public interface IElementSourceEditor: IDisposable
    {
        /// <summary> Gets or sets elements source to edit; </summary>
        IElementSource ElementSource { get; set; }

        /// <summary> Adds element to element source. </summary>
        void Add(Element element);

        /// <summary> Edits element in element source. </summary>
        void Edit(Element element);

        /// <summary> Deletes element with given id from element source covered by given bounding box. </summary>
        void Delete<T>(long elementId, BoundingBox bbox) where T : Element;

        /// <summary> Commits changes. </summary>
        void Commit();
    }

    /// <summary> Default implementation of <see cref="IElementSourceEditor"/>. </summary>
    /// <remarks> Current implementation works only with <see cref="ElementSource"/>. </remarks>
    internal sealed class ElementSourceEditor : IElementSourceEditor
    {
        private ElementSource _elementSource;

        #region IElementSourceEditor implementation

        /// <inheritdoc />
        public IElementSource ElementSource
        {
            get { return _elementSource; }
            set
            {
                _elementSource = value as ElementSource;
                if (_elementSource == null)
                    throw new NotSupportedException(Strings.UnsupportedElementSource);
            }
        }

        /// <inheritdoc />
        public void Add(Element element)
        {
            var boundingBox = GetBoundingBox(element);
            var offset = _elementSource.ElementStore.Insert(element);
            _elementSource.SpatialIndexTree.Insert(offset, boundingBox);
        }

        /// <inheritdoc />
        public void Edit(Element element)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc />
        public void Delete<T>(long elementId, BoundingBox bbox) where T : Element
        {
            var element = CreateElementOfType(typeof (T), bbox);
            element.Id = elementId;
            element.Tags = new TagCollection()
                .Add(Strings.DeletedElementTagKey, String.Empty)
                .AsReadOnly();

            var offsets = _elementSource.SpatialIndexTree
                .Search(bbox).ToArray().Wait();

            foreach (var offset in offsets)
            {
                var elementInStore = _elementSource.ElementStore.Get(offset);
                if (elementInStore.Id == elementId)
                {
                    // NOTE so far, element is deleted only from spatial tree
                    _elementSource.SpatialIndexTree.Remove(offset, GetBoundingBox(elementInStore));
                    break;
                }
            }

            Add(element);
        }

        /// <inheritdoc />
        public void Commit()
        {
        }

        #endregion

        private Element CreateElementOfType(Type type, BoundingBox bbox)
        {
            if (type == typeof (Node))
                return new Node() { Coordinate = bbox.MinPoint };

            if (type == typeof(Way))
                return new Way() { Coordinates = new List<GeoCoordinate>() { bbox.MinPoint, bbox.MaxPoint} };

            throw new NotSupportedException(Strings.UnsupportedElementType);
        }

        private BoundingBox GetBoundingBox(Element element)
        {
            var boundingBox = new BoundingBox(
                new GeoCoordinate(double.MaxValue, double.MaxValue), 
                new GeoCoordinate(double.MinValue, double.MinValue));

            if (element is Way)
                foreach (var geoCoordinate in ((Way)element).Coordinates)
                    boundingBox += geoCoordinate;
            else if (element is Node)
                boundingBox += ((Node) element).Coordinate;
            else
                throw new NotSupportedException(Strings.UnsupportedElementType);

            return boundingBox;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Commit();
            _elementSource.Dispose();
        }
    }
}
