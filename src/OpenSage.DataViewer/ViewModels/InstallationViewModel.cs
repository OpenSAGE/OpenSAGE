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

        private BindableCollection<TabViewModel> _tabs;
        public BindableCollection<TabViewModel> Tabs
        {
            get
            {
                if (_tabs == null)
                {
                    var tabs = new[]
                    {
                        new TabViewModel("3D Models", new[] { ".w3d" }),
                        new TabViewModel("Textures", new[] { ".dds", ".tga" }),
                        new TabViewModel("Maps", new[] { ".map", ".wak" }),
                        new TabViewModel("INI", new[] { ".ini" }),
                        new TabViewModel("Windows", new[] { ".wnd" }),
                        new TabViewModel("Audio", new[] { ".wav", ".mp3" }),
                        new TabViewModel("Videos", new[] { ".bik" }),
                        new TabViewModel("Cursors", new[] { ".ani" }),
                        new TabViewModel("Other", new string[0]),
                    };

                    foreach (var file in FileSystem.Files)
                    {
                        var fileExtension = System.IO.Path.GetExtension(file.FilePath).ToLower();
                        var tab = tabs.First(x => x.FileExtensions.Contains(fileExtension) || x.FileExtensions.Length == 0);
                        tab.Files.Add(file);
                    }

                    _tabs = new BindableCollection<TabViewModel>(tabs.Where(x => x.Files.Count > 0));

                    NotifyOfPropertyChange(nameof(SelectedTab));
                }
                return _tabs;
            }
        }

        private TabViewModel _selectedTab;
        public TabViewModel SelectedTab
        {
            get { return _selectedTab ?? (_selectedTab = Tabs.FirstOrDefault()); }
            set
            {
                _selectedTab = value;
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
