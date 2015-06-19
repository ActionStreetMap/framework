using System;
using ActionStreetMap.Infrastructure.Dependencies;
using ActionStreetMap.Maps.Entities;

namespace ActionStreetMap.Maps.Data
{
    /// <summary> Defines behavior of element source editor. </summary>
    public interface IElementSourceEditor
    {
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
    internal sealed class ElementSourceEditor : IElementSourceEditor, IDisposable
    {
        private readonly IElementSourceProvider _elementSourceProvider;
        private IElementSource _elementSource;

        /// <summary> Creates instance of <see cref="ElementSourceEditor"/>. </summary>
        /// <param name="elementSourceProvider">Element source provider. </param>
        [Dependency]
        public ElementSourceEditor(IElementSourceProvider elementSourceProvider)
        {
            _elementSourceProvider = elementSourceProvider;
        }

        #region IElementSourceEditor implementation

        /// <inheritdoc />
        public Element Get(int elementId)
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc />
        public void Add(Element element)
        {
            EnsureElementSource();
            throw new System.NotImplementedException();
        }

        /// <inheritdoc />
        public void Edit(Element element)
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc />
        public void Delete(int elementId)
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc />
        public void Commit()
        {
        }

        #endregion

        private void EnsureElementSource()
        {
            if (_elementSource == null)
                _elementSource = null; // TODO
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Commit();
        }
    }
}
