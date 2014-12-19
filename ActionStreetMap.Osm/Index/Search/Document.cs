using ActionStreetMap.Osm.Entities;

namespace ActionStreetMap.Osm.Index.Search
{
    public class Document
    {
        public int DocNumber { get; set; }
        public Element Element { get; set; }

        public Document(Element element)
        {
            DocNumber = -1;
            Element = element;
        }
    }
}
