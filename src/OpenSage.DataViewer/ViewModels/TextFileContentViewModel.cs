using System.IO;
using OpenSage.Data;

namespace OpenSage.DataViewer.ViewModels
{
    public sealed class TextFileContentViewModel : FileContentViewModel
    {
        public string Text { get; }

        public TextFileContentViewModel(FileSystemEntry file)
            : base(file)
        {
            using (var fileStream = file.Open())
            using (var streamReader = new StreamReader(fileStream))
                Text = streamReader.ReadToEnd();
        }
    }
}
