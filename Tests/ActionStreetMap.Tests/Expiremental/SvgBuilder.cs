using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using ActionStreetMap.Core.Geometry.Clipping;
using Path = System.Collections.Generic.List<ActionStreetMap.Core.Geometry.Clipping.IntPoint>;
using Paths = System.Collections.Generic.List<System.Collections.Generic.List<ActionStreetMap.Core.Geometry.Clipping.IntPoint>>;

namespace ActionStreetMap.Tests.Expiremental
{
    public class SVGBuilder
    {
        public class StyleInfo
        {
            public PolyFillType pft;
            public Color brushClr;
            public Color penClr;
            public double penWidth;
            public int[] dashArray;
            public Boolean showCoords;
            public StyleInfo Clone()
            {
                StyleInfo si = new StyleInfo();
                si.pft = this.pft;
                si.brushClr = this.brushClr;
                si.dashArray = this.dashArray;
                si.penClr = this.penClr;
                si.penWidth = this.penWidth;
                si.showCoords = this.showCoords;
                return si;
            }
            public StyleInfo()
            {
                pft = PolyFillType.pftNonZero;
                brushClr = Color.AntiqueWhite;
                dashArray = null;
                penClr = Color.Black;
                penWidth = 0.8;
                showCoords = true;
            }
        }

        public class PolyInfo
        {
            public Paths polygons;
            public StyleInfo si;
        }

        public StyleInfo style;
        private List<PolyInfo> PolyInfoList;
        const string svg_header = "<?xml version=\"1.0\" standalone=\"no\"?>\n" +
          "<!DOCTYPE svg PUBLIC \"-//W3C//DTD SVG 1.0//EN\"\n" +
          "\"http://www.w3.org/TR/2001/REC-SVG-20010904/DTD/svg10.dtd\">\n\n" +
          "<svg width=\"{0}px\" height=\"{1}px\" viewBox=\"0 0 {2} {3}\" " +
          "version=\"1.1\" xmlns=\"http://www.w3.org/2000/svg\">\n\n";
        const string svg_path_format = "\"\n style=\"fill:{0};" +
            " fill-opacity:{1:f2}; fill-rule:{2}; stroke:{3};" +
            " stroke-opacity:{4:f2}; stroke-width:{5:f2};\"/>\n\n";

        public SVGBuilder()
        {
            PolyInfoList = new List<PolyInfo>();
            style = new StyleInfo();
        }

        public void AddPaths(Paths poly)
        {
            if (poly.Count == 0) return;
            PolyInfo pi = new PolyInfo();
            pi.polygons = poly;
            pi.si = style.Clone();
            PolyInfoList.Add(pi);
        }

        public static void SaveToFile(Paths poly, string filename, double scale = 1.0, int margin = 10)
        {
            var svgBuilder = new SVGBuilder();
            svgBuilder.AddPaths(poly);
            svgBuilder.SaveToFile(filename, scale, margin);
        }

        public Boolean SaveToFile(string filename, double scale = 1.0, int margin = 10)
        {
            if (scale == 0) scale = 1.0;
            if (margin < 0) margin = 0;

            //calculate the bounding rect ...
            int i = 0, j = 0;
            while (i < PolyInfoList.Count)
            {
                j = 0;
                while (j < PolyInfoList[i].polygons.Count &&
                    PolyInfoList[i].polygons[j].Count == 0) j++;
                if (j < PolyInfoList[i].polygons.Count) break;
                i++;
            }
            if (i == PolyInfoList.Count) return false;
            IntRect rec = new IntRect();
            rec.left = PolyInfoList[i].polygons[j][0].X;
            rec.right = rec.left;
            rec.top = PolyInfoList[0].polygons[j][0].Y;
            rec.bottom = rec.top;

            for (; i < PolyInfoList.Count; i++)
            {
                foreach (Path pg in PolyInfoList[i].polygons)
                    foreach (IntPoint pt in pg)
                    {
                        if (pt.X < rec.left) rec.left = pt.X;
                        else if (pt.X > rec.right) rec.right = pt.X;
                        if (pt.Y < rec.top) rec.top = pt.Y;
                        else if (pt.Y > rec.bottom) rec.bottom = pt.Y;
                    }
            }

            rec.left = (Int64)((double)rec.left * scale);
            rec.top = (Int64)((double)rec.top * scale);
            rec.right = (Int64)((double)rec.right * scale);
            rec.bottom = (Int64)((double)rec.bottom * scale);
            Int64 offsetX = -rec.left + margin;
            Int64 offsetY = -rec.top + margin;

            StreamWriter writer = new StreamWriter(filename);
            if (writer == null) return false;
            writer.Write(svg_header,
                (rec.right - rec.left) + margin * 2,
                (rec.bottom - rec.top) + margin * 2,
                (rec.right - rec.left) + margin * 2,
                (rec.bottom - rec.top) + margin * 2);

            foreach (PolyInfo pi in PolyInfoList)
            {
                writer.Write(" <path d=\"");
                foreach (Path p in pi.polygons)
                {
                    if (p.Count < 3) continue;
                    writer.Write(String.Format(NumberFormatInfo.InvariantInfo, " M {0:f2} {1:f2}",
                        (double)((double)p[0].X * scale + offsetX),
                        (double)((double)p[0].Y * scale + offsetY)));
                    for (int k = 1; k < p.Count; k++)
                    {
                        writer.Write(String.Format(NumberFormatInfo.InvariantInfo, " L {0:f2} {1:f2}",
                        (double)((double)p[k].X * scale + offsetX),
                        (double)((double)p[k].Y * scale + offsetY)));
                    }
                    writer.Write(" z");
                }

                writer.Write(String.Format(NumberFormatInfo.InvariantInfo, svg_path_format,
                ColorTranslator.ToHtml(pi.si.brushClr),
                (float)pi.si.brushClr.A / 255,
                (pi.si.pft == PolyFillType.pftEvenOdd ? "evenodd" : "nonzero"),
                ColorTranslator.ToHtml(pi.si.penClr),
                (float)pi.si.penClr.A / 255,
                pi.si.penWidth));

                if (pi.si.showCoords)
                {
                    writer.Write("<g font-family=\"Verdana\" font-size=\"11\" fill=\"black\">\n\n");
                    foreach (Path p in pi.polygons)
                    {
                        foreach (IntPoint pt in p)
                        {
                            Int64 x = pt.X;
                            Int64 y = pt.Y;
                            writer.Write(String.Format(
                                "<text x=\"{0}\" y=\"{1}\">{2},{3}</text>\n",
                                (int)(x * scale + offsetX), (int)(y * scale + offsetY), x, y));

                        }
                        writer.Write("\n");
                    }
                    writer.Write("</g>\n");
                }
            }
            writer.Write("</svg>\n");
            writer.Close();
            return true;
        }
    }    
}
