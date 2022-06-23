namespace CurveRecognizer.Common;

public struct Color
{
    public byte Red { get; set; }
    public byte Green { get; set; }
    public byte Blue { get; set; }
    public byte Alpha { get; set; }

    public bool IsWhite => 
        Red == 255 && Green == 255 && Blue == 255;
    
    public bool IsBlack => 
        Red == 0 && Green == 0 && Blue == 0;

    public Color(byte red, byte green, byte blue, byte alpha)
    {
        Red = red;
        Green = green;
        Blue = blue;
        Alpha = alpha;
    }
}
