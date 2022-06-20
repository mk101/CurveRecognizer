using System.Windows.Media.Imaging;

namespace CurveRecognizer.Common;

public interface IFilesService
{
    BitmapImage? Open();
    void Save(WriteableBitmap image);
}
