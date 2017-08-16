using System.Windows.Media;
using System.Windows.Media.Imaging;
using OpenSage.Data;

namespace OpenSage.DataViewer.ViewModels
{
    public sealed class BitmapFileContentViewModel : FileContentViewModel
    {
        public ImageSource ImageSource { get; }

        public BitmapFileContentViewModel(FileSystemEntry file)
            : base(file)
        {
            using (var fileStream = file.Open())
                ImageSource = BitmapFrame.Create(fileStream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
        }
    }
}
