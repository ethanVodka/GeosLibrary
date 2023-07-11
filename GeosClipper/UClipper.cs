using ClipperLib;
using GeosLibrary.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeosLibrary.GeosClipper
{
    public class UClipper
    {
        public static List<UPoint[]> CombinePolygons(List<UPoint[]> polygons, double scale = 1.0)
        {
            Clipper clipper = new Clipper();

            foreach (var polygon in polygons)
            {
                var path = polygon.Select(p => new IntPoint(p.X * scale, p.Y * scale)).ToList();
                // true means the path is closed
                clipper.AddPath(path, PolyType.ptSubject, true);
            }

            List<List<IntPoint>> resultPolygons = new List<List<IntPoint>>();
            clipper.Execute(ClipType.ctUnion, resultPolygons, PolyFillType.pftNonZero, PolyFillType.pftNonZero);

            List<UPoint[]> finalPolygons = resultPolygons.Select(p => p.Select(ip => new UPoint(ip.X / scale, ip.Y / scale)).ToArray()).ToList();

            return finalPolygons;
        }

        public static void DividePolygons(List<UPoint[]> polygons, out List<UPoint[]> polygonsWithHoles, out List<UPoint[]> polygonsWithoutHoles, double scale = 1.0)
        {
            polygonsWithHoles = new List<UPoint[]>();
            polygonsWithoutHoles = new List<UPoint[]>();

            foreach (var polygon in polygons)
            {
                List<IntPoint> intPolygon = new List<IntPoint>();

                foreach (var point in polygon)
                {
                    intPolygon.Add(new IntPoint(point.X * scale, point.Y * scale));
                }

                if (Clipper.Orientation(intPolygon))
                {
                    polygonsWithoutHoles.Add(Utils.UArrayManager.AddFirstElementToLast(polygon));
                }
                else
                {
                    polygonsWithHoles.Add(Utils.UArrayManager.AddFirstElementToLast(polygon));
                }
            }
        }
    }
}
