using ClipperLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeosLibrary.Models
{
    public class PolygonSet
    {
        public UPoint[] Outer { get; set; }
        public List<UPoint[]> Holes { get; set; } = new List<UPoint[]>();


        public static List<PolygonSet> AnalyzePolygons(List<UPoint[]> outerPolygons, List<UPoint[]> holePolygons, double scale = 1.0)
        {
            List<PolygonSet> PolygonSets = new List<PolygonSet>();

            foreach (UPoint[] outerPolygon in outerPolygons)
            {
                PolygonSet PolygonSet = new PolygonSet { Outer = outerPolygon };

                List<IntPoint> outerIntPolygon = outerPolygon.Select(p => new IntPoint(p.X * scale, p.Y * scale)).ToList();

                foreach (UPoint[] holePolygon in holePolygons)
                {
                    List<IntPoint> holeIntPolygon = holePolygon.Select(p => new IntPoint(p.X * scale, p.Y * scale)).ToList();

                    // if the hole is in the outer polygon
                    if (Clipper.PointInPolygon(holeIntPolygon[0], outerIntPolygon) != 0)
                    {
                        PolygonSet.Holes.Add(holePolygon);
                    }
                }

                PolygonSets.Add(PolygonSet);
            }

            return PolygonSets;
        }
    }
}
