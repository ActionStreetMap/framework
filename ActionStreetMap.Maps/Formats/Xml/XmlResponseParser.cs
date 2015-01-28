using System.IO;
using System.Xml;

namespace ActionStreetMap.Maps.Formats.Xml
{
    /// <summary> Provides API to parse response of Overpass API backend. </summary>
    public class XmlResponseParser
    {
        private readonly string _response;
        
        /// <summary> Creates instance of <see cref="XmlResponseParser"/>. </summary>
        /// <param name="response">Raw response string.</param>
        public XmlResponseParser(string response)
        {
            _response = response;
        }

        public void Parse()
        {
            using (XmlReader reader = XmlReader.Create(new StringReader(_response)))
            {
                reader.MoveToContent();
                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element)
                    {
                       
                    }
                }
            }
        }
    }
}
