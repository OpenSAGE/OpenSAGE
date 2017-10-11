using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Input;
using Caliburn.Micro;
using OpenSage.DataViewer.Framework;

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
            var programFilesPath = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);

            var possibleInstallations = new[]
            {
                new InstallationViewModel("C&C Generals (The First Decade)", Path.Combine(programFilesPath, @"EA Games\Command & Conquer The First Decade\Command & Conquer(tm) Generals")),
                new InstallationViewModel("C&C Generals (Origin)", Path.Combine(programFilesPath, @"Origin Games\Command and Conquer Generals Zero Hour\Command and Conquer Generals")),
                new InstallationViewModel("C&C Generals Zero Hour (The First Decade)", Path.Combine(programFilesPath, @"EA Games\Command & Conquer The First Decade\Command & Conquer(tm) Generals Zero Hour")),
                new InstallationViewModel("C&C Generals Zero Hour (Origin)", Path.Combine(programFilesPath, @"Origin Games\Command and Conquer Generals Zero Hour\Command and Conquer Generals Zero Hour")),
                new InstallationViewModel("Battle for Middle-earth (DVD)", Path.Combine(programFilesPath, @"EA Games\The Battle for Middle-earth (tm)"))
            };

            return possibleInstallations.Where(x => Directory.Exists(x.Path));
        }
    }
}
