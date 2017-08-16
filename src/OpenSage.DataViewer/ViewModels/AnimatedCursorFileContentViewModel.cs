using System.Windows.Input;
using OpenSage.Data;

namespace OpenSage.DataViewer.ViewModels
{
    public sealed class AnimatedCursorFileContentViewModel : FileContentViewModel
    {
        public Cursor Cursor { get; }

        public AnimatedCursorFileContentViewModel(FileSystemEntry file)
            : base(file)
        {
            using (var fileStream = file.Open())
                Cursor = new Cursor(fileStream, true);
        }
    }
}
