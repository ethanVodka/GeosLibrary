using System;
using System.Collections.Generic;
using System.Linq;
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

        public static UPoint[] RemoveHoles(UPoint[] shell, List<UPoint[]> holes, double areaTolerance)
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

            var cleanedShell = cleanedPolygon.ExteriorRing.Coordinates;
            points.AddRange(Array.ConvertAll(cleanedShell, p => new UPoint(p.X, p.Y)));

            foreach (var cleanedHole in cleanedPolygon.InteriorRings)
            {
                var holeCoordinates = cleanedHole.Coordinates;

                // Add the first point of the shell to return to it after drawing the hole
                points.Add(points[0]);

                // Reverse the order of points for the hole
                var reversedHoleCoordinates = holeCoordinates.Reverse().ToArray();
                points.AddRange(Array.ConvertAll(reversedHoleCoordinates, p => new UPoint(p.X, p.Y)));


                // Return to the first point of the shell to close the path
                points.Add(points[0]);

            }
            return points.ToArray();
        }

        public static (Point[] Shell, List<Point[]> Holes) ExtractPointsFromGeometry(Geometry cleanedGeometry)
        {
            if (cleanedGeometry is Polygon cleanedPolygon)
            {
                var cleanedShell = cleanedPolygon.ExteriorRing.Coordinates;
                Point[] shellPoints = Array.ConvertAll(cleanedShell, p => new Point((int)p.X, (int)p.Y));

                var cleanedHoles = cleanedPolygon.InteriorRings;
                List<Point[]> holePointsList = new List<Point[]>();

                foreach (var ring in cleanedHoles)
                {
                    Point[] holePoints = Array.ConvertAll(ring.Coordinates, p => new Point((int)p.X, (int)p.Y));
                    holePointsList.Add(holePoints);
                }

                return (shellPoints, holePointsList);
            }
            else
            {
                throw new ArgumentException("Input geometry is not a polygon.");
            }

            /*             
                 #include "stdafx.h"
                 #include <msclr/marshal_cppstd.h>  // std::string と System::String の変換に使用
                 using namespace System;
                 using namespace NetTopologySuite::Geometries;
                 
                 // C++/CLIのメイン関数
                 int main(array<System::String^>^ args)
                 {
                     // C++/CLIから呼び出すために、C#の関数を呼び出す
                     Geometry^ cleanedGeometry = ...;  // クリーニングされたジオメトリを取得するコード
                 
                     // C++/CLIからC#の関数を呼び出す
                     std::pair<array<Point>^, List<array<Point>^>^> result;
                     result = ExtractPointsFromGeometry(cleanedGeometry);
                 
                     // C++の配列に変換する
                     array<Point>^ shellPoints = result.first;
                     List<array<Point>^>^ holePointsList = result.second;
                 
                     // C++の処理を続ける...
                     // (shellPointsとholePointsListを使用した処理)
                 
                     return 0;
                 }
             */
        }
    }
}