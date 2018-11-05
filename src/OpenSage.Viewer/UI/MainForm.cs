using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        private int _currentFile = -1;

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

        private void DrawMainUi(ref bool isGameViewFocused)
        {
            if (ImGui.BeginMenuBar())
            {
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
                ImGui.EndMenuBar();
            }

            ImGui.BeginChild("sidebar", new Vector2(250, 0), true, 0);

            if (_launcherImage != null)
            {
                var availableSize = ImGui.GetContentRegionAvail();

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

            ImGui.BeginChild("files list", Vector2.Zero, true);

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
                        new Views.AssetViewContext(_game, _gamePanel, _imGuiRenderer, entry)));
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

                bool open = false;

                if (ImGui.BeginPopupModal(exportId, ref open, ImGuiWindowFlags.AlwaysAutoResize))
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

                ImGui.Text(_contentView.DisplayName);

                ImGui.BeginChild("content view");

                _contentView.Draw(ref isGameViewFocused);

                ImGui.EndChild();
                ImGui.EndChild();
            }
        }

        private void DrawNoGamesUi()
        {
            ImGui.BeginChild("content", Vector2.Zero, false, ImGuiWindowFlags.AlwaysAutoResize);
            ImGui.Text("OpenSAGE was unable to find any game installations.\n");

            ImGui.Spacing();

            ImGui.Text("You can manually specify installation paths by setting any of the following environment variables:");

            foreach (var gameDefinition in GameDefinition.All)
            {
                ImGui.Text($"  {gameDefinition.Identifier.ToUpper()}_PATH=<installation path>");
            }

            ImGui.Spacing();

            ImGui.Text("OpenSAGE doesn't yet detect every released version of every game. Please report undetected versions to ");
            ImGui.SameLine();
            if (ImGui.SmallButton("our GitHub page."))
            {
                Process.Start(new ProcessStartInfo("https://github.com/OpenSAGE/OpenSAGE/issues")
                {
                    UseShellExecute = true
                });
            }
            ImGui.EndChild();
        }

        public void Draw(ref bool isGameViewFocused)
        {
            ImGui.SetNextWindowPos(Vector2.Zero, ImGuiCond.Always, Vector2.Zero);
            ImGui.SetNextWindowSize(new Vector2(_gameWindow.ClientBounds.Width, _gameWindow.ClientBounds.Height), ImGuiCond.Always);

            ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 0);
            bool open = false;
            ImGui.Begin("OpenSAGE Viewer", ref open, ImGuiWindowFlags.MenuBar | ImGuiWindowFlags.NoTitleBar);

            if (_gamePanel != null)
            {
                _gamePanel.IsGameViewActive = isGameViewFocused;
                DrawMainUi(ref isGameViewFocused);
            }
            else
            {
                DrawNoGamesUi();
            }

            ImGui.End();
            ImGui.PopStyleVar();
        }

        private void ChangeInstallation(GameInstallation installation)
        {
            _selectedInstallation = installation;

            _imGuiRenderer.ClearCachedImageResources();

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
                if (File.Exists(fullImagePath))
                {
                    _launcherImage = AddDisposable(new ImageSharpTexture(fullImagePath).CreateDeviceTexture(
                        _gameWindow.GraphicsDevice, _gameWindow.GraphicsDevice.ResourceFactory));
                }
            }

            _fileSystem = AddDisposable(installation.CreateFileSystem());

            _files = _fileSystem.Files.OrderBy(x => x.FilePath).ToList();

            _currentFile = -1;

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
