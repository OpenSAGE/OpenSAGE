using System.IO;
using OpenSage.Data;

namespace OpenSage.DataViewer.ViewModels
{
    public sealed class FileSystemEntryViewModel
    {
        private readonly FileSystemEntry _entry;

        public string FilePath => _entry.FilePath;

        private string FileExtension => Path.GetExtension(_entry.FilePath).ToLower();

        public string FileTypeDisplayName
        {
            get
            {
                switch (FileExtension)
                {
                    case ".w3d":
                        return "3D Models";

                    case ".dds":
                    case ".tga":
                        return "Textures";

                    case ".map":
                    case ".wak":
                        return "Maps";

                    case ".ini":
                        return "INI";

                    case ".wnd":
                        return "Windows";

                    case ".wav":
                    case ".mp3":
                        return "Audio";

                    case ".bik":
                        return "Videos";

                    case ".ani":
                        return "Cursors";

                    default:
                        return "Other";
                }
            }
        }

        public FileSystemEntryViewModel(FileSystemEntry entry)
        {
            _entry = entry;
        }

        public FileContentViewModel CreateFileContentViewModel()
        {
            switch (FileExtension)
            {
                case ".w3d":
                    return new W3dFileContentViewModel(_entry);

                case ".dds":
                case ".tga":
                    return new TextureFileContentViewModel(_entry);

                case ".map":
                    return new MapFileContentViewModel(_entry);

                case ".wav":
                case ".mp3":
                case ".bik":
                    return new AudioVideoFileContentViewModel(_entry);

                case ".ani":
                    return new AnimatedCursorFileContentViewModel(_entry);

                case ".bmp":
                    return new BitmapFileContentViewModel(_entry);

                case ".csf":
                    return new CsfFileContentViewModel(_entry);

                case ".wnd":
                    return new WndFileContentViewModel(_entry);

                case ".ini":
                    return new IniFileContentViewModel(_entry);

                case ".txt":
                    return new TextFileContentViewModel(_entry);

                default:
                    return new UnsupportedFileContentViewModel(_entry);
            }
        }
    }
}
