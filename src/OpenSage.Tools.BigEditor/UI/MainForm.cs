using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using ImGuiNET;
using OpenSage.FileFormats.Big;
using OpenSage.Tools.BigEditor.Util;
using Veldrid.Sdl2;
using SharpFileDialog;
using Veldrid;
using OpenSage.Tools.BigEditor.Views;

namespace OpenSage.Tools.BigEditor.UI
{
    internal sealed class MainForm : DisposableBase
    {
        private BigArchive _bigArchive;
        private readonly List<BigArchiveEntry> _files;
        private int _currentFile;
        private string _currentFileName;
        private View _currentView;
        private GraphicsDevice _graphicsDevice;
        private ImGuiRenderer _imguiRenderer;

        private byte[] _searchTextBuffer;
        private string _searchText;
        private float _scrollY;

        private BigArchiveMode? _openMode;

        private bool CanModify => _openMode == BigArchiveMode.Update || _openMode == BigArchiveMode.Create;

        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public MainForm(GraphicsDevice gd, ImGuiRenderer ren)
        {
            _files = new List<BigArchiveEntry>();
            _imguiRenderer = ren;
            _currentFileName = null;
            _scrollY = 0.0f;
            _graphicsDevice = gd;
            Reset();
        }

        private void UpdateFilesList()
        {
            _files.Clear();

            if (_bigArchive == null)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(_searchText))
            {
                _files.AddRange(_bigArchive.Entries.OrderBy(x => x.FullName));
            }
            else
            {
                _files.AddRange(_bigArchive.Entries
                    .Where(entry => entry.FullName.IndexOf(_searchText, StringComparison.OrdinalIgnoreCase) >= 0)
                    .OrderBy(x => x.FullName));
            }
        }

        private void UpdateSearch(string searchText)
        {
            searchText = ImGuiUtility.TrimToNullByte(searchText);

            if (searchText == _searchText)
            {
                return;
            }

            _searchText = searchText;
            UpdateFilesList();
        }

        private void Reset()
        {
            RemoveAndDispose(ref _bigArchive);
            _currentFile = -1;
            _currentFileName = "";
            _currentView = null;
            _searchTextBuffer = new byte[32];
            _files.Clear();
            UpdateSearch(null);
        }

        public void Draw(Sdl2Window window)
        {
            ImGui.SetNextWindowPos(Vector2.Zero, ImGuiCond.Always, Vector2.Zero);
            ImGui.SetNextWindowSize(new Vector2(window.Width, window.Height), ImGuiCond.Always);

            ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 0);
            bool open = false;
            ImGui.Begin("OpenSAGE Big Editor", ref open, ImGuiWindowFlags.MenuBar | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoResize);

            var wasOpenClicked = false;
            var wasExportAllClicked = false;
            var wasImportDirectoryClicked = false;
            var wasImportFileClicked = false;

            if (ImGui.BeginMenuBar())
            {
                if (ImGui.BeginMenu("File"))
                {
                    if (ImGui.MenuItem("Open...", "Ctrl+O", false, true))
                    {
                        _openMode = BigArchiveMode.Update;
                        wasOpenClicked = true;
                    }

                    if (ImGui.MenuItem("Open (read only)...")) {
                        _openMode = BigArchiveMode.Read;
                        wasOpenClicked = true;
                    }

                    if (ImGui.MenuItem("Close", null, false, _bigArchive != null))
                    {
                        _openMode = null;
                        Reset();
                    }

                    ImGui.Separator();

                    if (ImGui.MenuItem("Exit", "Alt+F4", false, true))
                    {
                        Environment.Exit(0);
                    }

                    ImGui.EndMenu();
                }
                if (ImGui.BeginMenu("Edit"))
                {
                    if (ImGui.MenuItem("Export All", "Ctrl+E", false, _bigArchive != null))
                    {
                        wasExportAllClicked = true;
                    }

                    ImGui.Separator();

                    if (ImGui.MenuItem("Import Directory", "Ctrl+Shift+I", false, _bigArchive != null && CanModify))
                    {
                        wasImportDirectoryClicked = true;
                    }

                    if (ImGui.MenuItem("Import File...", "Ctrl+I", false, _bigArchive != null && CanModify))
                    {
                        wasImportFileClicked = true;
                    }

                    ImGui.EndMenu();
                }

                ImGui.EndMenuBar();
            }

            if (wasOpenClicked)
            {
                // "Open .big file"
                var bigFilter = new NativeFileDialog.Filter { Name = "Big archive", Extensions = ["big"], };
                if (NativeFileDialog.OpenDialog([bigFilter], null, out var result))
                {
                    OpenBigFile(result);
                }
            }
            if (wasExportAllClicked)
            {
                // "Select folder for export files"
                if (NativeFileDialog.PickFolder(null, out var result))
                {
                    Export(result);
                }
            }
            if (wasImportDirectoryClicked)
            {
                // "Select folder for import files"
                if (NativeFileDialog.PickFolder(null, out var result))
                {
                    ImportDirectory(result);
                }
            }
            if (wasImportFileClicked)
            {
                // "Select file which you want to import"
                if (NativeFileDialog.OpenDialog(null, null, out var result))
                {
                    ImportFile(result);
                }
            }

            if (_bigArchive != null)
            {
                ImGui.BeginChild("body", new Vector2(0, -36));

                DrawFilesList(new Vector2(window.Width, window.Height));

                ImGui.SameLine();

                DrawFileContent();

                ImGui.EndChild(); // end body

                DrawStatusPanel();
            }
            else
            {
                ImGui.Text("Open a .big file to see its contents here.");
            }

            ImGui.End();
            ImGui.PopStyleVar();
        }

        private void DrawFileContent()
        {
            ImGui.BeginChild("content");

            if (_currentView != null)
            {
                _currentView.Draw();
            }
            else
            {
                ImGui.Text("Select a text file to see its contents here.");
            }

            ImGui.EndChild();
        }

        private void OpenBigFile(string path)
        {
            Reset();

            if (File.Exists(path))
            {
                try
                {
                    _bigArchive = AddDisposable(new BigArchive(path, _openMode.Value));
                }
                catch (Exception e)
                {
                    logger.Error(e.Message);
                }
            }
        }

        private void DrawFilesList(Vector2 windowSize)
        {
            ImGui.BeginChild("sidebar", new Vector2(350, 0), ImGuiChildFlags.Border);

            ImGui.PushItemWidth(-1);
            ImGuiUtility.InputText("##search", _searchTextBuffer, out var searchText);
            UpdateSearch(searchText);
            ImGui.PopItemWidth();

            ImGui.BeginChild("files list", Vector2.Zero, ImGuiChildFlags.Border);

            ImGui.Columns(2, "Files", false);

            ImGui.SetColumnWidth(0, 250);

            ImGui.Separator();
            ImGui.Text("Name"); ImGui.NextColumn();
            ImGui.Text("Size"); ImGui.NextColumn();
            ImGui.Separator();

            if (ImGui.IsKeyPressed(ImGuiKey.DownArrow) && _currentFile != _files.Count - 1)
            {
                _currentFile++;
                _scrollY += ImGui.GetIO().DeltaTime * 1000.0f;
                ImGui.SetScrollY(_scrollY);

            }
            if (ImGui.IsKeyPressed(ImGuiKey.UpArrow) && _currentFile != 0)
            {
                _currentFile--;
                _scrollY -= ImGui.GetIO().DeltaTime * 1000.0f;
                ImGui.SetScrollY(_scrollY);
            }
            if (ImGui.IsMouseClicked(0))
            {
                _scrollY = ImGui.GetScrollY();
            }

            for (var i = 0; i < _files.Count; i++)
            {
                var entry = _files[i];

                if (ImGui.Selectable(entry.FullName, i == _currentFile, ImGuiSelectableFlags.SpanAllColumns))
                {
                    if (_currentFile == i)
                        continue;

                    _currentFile = i;
                    _currentFileName = entry.FullName;

                    switch (Path.GetExtension(entry.FullName).ToLowerInvariant())
                    {
                        case ".ini":
                        case ".txt":
                        case ".wnd":
                        case ".xml":
                        case ".lua":
                        case ".inc":
                            using (var stream = entry.Open())
                            {
                                _currentView = AddDisposable(new TextView(stream));
                            }
                            break;
                        case ".tga":
                        case ".bmp":
                        case ".jpg":
                        case ".png":
                            using (var stream = entry.Open())
                            {
                                _currentView = AddDisposable(new ImageView(stream, _graphicsDevice, _imguiRenderer));
                            }
                            break;
                        default:
                            _currentView = null;
                            break;
                    }
                }

                var shouldOpenSaveDialog = false;

                if (ImGui.BeginPopupContextItem("context" + i))
                {
                    _currentFile = i;
                    _currentFileName = entry.FullName;

                    if (ImGui.Selectable("Export..."))
                    {
                        shouldOpenSaveDialog = true;
                    }

                    if (CanModify)
                    {
                        if (ImGui.Selectable("Delete", false))
                        {
                            entry.Delete();
                            UpdateFilesList();
                            _currentFile = -1;
                            _currentFileName = "";
                            _currentView = null;
                        }
                    }

                    ImGui.EndPopup();
                }

                ImGui.NextColumn();

                ImGui.Text(ImGuiUtility.GetFormatedSize(entry.Length));
                ImGui.NextColumn();

                if (shouldOpenSaveDialog)
                {
                    // "Export file"
                    if (NativeFileDialog.SaveDialog(null, null, out var result))
                    {
                        // saveDialog.DefaultFileName = entry.Name;
                        ExportFile(entry, result);
                    }
                }
            }

            ImGui.Columns(1, null, false);

            ImGui.EndChild();

            ImGui.EndChild();
        }

        private void DrawStatusPanel()
        {
            ImGui.BeginChild("statusbar", new Vector2(0, 30), ImGuiChildFlags.Border);

            ImGui.Text($"{Path.GetFileName(_bigArchive.FilePath)} | Version: {_bigArchive.Version} | Size: {ImGuiUtility.GetFormatedSize(_bigArchive.Size)} | Files: {_bigArchive.Entries.Count}");

            ImGui.SameLine();

            if (_currentFileName != null)
            {
                ImGui.Text($"| Selected file: {_currentFileName}");
            }

            ImGui.EndChild(); // end statusbar
        }

        private void Export(string path)
        {
            foreach (var entry in _files)
            {
                string filePath = Path.Combine(path, Path.GetDirectoryName(entry.FullName.Replace('\\', Path.DirectorySeparatorChar)));
                try
                {
                    Directory.CreateDirectory(filePath);
                }
                catch (Exception e)
                {
                    logger.Error(e.Message);
                    return;
                }

                try
                {
                    using (var entryStream = entry.Open())
                    {
                        using (var fileStream = File.OpenWrite(Path.Combine(filePath, entry.Name)))
                        {
                            entryStream.CopyTo(fileStream);
                        }
                    }
                }
                catch (Exception e)
                {
                    logger.Error(e.Message);
                }
            }
        }

        private static void ExportFile(BigArchiveEntry entry, string path)
        {
            using var entryStream = entry.Open();
            using var fileStream = File.OpenWrite(path);
            entryStream.CopyTo(fileStream);
        }

        private void ImportDirectory(string path)
        {
            foreach (var filePath in Directory.GetFiles(path))
            {
                using var fileStream = File.OpenRead(filePath);
                var entry = _bigArchive.CreateEntry(filePath);

                using var entryStream = entry.Open();
                fileStream.CopyTo(entryStream);
            }

            UpdateFilesList();
        }

        private void ImportFile(string path)
        {
            using (var fileStream = File.OpenRead(path))
            {
                var entry = _bigArchive.CreateEntry(path);

                using (var entryStream = entry.Open())
                {
                    fileStream.CopyTo(entryStream);
                }
            }

            UpdateFilesList();
        }
    }
}
