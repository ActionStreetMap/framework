using System.Text;

namespace ActionStreetMap.Osm.Formats.O5m
{
    internal static class O5mExtensions
    {
        public static string Decode(this Encoding e, byte[] chars, int start, int len)
        {
            byte[] bom = e.GetPreamble();
            if (bom.Length > 0)
            {
                if (len >= bom.Length)
                {
                    int pos = start;
                    bool hasBom = true;
                    for (int n = 0; n < bom.Length && hasBom; n++)
                    {
                        if (bom[n] != chars[pos++])
                            hasBom = false;
                    }
                    if (hasBom)
                    {
                        len -= pos - start;
                        start = pos;
                    }
                }
            }
            return e.GetString(chars, start, len);
        }
    }
}
