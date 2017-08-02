using System.IO;
using OpenZH.Data;

namespace OpenZH.DataViewer.ViewModels
{
    public class AudioVideoFileContentViewModel : FileContentViewModel
    {
        public string MediaFileName { get; }

        public AudioVideoFileContentViewModel(FileSystemEntry file)
            : base(file)
        {
            // TODO: This leaves files behind in the temp folder, not ideal.
            // But any trivial solutions have problems with the video player holding a lock on the previous file.
            var tempFileName = Path.GetTempFileName();
            MediaFileName = Path.ChangeExtension(tempFileName, Path.GetExtension(file.FilePath));
            System.IO.File.Delete(tempFileName);

            using (var entryStream = file.Open())
            using (var outputFileStream = System.IO.File.OpenWrite(MediaFileName))
                entryStream.CopyTo(outputFileStream);
        }
    }
}
