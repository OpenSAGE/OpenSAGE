using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using OpenSage.Data;

namespace OpenSage.DataViewer.ViewModels
{
    public sealed class InstallationViewModel : PropertyChangedBase
    {
        private FileSystem _fileSystem;

        public FileSystem FileSystem
        {
            get
            {
                if (_fileSystem == null)
                {
                    _fileSystem = new FileSystem(Path);
                }
                return _fileSystem;
            }
        }

        public string DisplayName { get; }
        public string Path { get; }

        private List<FileSystemEntryViewModel> _files;
        public IReadOnlyList<FileSystemEntryViewModel> Files
        {
            get
            {
                if (_files == null)
                {
                    _files = new List<FileSystemEntryViewModel>();
                    foreach (var file in FileSystem.Files)
                    {
                        _files.Add(new FileSystemEntryViewModel(file));
                    }

                    NotifyOfPropertyChange(nameof(SelectedFile));
                }
                return _files;
            }
        }

        private FileSystemEntryViewModel _selectedFile;
        public FileSystemEntryViewModel SelectedFile
        {
            get { return _selectedFile ?? (_selectedFile = Files.FirstOrDefault()); }
            set
            {
                _selectedFile = value;
                NotifyOfPropertyChange();

                SelectedFileContent = value?.CreateFileContentViewModel() ?? null;
            }
        }

        private FileContentViewModel _selectedFileContent;
        public FileContentViewModel SelectedFileContent
        {
            get { return _selectedFileContent; }
            private set
            {
                _selectedFileContent = value;
                NotifyOfPropertyChange();
            }
        }

        public InstallationViewModel(string displayName, string path)
        {
            DisplayName = displayName;
            Path = path;
        }
    }
}
