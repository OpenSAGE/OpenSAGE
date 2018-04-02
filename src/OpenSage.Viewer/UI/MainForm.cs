using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using ImGuiNET;
using OpenSage.Data;
using OpenSage.Mods.BuiltIn;

namespace OpenSage.Viewer.UI
{
    internal sealed class MainForm : DisposableBase
    {
        private readonly List<GameInstallation> _installations;

        private GameInstallation _selectedInstallation;
        private FileSystem _fileSystem;

        private List<FileSystemEntry> _files;
        private int _currentFile;

        private byte[] _dataInput = new byte[32];

        private ContentView _contentView;

        public MainForm()
        {
            _installations = GameInstallation.FindAll(GameDefinition.All).ToList();

            ChangeInstallation(_installations.FirstOrDefault());
        }

        public unsafe void Draw()
        {
            ImGui.SetNextWindowPos(Vector2.Zero, Condition.Always, Vector2.Zero);
            ImGui.SetNextWindowSize(new Vector2(1024, 768), Condition.Always);

            ImGui.BeginWindow("OpenSAGE Viewer", WindowFlags.MenuBar | WindowFlags.NoTitleBar);

            ImGui.BeginMainMenuBar();
            if (ImGui.BeginMenu("Installation"))
            {
                foreach (var installation in _installations)
                {
                    if (ImGui.MenuItem(installation.Game.DisplayName, null, _selectedInstallation == installation, true))
                    {
                        ChangeInstallation(installation);
                    }
                }

                ImGui.EndMenu();
            }
            ImGui.EndMainMenuBar();

            ImGui.BeginChild("sidebar", new Vector2(250, 0), true, 0);

            TextEditCallback callback = (data) =>
            {
                int* p_cursor_pos = (int*) data->UserData;
                if (!data->HasSelection())
                    *p_cursor_pos = data->CursorPos;
                return 0;
            };

            ImGui.InputText("Search", _dataInput, (uint) _dataInput.Length, InputTextFlags.Default, callback);

            var searchText = Encoding.UTF8.GetString(_dataInput).TrimEnd('\0');

            for (var i = 0; i < _files.Count; i++)
            {
                if (!string.IsNullOrEmpty(searchText) && _files[i].FilePath.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) < 0)
                {
                    continue;
                }

                if (ImGui.Selectable(_files[i].FilePath, i == _currentFile))
                {
                    _currentFile = i;

                    RemoveAndDispose(ref _contentView);

                    _contentView = AddDisposable(new ContentView(_files[i]));
                }

                if (ImGui.BeginPopupContextItem("context" + i))
                {
                    if (ImGui.Selectable("Export..."))
                    {
                        // TODO
                    }

                    ImGui.EndPopup();
                }
            }

            ImGui.EndChild();

            ImGui.SameLine();

            ImGui.BeginChild("content");

            _contentView?.Draw();

            ImGui.EndChild();

            ImGui.EndWindow();
        }

        private void ChangeInstallation(GameInstallation installation)
        {
            _selectedInstallation = installation;

            RemoveAndDispose(ref _contentView);
            _files = null;
            RemoveAndDispose(ref _fileSystem);

            if (installation == null)
            {
                _files = new List<FileSystemEntry>();
                return;
            }

            //var launcherImagePath = installation.Game.LauncherImagePath;
            //if (launcherImagePath != null)
            //{
            //    var fullImagePath = Path.Combine(installation.Path, launcherImagePath);
            //    _installationImageView.Image = new Bitmap(fullImagePath);
            //}
            //else
            //{
            //    _installationImageView.Image = null;
            //}

            _fileSystem = AddDisposable(installation.CreateFileSystem());

            _files = _fileSystem.Files.OrderBy(x => x.FilePath).ToList();

            //InstallationChanged?.Invoke(this, new InstallationChangedEventArgs(installation, _fileSystem));
        }
    }
}
