using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Caliburn.Micro;
using OpenSage.DataViewer.Framework;
using OpenSage.Data;

namespace OpenSage.DataViewer.ViewModels
{
    public class ShellViewModel : PropertyChangedBase
    {
        public BindableCollection<InstallationViewModel> Installations { get; }

        private InstallationViewModel _selectedInstallation;
        public InstallationViewModel SelectedInstallation
        {
            get { return _selectedInstallation; }
            set
            {
                _selectedInstallation = value;
                NotifyOfPropertyChange();

                var gameService = IoC.Get<GameService>();
                gameService.OnFileSystemChanged(value.FileSystem);
            }
        }

        public ICommand SelectInstallationCommand { get; }

        public ShellViewModel()
        {
            Installations = new BindableCollection<InstallationViewModel>(FindInstallations());
            SelectedInstallation = Installations.FirstOrDefault();

            SelectInstallationCommand = new RelayCommand<InstallationViewModel>(x => SelectedInstallation = x);
        }

        private static IEnumerable<InstallationViewModel> FindInstallations()
        {
            var locator = IoC.Get<IInstallationLocator>();

            return SageGames.GetAll()
                .SelectMany(locator.FindInstallations)
                .Select(installation => new InstallationViewModel(installation.DisplayName, installation.Path));
        }
    }
}
