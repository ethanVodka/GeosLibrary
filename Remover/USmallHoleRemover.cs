using System;
using System.Collections.Generic;
using GeosLibrary.Models;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;

namespace GeosLibrary.Models
{
    public class USmallHoleRemover
    {
        private class IsSmall : UHoleRemover.IPredicate
        {
            private readonly double _area;

            public IsSmall(double area)
            {
                _area = area;
            }

            public bool Value(Geometry geom)
            {
                double holeArea = Math.Abs(Area.OfRingSigned(geom.Coordinates));
                return holeArea <= _area;
            }
        }

        private static Geometry Clean(Geometry geom, double areaTolerance)
        {
            var remover = new UHoleRemover(geom, new IsSmall(areaTolerance));
            return remover.GetResult();
        }

        public static UPoint[] RemoveHoles(UPoint[] shell, List<UPoint[]>holes, double areaTolerance)
        {
            var geometryFactory = new GeometryFactory();

            // Convert shell points to coordinates
            Coordinate[] shellCoordinates = Array.ConvertAll(shell, p => new Coordinate(p.X, p.Y));

            // Create LinearRing for shell
            LinearRing shellRing = geometryFactory.CreateLinearRing(shellCoordinates);

            // Create a LinearRing for each hole
            List<LinearRing> holeRings = new List<LinearRing>();
            foreach (var holePoints in holes)
            {
                // Convert hole points to coordinates
                Coordinate[] holeCoordinates = Array.ConvertAll(holePoints, p => new Coordinate(p.X, p.Y));

                LinearRing hole = geometryFactory.CreateLinearRing(holeCoordinates);
                holeRings.Add(hole);
            }

            // Create Polygon with shell and holes
            Polygon polygon = geometryFactory.CreatePolygon(shellRing, holeRings.ToArray());
            Geometry cleanedGeometry = Clean(polygon, areaTolerance);

            return ExtractPointsFromCleanedGeometryIncludingHoles(cleanedGeometry);
        }

        private static UPoint[] ExtractPointsFromCleanedGeometryIncludingHoles(Geometry cleanedGeometry)
        {
            if (!(cleanedGeometry is Polygon cleanedPolygon))
                throw new ArgumentException("Input geometry is not a polygon.");

            List<UPoint> points = new List<UPoint>();

            // Add shell
            var cleanedShell = cleanedPolygon.ExteriorRing.Coordinates;
            points.AddRange(Array.ConvertAll(cleanedShell, p => new UPoint(p.X, p.Y)));

            foreach (var cleanedHole in cleanedPolygon.InteriorRings)
            {
                // Add special point to indicate a jump
                points.Add(new UPoint(double.NaN, double.NaN));

                // Add hole
                var holeCoordinates = cleanedHole.Coordinates;
                points.AddRange(Array.ConvertAll(holeCoordinates, p => new UPoint(p.X, p.Y)));
            }

            return points.ToArray();
        }
    }
}