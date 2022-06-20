namespace CurveRecognizer.Common;

public static class MathHelper
{
    public static (double k, double b) FindLineEquation(in Point first, in Point second)
    {
        var k = (double) (first.Y - second.Y) / (first.X - second.X);
        var b = first.Y - k * first.X;

        return (k, b);
    }

    public static (double k, double b) FindNormalLineEquation(double targetK, in Point point)
    {
        var k = 1.0 / targetK;
        var b = point.X * k + point.Y;

        return (-k, b);
    }

    public static int FindOrdinate(double k, double b, int x)
    {
        return (int) (k * x + b);
    }
}
