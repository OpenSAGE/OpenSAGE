using System;
using System.Collections.Generic;
using System.IO;
using Caliburn.Micro;
using OpenSage.Data;

namespace OpenSage.DataViewer.ViewModels
{
    public class FileContentViewModel : PropertyChangedBase, IDisposable
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

                case ".map":
                    return new MapFileContentViewModel(file);

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

        private readonly List<IDisposable> _disposables;

        public FileSystemEntry File { get; }

        protected FileContentViewModel(FileSystemEntry file)
        {
            _disposables = new List<IDisposable>();

            File = file;
        }

        protected T AddDisposable<T>(T disposable)
            where T : IDisposable
        {
            _disposables.Add(disposable);
            return disposable;
        }

        public void Dispose()
        {
            _disposables.Reverse();
            foreach (var disposable in _disposables)
            {
                disposable.Dispose();
            }
            _disposables.Clear();
        }
    }
}
