using System.Collections.Generic;

namespace CurveRecognizer.Common;

public class CurvePoint
{
    public Point Position { get; }
    public CurvePoint? Next { get; set; }

    public CurvePoint(Point position)
    {
        Position = position;
        Next = null;
    }
    
}
