using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;

namespace CurveRecognizer.Common;

public class CurveSplitService
{
    public CurveSplitService(BitmapImage image)
    {
        _image = image;
        _colors = new Color[0, 0];
    }

    public List<CurvePoint> SplitCurve()
    {
        GetPixelsColor();
        CalculateSize();
        
        var allPoints = GetAllPoints();

        FilterPointsWithDistance(allPoints, 20);

        AlignPoints(allPoints);
        
        FilterPointsWithDistance(allPoints, 10);

        var curvePoints = CalculateCurvePoints(allPoints);

// #if DEBUG
//         DebugPrint(curvePoints, 1);
// #endif
        return curvePoints;
    }

    private List<CurvePoint> CalculateCurvePoints(IList<Point> points)
    {
        var result = new List<CurvePoint>();
        var startPoint = new CurvePoint(points[0]);

        CurvePoint? current = startPoint;
        CurvePoint? previous = null;
        
        result.Add(startPoint);
        points.Remove(startPoint.Position);
        int size = points.Count;
       
        while (result.Count <= size)
        {
            int distance = 10;
            var neighbors = points.Where(p => p.LengthTo(current.Position) < distance && p != current.Position).ToList();
            while (!neighbors.Any())
            {
                distance += 5;
                neighbors = points.Where(p => p.LengthTo(current.Position) < distance && p != current.Position).ToList();
            }

            if (neighbors.Count == 1)
            {
                var next = new CurvePoint(neighbors.First());
                current.Next = next;
                
                previous = current;
                current = current.Next;
                
                result.Add(current);
                points.Remove(current.Position);
                continue;
            }

            if (previous is null)
            {
                var next = new CurvePoint(neighbors.First());
                current.Next = next;
                
                previous = current;
                current = current.Next;
                
                result.Add(current);
                points.Remove(current.Position);
                continue;
            }

            var (k, _) = MathHelper.FindLineEquation(previous.Position, current.Position);
            CurvePoint? candidate = null;
            var difference = double.MaxValue;

            foreach (var neighbor in neighbors)
            {
                var (candidateK, _) = MathHelper.FindLineEquation(current.Position, neighbor);
                if (Math.Abs(k - candidateK) < difference)
                {
                    candidate = new CurvePoint(neighbor);
                    difference = Math.Abs(k - candidateK);
                }
            }

            // if (difference > 5.0)
            // {
            //     distance *= 100;
            //     continue;
            // }

            if (candidate is null)
            {
                throw new NullReferenceException("Failed to processing curve");
            }
            current.Next = candidate;

            previous = current;
            current = current.Next;
            
            result.Add(current);
            points.Remove(current.Position);
        }

        return result;
    }

    private void AlignPoints(IList<Point> allPoints)
    {
        for (int i = 0; i < allPoints.Count; i++)
        {
            var point = allPoints[i];
            var left = new Point(point.X - 1, point.Y );
            var right = new Point(point.X + 1, point.Y);

            var firstPoint = FindEdgePoint(left);
            var secondPoint = FindEdgePoint(right);

            if (firstPoint.Y == secondPoint.Y)
            {
                var top = FindEdgePoint(point);
                var bottom = FindEdgePoint(point, false);

                point.Y = (top.Y + bottom.Y) / 2;
                allPoints[i] = point;
                continue;
            }

            var (targetK, _) = MathHelper.FindLineEquation(firstPoint, secondPoint);
            var (k, b) = MathHelper.FindNormalLineEquation(targetK, point);

            var start = FindAbscissaEdge(point, k, b, false);
            var finish = FindAbscissaEdge(point, k, b);

            point.X = (start + finish) / 2;
            point.Y = MathHelper.FindOrdinate(k, b, point.X);

            allPoints[i] = point;
        }
    }

    private int FindAbscissaEdge(in Point start, double k, double b, bool isRightDirection = true)
    {
        int step = isRightDirection ? 1 : -1;
        int x = start.X;
        
        while (GetPixel(x, MathHelper.FindOrdinate(k, b, x)).IsBlack)
        {
            x += step;
        }

        return x - step;
    }

    private Point FindEdgePoint(in Point start, bool isUpDirection = true)
    {
        var step = isUpDirection ? 1 : -1;
        var delta = 0;
        while (GetPixel(start.X, start.Y + delta).IsBlack)
        {
            delta += step;
        }

        delta -= step;

        return new Point(start.X, start.Y + delta);
    }

    private void DebugPrint(IReadOnlyList<CurvePoint> points, int size)
    {
        var width = _image.PixelWidth;
        var height = _image.PixelHeight;
        var pixels = new byte[width * 4 * height];
        _image.CopyPixels(pixels, width * 4, 0);

        for (var i = 0; i < points.Count; i++)
        {
            var point = points[i].Position;
            var stride = width * 4;

            var colorA = new byte[] {0, 0, 255};
            var colorB = new byte[] {255, 0, 0};
            for (int x = point.X - size; x < point.X + size; x++)
            {
                for (int y = point.Y - size; y < point.Y + size; y++)
                {
                    var index = y * stride + 4 * x;
                    var val = (double)i / points.Count;
                    pixels[index] = (byte)(colorA[0] + val * (colorB[0] - colorA[0]));
                    pixels[index + 1] = (byte)(colorA[1] + val * (colorB[1] - colorA[1]));
                    pixels[index + 2] = (byte)(colorA[2] + val * (colorB[2] - colorA[2]));
                }
            }
        }

        var writeableBitmap = new WriteableBitmap(_image);
        writeableBitmap.WritePixels(new Int32Rect(0, 0, width, height), pixels, width * 4, 0);

        BitmapEncoder encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(writeableBitmap));

        using var fileStream = new FileStream(@"D:\debug_curve.png", FileMode.Create);
        encoder.Save(fileStream);
    }

    private void FilterPointsWithDistance(List<Point> allPoints, double distance)
    {
        while (true)
        {
            List<Point>? neighbors = null;
            foreach (var point in allPoints)
            {
                neighbors = allPoints.Where(p => p.LengthTo(point) <= distance && p != point).ToList();
                if (neighbors.Any())
                {
                    break;
                }
            }

            if (neighbors is not null && neighbors.Any())
            {
                //allPoints = allPoints.Except(neighbors).ToList();
                foreach (var neighbor in neighbors)
                {
                    allPoints.Remove(neighbor);
                }

                continue;
            }

            break;
        }
    }

    private List<Point> GetAllPoints()
    {
        var width = _image.PixelWidth;
        var height = _image.PixelHeight;
        var allPoints = new List<Point>();

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (_colors[i, j].IsWhite)
                {
                    continue;
                }
                
                if (IsFit(i, j, _size))
                {
                    allPoints.Add(new Point(i, j));
                }
            }
        }

        return allPoints;
    }

    private void GetPixelsColor()
    {
        var width = _image.PixelWidth;
        var height = _image.PixelHeight;
        var pixels = new byte[width * 4 * height];
        _image.CopyPixels(pixels, width * 4, 0);

        _colors = new Color[width, height];
        var stride = width * 4;
        for (int j = 0; j < height; j++)
        {
            for (int i = 0; i < width; i++)
            {
                var index = j * stride + 4 * i;
                var red = pixels[index];
                var green = pixels[index + 1];
                var blue = pixels[index + 2];
                var alpha = pixels[index + 3];

                if (red != 255 && red == green && green == blue)
                {
                    red = 0;
                    green = 0;
                    blue = 0;
                }
                _colors[i, j] = new Color(red, green, blue, alpha);
            }
        }
    }

    /// <summary>
    /// Get pixel color with overflow
    /// </summary>
    private Color GetPixel(int x, int y)
    {
        var width = _colors.GetLength(0);
        var height = _colors.GetLength(1);

        if (x < 0 || x >= width)
        {
            return new Color(255, 255, 255, 255);
        }

        if (y < 0 || y >= height)
        {
            return new Color(255, 255, 255, 255);
        }

        return _colors[x, y];
    }

    private bool IsFit(int x, int y, int size)
    {
        return GetPixel(x - size, y).IsBlack
               && GetPixel(x + size, y).IsBlack
               && GetPixel(x, y - size).IsBlack
               && GetPixel(x, y + size).IsBlack;
    }
    
    private void CalculateSize()
    {
        var width = _colors.GetLength(0);
        var height = _colors.GetLength(1);

        List<int> sizes = new List<int>();
        
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                var color = _colors[i, j];
                if (color.IsWhite)
                {
                    continue;
                }

                if (!color.IsBlack)
                {
                    throw new InvalidDataException("Picture must be black and white");
                }

                int localSize = 1;
                while (true)
                {
                    if (!IsFit(i, j, localSize))
                    {
                        break;
                    }

                    localSize += 1;
                }
                
                sizes.Add(localSize);
            }
        }

        _size = sizes.GroupBy(i => i).OrderByDescending(grp => grp.Count()).Select(grp => grp.Key).First();
    }

    private readonly BitmapImage _image;
    private Color[,] _colors;
    private int _size = 1;
}
