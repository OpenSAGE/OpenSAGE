using System;
using System.IO;
using OpenSage.Data;

namespace OpenSage.DataViewer.ViewModels
{
    public class FileContentViewModel : IDisposable
    {
        public static FileContentViewModel Create(FileSystemEntry file)
        {
            switch (Path.GetExtension(file.FilePath).ToLower())
            {
                case ".w3d":
                    return new W3dFileContentViewModel(file);

                case ".dds":
                case ".tga":
                    return new TextureFileContentViewModel(file);

                case ".wav":
                case ".mp3":
                case ".bik":
                    return new AudioVideoFileContentViewModel(file);

                case ".ani":
                    return new AnimatedCursorFileContentViewModel(file);

                case ".bmp":
                    return new BitmapFileContentViewModel(file);

                case ".csf":
                    return new CsfFileContentViewModel(file);

                case ".wnd":
                    return new WndFileContentViewModel(file);

                case ".txt":
                    return new TextFileContentViewModel(file);

                default:
                    return new UnsupportedFileContentViewModel(file);
            }
        }

        public FileSystemEntry File { get; }

        protected FileContentViewModel(FileSystemEntry file)
        {
            File = file;
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing) { }
    }
}
