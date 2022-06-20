using System;

namespace CurveRecognizer.Common;

public struct Point
{
    public int X { get; set; }
    public int Y { get; set; }

    public Point(int x, int y)
    {
        X = x;
        Y = y;
    }

    public double LengthTo(in Point point) => Length(this, point);

    public static double Length(in Point first, in Point second)
    {
        return Math.Sqrt(
            Math.Pow(second.X - first.X, 2) +
            Math.Pow(second.Y - first.Y, 2)
        );
    }

    public bool Equals(Point other)
    {
        return X == other.X && Y == other.Y;
    }

    public override bool Equals(object? obj)
    {
        return obj is Point other && Equals(other);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            return (X * 397) ^ Y;
        }
    }

    public static bool operator ==(Point left, Point right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Point left, Point right)
    {
        return !left.Equals(right);
    }
}
