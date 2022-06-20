using System;
using System.IO;
using System.Windows.Media.Imaging;
using Microsoft.Win32;

namespace CurveRecognizer.Common;

public class PngFilesService : IFilesService
{
    public const string Filter = "png files (*.png)|*.png";

    public BitmapImage? Open()
    {
        var ofd = new OpenFileDialog
        {
            Filter = Filter
        };

        var result = ofd.ShowDialog();
        if (result == false)
        {
            return null;
        }

        return new BitmapImage(new Uri(ofd.FileName));
    }

    public void Save(WriteableBitmap image)
    {
        var sfd = new SaveFileDialog()
        {
            Filter = Filter
        };

        var result = sfd.ShowDialog();
        if (result == false)
        {
            return;
        }

        BitmapEncoder encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(image));

        using var fileStream = new FileStream(sfd.FileName, FileMode.Create);
        encoder.Save(fileStream);
    }
}
