using System;
using ActionStreetMap.Core;
using ActionStreetMap.Maps.Entities;

namespace ActionStreetMap.Maps.Data
{
    /// <summary> Defines behavior of element source editor. </summary>
    public interface IElementSourceEditor: IDisposable
    {
        /// <summary> Gets or sets elements source to edit; </summary>
        IElementSource ElementSource { get; set; }

        /// <summary> Gets element from  element source by given id.</summary>
        Element Get(int elementId);

        /// <summary> Adds element to element source. </summary>
        void Add(Element element);

        /// <summary> Edits element in element source. </summary>
        void Edit(Element element);

        /// <summary> Deletes element with given id from element source. </summary>
        void Delete(int elementId);

        /// <summary> Commits changes. </summary>
        void Commit();
    }

    /// <summary> Default implementation of <see cref="IElementSourceEditor"/>. </summary>
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
        public Element Get(int elementId)
        {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void Delete(int elementId)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void Commit()
        {
        }

        #endregion

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
