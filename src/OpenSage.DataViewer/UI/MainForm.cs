using System;
using System.IO;
using Eto.Drawing;
using Eto.Forms;
using OpenSage.Data;
using OpenSage.DataViewer.Framework;
using OpenSage.Mods.BuiltIn;

namespace OpenSage.DataViewer.UI
{
    public sealed class MainForm : Form
    {
        public event EventHandler<InstallationChangedEventArgs> InstallationChanged;

        private readonly ButtonMenuItem _installationsMenuItem;
        private readonly ImageView _installationImageView;

        private GameInstallation _installation;
        private FileSystem _fileSystem;

        public MainForm()
        {
            ClientSize = new Size(1024, 768);
            Maximize();

            Title = "OpenSAGE Data Viewer";

            Icon = Icon.FromResource("OpenSage.DataViewer.Resources.AppIcon.ico");

            var quitCommand = new Command((sender, e) => Application.Instance.Quit())
            {
                MenuText = "Quit",
                Shortcut = Application.Instance.CommonModifier | Keys.Q
            };

            _installationsMenuItem = new ButtonMenuItem { Text = "&Installation" };
            RadioCommand firstInstallationCommand = null;

            var installations = GameInstallation.FindAll(GameDefinition.All);
            foreach (var installation in installations)
            {
                var installationCommand = new RadioCommand((sender, e) => ChangeInstallation(installation))
                {
                    MenuText = installation.Game.DisplayName.Replace("&", "&&")
                };

                if (firstInstallationCommand == null)
                {
                    firstInstallationCommand = installationCommand;
                }
                else
                {
                    installationCommand.Controller = firstInstallationCommand;
                }

                _installationsMenuItem.Items.Add(installationCommand);
            }

            Menu = new MenuBar
            {
                Items =
                {
                    _installationsMenuItem,
                },
                QuitItem = quitCommand
            };

            _installationImageView = new ImageView();
            _installationImageView.Width = 250;
            _installationImageView.Height = 187;

            var contentView = new ContentView(() => _installation, () => _fileSystem);

            var filesList = new FilesList(this) { Width = 250 };
            filesList.SelectedFileChanged += (sender, e) =>
            {
                contentView.SetContent(e.Entry);
            };

            var sidebar = new TableLayout(
                _installationImageView,
                filesList);

            Content = new Splitter
            {
                Panel1 = sidebar,
                Panel2 = contentView
            };
        }

        protected override void OnShown(EventArgs e)
        {
            if (_installationsMenuItem.Items.Count > 0)
            {
                _installationsMenuItem.Items[0].PerformClick();
            }

            base.OnShown(e);
        }

        private void ChangeInstallation(GameInstallation installation)
        {
            _installation = installation;

            if (_fileSystem != null)
            {
                _fileSystem.Dispose();
                _fileSystem = null;
            }

            var launcherImagePath = installation.Game.LauncherImagePath;
            if (launcherImagePath != null)
            {
                var fullImagePath = Path.Combine(installation.Path, launcherImagePath);
                _installationImageView.Image = new Bitmap(fullImagePath);
            }
            else
            {
                _installationImageView.Image = null;
            }

            _fileSystem = installation.CreateFileSystem();

            InstallationChanged?.Invoke(this, new InstallationChangedEventArgs(installation, _fileSystem));
        }

        protected override void Dispose(bool disposing)
        {
            _fileSystem?.Dispose();

            base.Dispose(disposing);
        }
    }
}
