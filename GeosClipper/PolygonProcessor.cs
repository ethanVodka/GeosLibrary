using System;
using System.Collections.Generic;
using ClipperLib;
using GeosLibrary.Models;

public class PolygonProcessor
{
    // UPoint を IntPoint に変換するための関数
    private static IntPoint ToIntPoint(UPoint point, double scale)
    {
        return new IntPoint((long)(point.X * scale), (long)(point.Y * scale));
    }

    // IntPoint を UPoint に変換するための関数
    private static UPoint ToUPoint(IntPoint point, double scale)
    {
        return new UPoint((double)(point.X / scale), (double)(point.Y / scale));
    }

    // UPoint 配列を IntPoint のリストに変換するための関数
    private static List<IntPoint> ToIntPoints(UPoint[] points, double scale)
    {
        var intPoints = new List<IntPoint>();
        foreach (var point in points)
        {
            intPoints.Add(ToIntPoint(point, scale));
        }
        return intPoints;
    }

    // IntPoint のリストを UPoint 配列に変換するための関数
    private static UPoint[] ToUPoints(List<IntPoint> points, double scale)
    {
        var uPoints = new List<UPoint>();
        foreach (var point in points)
        {
            uPoints.Add(ToUPoint(point, scale));
        }
        return uPoints.ToArray();
    }

    // メインの関数。この関数がポリゴン間の隙間を埋める処理を行う
    public static List<UPoint[]> CreateGapFillPolygons(List<UPoint[]> polygons, double distance, double scale)
    {
        var intPolygons = new List<List<IntPoint>>();
        foreach (var polygon in polygons)
        {
            // 入力されたポリゴンをIntPointのリストに変換
            intPolygons.Add(ToIntPoints(polygon, scale));
        }

        // ポリゴン間の隙間を埋める処理を実行
        var gapFillPolygons = CreateGapFillPolygons(intPolygons, distance * scale);

        var uPolygons = new List<UPoint[]>();
        foreach (var polygon in gapFillPolygons)
        {
            // 結果のポリゴンをUPointの配列に変換
            uPolygons.Add(ToUPoints(polygon, scale));
        }

        return uPolygons;
    }

    // ポリゴン間の隙間を埋める処理を実行する内部の関数
    private static List<List<IntPoint>> CreateGapFillPolygons(List<List<IntPoint>> polygons, double scaledDistance)
    {
        var solution = new List<List<IntPoint>>();
        var clipper = new Clipper();

        // ステップ1：ポリゴンを拡大
        foreach (var polygon in polygons)
        {
            var offsetPolygons = new List<List<IntPoint>>();
            var clipperOffset = new ClipperOffset();
            clipperOffset.AddPath(polygon, JoinType.jtMiter, EndType.etClosedPolygon);
            clipperOffset.Execute(ref offsetPolygons, scaledDistance);

            foreach (var offsetPolygon in offsetPolygons)
            {
                clipper.AddPath(offsetPolygon, PolyType.ptSubject, true);
            }
        }

        // ステップ2：ユニオン演算を実行して隙間を埋める
        var unionResult = new List<List<IntPoint>>();
        clipper.Execute(ClipType.ctUnion, unionResult, PolyFillType.pftNonZero, PolyFillType.pftNonZero);

        clipper.Clear();

        // ステップ3：ポリゴンを元のサイズに戻す
        foreach (var polygon in unionResult)
        {
            var offsetPolygons = new List<List<IntPoint>>();
            var clipperOffset = new ClipperOffset();
            clipperOffset.AddPath(polygon, JoinType.jtMiter, EndType.etClosedPolygon);
            clipperOffset.Execute(ref offsetPolygons, -scaledDistance);

            foreach (var offsetPolygon in offsetPolygons)
            {
                clipper.AddPath(offsetPolygon, PolyType.ptSubject, true);
            }
        }

        clipper.Execute(ClipType.ctUnion, solution, PolyFillType.pftNonZero, PolyFillType.pftNonZero);

        return solution;
    }
}
