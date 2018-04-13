using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using ImGuiNET;
using OpenSage.Data;
using OpenSage.Mathematics;
using OpenSage.Mods.BuiltIn;
using OpenSage.Viewer.Framework;
using OpenSage.Viewer.Util;
using Veldrid;
using Veldrid.ImageSharp;

namespace OpenSage.Viewer.UI
{
    internal sealed class MainForm : DisposableBase
    {
        private readonly GameWindow _gameWindow;
        private readonly ImGuiRenderer _imGuiRenderer;

        private readonly List<GameInstallation> _installations;

        private GameInstallation _selectedInstallation;
        private Texture _launcherImage;
        private FileSystem _fileSystem;
        private ImGuiGamePanel _gamePanel;
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

        public void Draw(ref bool isGameViewFocused)
        {
            _gamePanel.IsGameViewActive = isGameViewFocused;

            ImGui.SetNextWindowPos(Vector2.Zero, Condition.Always, Vector2.Zero);
            ImGui.SetNextWindowSize(new Vector2(_gameWindow.ClientBounds.Width, _gameWindow.ClientBounds.Height), Condition.Always);

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

            if (_launcherImage != null)
            {
                var availableSize = ImGui.GetContentRegionAvailable();

                var launcherImageSize = SizeF.CalculateSizeFittingAspectRatio(
                    new SizeF(_launcherImage.Width, _launcherImage.Height),
                    new Size((int) availableSize.X, (int) availableSize.Y));

                ImGui.Image(
                    _imGuiRenderer.GetOrCreateImGuiBinding(_gameWindow.GraphicsDevice.ResourceFactory, _launcherImage),
                    new Vector2(launcherImageSize.Width, launcherImageSize.Height),
                    Vector2.Zero,
                    Vector2.One,
                    Vector4.One,
                    Vector4.Zero);
            }

            ImGui.PushItemWidth(-1);
            ImGuiUtility.InputText("##search", _searchTextBuffer, out var searchText);
            ImGui.PopItemWidth();

            ImGui.BeginChild("files list", true, 0);

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

                    _game.ContentManager.Unload();

                    _contentView = AddDisposable(new ContentView(
                        new Views.AssetViewContext(_game, _imGuiRenderer, entry)));
                }

                var shouldOpenSaveDialog = false;

                if (ImGui.BeginPopupContextItem("context" + i))
                {
                    _currentFile = i;

                    if (ImGui.Selectable("Export..."))
                    {
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
            ImGui.EndChild();

            ImGui.SameLine();

            if (_contentView != null)
            {
                ImGui.BeginChild("content");

                ImGui.Text(_contentView.Entry.FilePath);

                ImGui.BeginChild("content view");

                var windowPos = ImGui.GetWindowPosition();
                var availableSize = ImGui.GetContentRegionAvailable();
                _gamePanel.EnsureFrame(
                    new Mathematics.Rectangle(
                        (int) windowPos.X,
                        (int) windowPos.Y,
                        (int) availableSize.X,
                        (int) availableSize.Y));

                _contentView.Draw(ref isGameViewFocused);

                ImGui.EndChild();
                ImGui.EndChild();
            }

            ImGui.EndWindow();
        }

        private void ChangeInstallation(GameInstallation installation)
        {
            _selectedInstallation = installation;

            RemoveAndDispose(ref _contentView);
            _files = null;
            RemoveAndDispose(ref _game);
            RemoveAndDispose(ref _gamePanel);
            RemoveAndDispose(ref _fileSystem);
            RemoveAndDispose(ref _launcherImage);

            if (installation == null)
            {
                _files = new List<FileSystemEntry>();
                return;
            }

            var launcherImagePath = installation.Game.LauncherImagePath;
            if (launcherImagePath != null)
            {
                var fullImagePath = Path.Combine(installation.Path, launcherImagePath);
                _launcherImage = AddDisposable(new ImageSharpTexture(fullImagePath).CreateDeviceTexture(
                    _gameWindow.GraphicsDevice, _gameWindow.GraphicsDevice.ResourceFactory));
            }

            _fileSystem = AddDisposable(installation.CreateFileSystem());

            _files = _fileSystem.Files.OrderBy(x => x.FilePath).ToList();

            _gamePanel = AddDisposable(new ImGuiGamePanel(_gameWindow));
            _gamePanel.EnsureFrame(new Mathematics.Rectangle(0, 0, 100, 100));

            _game = AddDisposable(GameFactory.CreateGame(
                installation,
                _fileSystem,
                _gamePanel));

            //InstallationChanged?.Invoke(this, new InstallationChangedEventArgs(installation, _fileSystem));
        }
    }
}
