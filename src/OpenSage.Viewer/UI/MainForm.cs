using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using ImGuiNET;
using OpenSage.Data;
using OpenSage.Mods.BuiltIn;
using OpenSage.Viewer.Util;
using Veldrid;

namespace OpenSage.Viewer.UI
{
    internal sealed class MainForm : DisposableBase
    {
        private readonly GameWindow _gameWindow;
        private readonly ImGuiRenderer _imGuiRenderer;

        private readonly List<GameInstallation> _installations;

        private GameInstallation _selectedInstallation;
        private FileSystem _fileSystem;
        private Game _game;

        private List<FileSystemEntry> _files;
        private int _currentFile;

        private byte[] _searchTextBuffer = new byte[32];
        private byte[] _filePathBuffer = new byte[1024];

        private ContentView _contentView;

        public MainForm(GameWindow gameWindow, ImGuiRenderer imGuiRenderer)
        {
            _gameWindow = gameWindow;
            _imGuiRenderer = imGuiRenderer;

            _installations = GameInstallation.FindAll(GameDefinition.All).ToList();

            ChangeInstallation(_installations.FirstOrDefault());
        }

        public void Draw()
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

            ImGuiUtility.InputText("Search", _searchTextBuffer, out var searchText);

            for (var i = 0; i < _files.Count; i++)
            {
                var entry = _files[i];

                if (!string.IsNullOrEmpty(searchText) && entry.FilePath.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) < 0)
                {
                    continue;
                }

                if (ImGui.Selectable(entry.FilePath, i == _currentFile))
                {
                    _currentFile = i;

                    RemoveAndDispose(ref _contentView);

                    _contentView = AddDisposable(new ContentView(
                        _game.GraphicsDevice,
                        _imGuiRenderer,
                        _game,
                        entry));
                }

                var shouldOpenSaveDialog = false;

                if (ImGui.BeginPopupContextItem("context" + i))
                {
                    _currentFile = i;

                    if (ImGui.Selectable("Export..."))
                    {
                        //ImGui.CloseCurrentPopup();
                        shouldOpenSaveDialog = true;
                    }

                    ImGui.EndPopup();
                }

                var exportId = "Export##ExportDialog" + i;
                if (shouldOpenSaveDialog)
                {
                    ImGui.OpenPopup(exportId);
                }

                if (ImGui.BeginPopupModal(exportId, WindowFlags.AlwaysAutoResize))
                {
                    ImGuiUtility.InputText("File Path", _filePathBuffer, out var filePath);

                    if (ImGui.Button("Save"))
                    {
                        using (var entryStream = entry.Open())
                        {
                            using (var fileStream = File.OpenWrite(filePath))
                            {
                                entryStream.CopyTo(fileStream);
                            }
                        }

                        ImGui.CloseCurrentPopup();
                    }

                    ImGui.SetItemDefaultFocus();

                    ImGui.SameLine();

                    if (ImGui.Button("Cancel"))
                    {
                        ImGui.CloseCurrentPopup();
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
            RemoveAndDispose(ref _game);
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

            _game = AddDisposable(GameFactory.CreateGame(
                installation,
                _fileSystem,
                _gameWindow));

            //InstallationChanged?.Invoke(this, new InstallationChangedEventArgs(installation, _fileSystem));
        }
    }
}
