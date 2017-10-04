using System.Collections.ObjectModel;
using System.IO;
using Caliburn.Micro;
using OpenSage.Data;

namespace OpenSage.DataViewer.ViewModels
{
    public abstract class FileContentViewModelBase : PropertyChangedBase
    {

    }

    public abstract class FileContentViewModel : FileContentViewModelBase
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

                case ".ini":
                    return new IniFileContentViewModel(file);

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
    }

    public abstract class FileContentViewModel<TSubObject> : FileContentViewModel
        where TSubObject : FileSubObjectViewModel
    {
        public ObservableCollection<TSubObject> SubObjects { get; } = new ObservableCollection<TSubObject>();

        private TSubObject _selectedSubObject;

        public TSubObject SelectedSubObject
        {
            get { return _selectedSubObject; }
            set
            {
                _selectedSubObject?.Deactivate();

                _selectedSubObject = value;

                _selectedSubObject?.Activate();

                OnSelectedSubObjectChanged(_selectedSubObject);

                NotifyOfPropertyChange();
            }
        }

        protected virtual void OnSelectedSubObjectChanged(TSubObject subObject) { }

        protected FileContentViewModel(FileSystemEntry file) 
            : base(file)
        {
        }
    }

    public abstract class FileSubObjectViewModel : FileContentViewModelBase
    {
        public abstract string GroupName { get; }
        public abstract string Name { get; }

        public virtual void Activate() { }

        public virtual void Deactivate() { }
    }
}
