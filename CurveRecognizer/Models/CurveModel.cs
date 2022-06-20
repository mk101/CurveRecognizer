using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;
using CurveRecognizer.Common;

namespace CurveRecognizer.Models;

public class CurveModel
{
    public CurveModel(BitmapImage? image, IFilesService filesService)
    {
        _base = image;
        _filesService = filesService;
        _image = null;
        
        if (_base is not null)
        {
            _image = new WriteableBitmap(_base);
        }

        _points = null;
    }

    public BitmapImage? OpenImage()
    {
        var image = _filesService.Open();
        if (image is null)
        {
            return _base;
        }

        _base = image;
        _image = new WriteableBitmap(_base);
        _points = null;

        return _base;
    }

    public void SaveImage()
    {
        if (_image is null)
        {
            throw new ArgumentException("Image must be set");
        }
        
        _filesService.Save(_image);
    }

    public WriteableBitmap MarkPoints()
    {
        if (_base is null)
        {
            throw new ArgumentException("Image expected");
        }

        _image = new WriteableBitmap(_base);
        if (_points is null)
        {
            _curveSplitService = new CurveSplitService(_base);
            _points = _curveSplitService.SplitCurve();
        }

        var width = _image.PixelWidth;
        var height = _image.PixelHeight;
        var stride = width * 4;
        var pixels = new byte[width * 4 * height];
        _image.CopyPixels(pixels, width * 4, 0);
        
        var colorA = new byte[] {0, 0, 255};
        var colorB = new byte[] {255, 0, 0};

        var first = _points.First();
        var last = _points.Last();
        
        MarkPointInArray(first, 3, pixels, stride, colorA);
        
        MarkPointInArray(last, 3, pixels, stride, colorB);

        _image.WritePixels(new Int32Rect(0, 0, width, height), pixels, stride, 0);

        return _image;
    }

    private static void MarkPointInArray(CurvePoint point, int size, byte[] pixels, int stride, byte[] color)
    {
        for (int i = point.Position.X - size; i < point.Position.X + size; i++)
        {
            for (int j = point.Position.Y - size; j < point.Position.Y + size; j++)
            {
                var index = j * stride + 4 * i;
                for (int k = 0; k < 3; k++)
                {
                    pixels[index + k] = color[k];
                }
            }
        }
    }

    public WriteableBitmap MarkSkeleton()
    {
        if (_base is null)
        {
            throw new ArgumentException("Image expected");
        }

        _image = new WriteableBitmap(_base);
        if (_points is null)
        {
            _curveSplitService = new CurveSplitService(_base);
            _points = _curveSplitService.SplitCurve();
        }

        var width = _image.PixelWidth;
        var height = _image.PixelHeight;
        var stride = width * 4;
        var pixels = new byte[width * 4 * height];
        _image.CopyPixels(pixels, width * 4, 0);
        
        var colorA = new byte[] {0, 0, 255};
        var colorB = new byte[] {255, 0, 0};

        for (var i = 0; i < _points.Count; i++)
        {
            var point = _points[i];
            var val = (double) i / _points.Count;
            var color = new byte[3];
            
            color[0] = (byte) (colorA[0] + val * (colorB[0] - colorA[0]));
            color[1] = (byte) (colorA[1] + val * (colorB[1] - colorA[1]));
            color[2] = (byte) (colorA[2] + val * (colorB[2] - colorA[2]));
            
            MarkPointInArray(point, 3, pixels, stride, color);
        }

        _image.WritePixels(new Int32Rect(0, 0, width, height), pixels, stride, 0);

        return _image;
    }

    public BitmapImage Clear()
    {
        if (_base is null)
        {
            throw new ArgumentException("Image must be set");
        }
        
        _image = null;
        return _base;
    }

    private BitmapImage? _base;
    private WriteableBitmap? _image;
    private readonly IFilesService _filesService;
    private CurveSplitService? _curveSplitService;

    private List<CurvePoint>? _points;
}
