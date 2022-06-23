using System.Windows;
using System.Windows.Media;
using CurveRecognizer.Common;
using CurveRecognizer.Models;

namespace CurveRecognizer.ViewModels;

public class ApplicationViewModel : NotifyPropertyChanged
{
    public ImageSource? ApplicationImage
    {
        get => _applicationImage;
        set
        {
            _applicationImage = value;
            OnPropertyChanged(nameof(ApplicationImage));
        }
    }

    public RelayCommand OpenFileCommand { get; }
    public RelayCommand SaveToFileCommand { get; }
    public RelayCommand ExitCommand { get; }
    
    public RelayCommand MarkEdgePointsCommand { get; }
    public RelayCommand MarkSkeletonCommand { get; }
    public RelayCommand ClearCommand { get; }

    public ApplicationViewModel()
    {
        var model = new CurveModel(null, new PngFilesService());
        
        OpenFileCommand = new RelayCommand(() => ApplicationImage = model.OpenImage());
        SaveToFileCommand = new RelayCommand(() => model.SaveImage());
        ExitCommand = new RelayCommand(() => Application.Current.Shutdown());
        
        MarkEdgePointsCommand = new RelayCommand(() => ApplicationImage = model.MarkPoints());
        MarkSkeletonCommand = new RelayCommand(() => ApplicationImage = model.MarkSkeleton());
        ClearCommand = new RelayCommand(() => ApplicationImage = model.Clear());
    }

    private ImageSource? _applicationImage;
}
