using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using ImGuiNET;
using OpenSage.FileFormats.Big;
using OpenSage.Tools.BigEditor.Util;
using Veldrid.Sdl2;

namespace OpenSage.Tools.BigEditor.UI
{
    internal sealed class MainForm : DisposableBase
    {
        private Sdl2Window _window;
        private BigArchive _bigArchive;
        private FileBrowser _fileBrowser;
        private readonly List<BigArchiveEntry> _files;
        private int _currentFile;
        private string _currentFileName;
        private string _currentFileText;

        private byte[] _searchTextBuffer;
        private byte[] _filePathBuffer;
        private string _searchText;
        static float _scrollY;

        public MainForm()
        {
            _fileBrowser = new FileBrowser();
            _files = new List<BigArchiveEntry>();
            _currentFileName = "";
            _scrollY = 0.0f;
            OpenBigFile(null);
        }

        private void UpdateSearch(string searchText)
        {
            searchText = ImGuiUtility.TrimToNullByte(searchText);

            if (searchText == _searchText)
            {
                return;
            }

            _searchText = searchText;

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

        public void Draw(Sdl2Window window)
        {
            _window = window;

            ImGui.SetNextWindowPos(Vector2.Zero, ImGuiCond.Always, Vector2.Zero);
            ImGui.SetNextWindowSize(new Vector2(window.Width, window.Height), ImGuiCond.Always);

            ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 0);
            bool open = false;
            ImGui.Begin("OpenSAGE Big Editor", ref open, ImGuiWindowFlags.MenuBar | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoResize);

            bool wasOpenClicked = false;
            bool wasExportAllClicked = false;
            bool wasImportClicked = false;

            if (ImGui.BeginMenuBar())
            {
                if (ImGui.BeginMenu("File"))
                {
                    if (ImGui.MenuItem("Open...", "Ctrl+O", false, true))
                    {
                        wasOpenClicked = true;
                    }
                    if (ImGui.MenuItem("Close", null, false, _bigArchive != null))
                    {
                        OpenBigFile(null);
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

                    if (ImGui.MenuItem("Import", "Ctrl+I", false, _bigArchive != null))
                    {
                        wasImportClicked = true;
                    }

                    ImGui.EndMenu();
                }

                ImGui.EndMenuBar();
            }

            const string openFilePopupId = "Open Big Archive##OpenBigArchive";
            const string wasExportAllPopupId = "Export All Files##ExportAllFiles";
            const string wasImportPopupId = "Import Files##ImportFiles";

            if (wasOpenClicked)
            {
                ImGui.OpenPopup(openFilePopupId);
            }
            if (wasExportAllClicked)
            {
                ImGui.OpenPopup(wasExportAllPopupId);
            }
            if (wasImportClicked)
            {
                ImGui.OpenPopup(wasImportPopupId);
            }

            bool fileOpenPopupOpen = true;
            if (ImGui.BeginPopupModal(openFilePopupId, ref fileOpenPopupOpen, ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.HorizontalScrollbar))
            {
                ImGui.SetWindowSize(new Vector2(window.Width - 50, window.Height - 50), ImGuiCond.Always);
                ImGui.SetWindowPos(new Vector2(25, 25), ImGuiCond.Always);

                string path = _fileBrowser.Draw(FileBrowserType.Open);
                if (path != null && path.CompareTo("") != 0) {
                    path = ImGuiUtility.TrimToNullByte(path);

                    OpenBigFile(path);
                }

                ImGui.EndPopup();
            }
            fileOpenPopupOpen = true;
            if (ImGui.BeginPopupModal(wasExportAllPopupId, ref fileOpenPopupOpen, ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.HorizontalScrollbar))
            {
                ImGui.SetWindowSize(new Vector2(window.Width - 50, window.Height - 50), ImGuiCond.Always);
                ImGui.SetWindowPos(new Vector2(25, 25), ImGuiCond.Always);

                string path = _fileBrowser.Draw(FileBrowserType.ExportToDir);
                if (path != null && path.CompareTo("") != 0) {
                    path = ImGuiUtility.TrimToNullByte(path);

                    Console.WriteLine("path: {0}", path);

                    Export(path);
                }

                ImGui.EndPopup();
            }
            fileOpenPopupOpen = true;
            if (ImGui.BeginPopupModal(wasImportPopupId, ref fileOpenPopupOpen, ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.HorizontalScrollbar))
            {
                ImGui.SetWindowSize(new Vector2(window.Width - 50, window.Height - 50), ImGuiCond.Always);
                ImGui.SetWindowPos(new Vector2(25, 25), ImGuiCond.Always);

                string path = _fileBrowser.Draw(FileBrowserType.Import);
                if (path != null && path.CompareTo("") != 0) {
                    path = ImGuiUtility.TrimToNullByte(path);
                    Console.WriteLine("Import Dialog");
                    // OpenBigFile(path);
                }

                ImGui.EndPopup();
            }

            if (_bigArchive != null)
            {
                ImGui.BeginChild("body", new Vector2(0, -36), false, 0);

                DrawFilesList();

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

            if (_currentFileText != null)
            {
                ImGui.TextUnformatted(_currentFileText);

                if (ImGui.BeginPopupContextItem())
                {
                    if (ImGui.Selectable("Copy to clipboard"))
                    {
                        ImGui.SetClipboardText(_currentFileText);
                    }
                    ImGui.EndPopup();
                }
            }
            else
            {
                ImGui.Text("Select a text file to see its contents here.");
            }

            ImGui.EndChild();
        }

        private void OpenBigFile(string filePath)
        {
            RemoveAndDispose(ref _bigArchive);

            if (File.Exists(filePath))
            {
                try {
                    _bigArchive = AddDisposable(new BigArchive(filePath));
                } catch (Exception e) {
                    Console.WriteLine(e.Message);
                }
            }

            _currentFile = -1;
            _currentFileName = "";
            _currentFileText = null;
            _searchTextBuffer = new byte[32];
            _filePathBuffer = new byte[1024];
            _files.Clear();
            UpdateSearch(null);
        }

        private void DrawFilesList()
        {
            ImGui.BeginChild("sidebar", new Vector2(350, 0), true, 0);

            ImGui.PushItemWidth(-1);
            ImGuiUtility.InputText("##search", _searchTextBuffer, out var searchText);
            UpdateSearch(searchText);
            ImGui.PopItemWidth();

            ImGui.BeginChild("files list", Vector2.Zero, true);

            ImGui.Columns(2, "Files", false);

            ImGui.SetColumnWidth(0, 250);

            ImGui.Separator();
            ImGui.Text("Name"); ImGui.NextColumn();
            ImGui.Text("Size"); ImGui.NextColumn();
            ImGui.Separator();

            if (ImGui.IsKeyPressed(ImGui.GetKeyIndex(ImGuiKey.DownArrow)) && _currentFile != _files.Count-1)
            {
                _currentFile++;
                _scrollY += ImGui.GetIO().DeltaTime * 1000.0f;
                ImGui.SetScrollY(_scrollY);

            }
            if (ImGui.IsKeyPressed(ImGui.GetKeyIndex(ImGuiKey.UpArrow)) && _currentFile != 0)
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

                if (ImGui.Selectable(entry.FullName, i == _currentFile, ImGuiSelectableFlags.SpanAllColumns) || i == _currentFile)
                {
                    _currentFile = i;
                    _currentFileName = entry.FullName;

                    switch (Path.GetExtension(entry.FullName).ToLowerInvariant())
                    {
                        case ".ini":
                        case ".txt":
                        case ".wnd":
                            using (var stream = entry.Open())
                            using (var reader = new StreamReader(stream))
                            {
                                _currentFileText = reader.ReadToEnd();
                            }
                            break;

                        default:
                            _currentFileText = null;
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

                    ImGui.EndPopup();
                }

                ImGui.NextColumn();

                ImGui.Text(ImGuiUtility.GetFormatedSize(entry.Length));
                ImGui.NextColumn();

                var exportId = "Export##ExportDialog" + i;
                if (shouldOpenSaveDialog)
                {
                    ImGui.OpenPopup(exportId);
                }

                bool contextMenuOpen = true;
                if (ImGui.BeginPopupModal(exportId, ref contextMenuOpen, ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.HorizontalScrollbar))
                {
                    ImGui.SetWindowSize(new Vector2(_window.Width - 50, _window.Height - 50), ImGuiCond.Always);
                    ImGui.SetWindowPos(new Vector2(25, 25), ImGuiCond.Always);

                    string path = _fileBrowser.Draw(FileBrowserType.Save, entry.Name);
                    if (path != null && path.CompareTo("") != 0) {
                        path = ImGuiUtility.TrimToNullByte(path);

                        using (var entryStream = entry.Open())
                        {
                            Console.WriteLine($"{path}{Path.DirectorySeparatorChar}{entry.Name}");
                            using (var fileStream = File.OpenWrite($"{path}{Path.DirectorySeparatorChar}{entry.Name}"))
                            {
                                entryStream.CopyTo(fileStream);
                            }
                        }
                    }

                    ImGui.EndPopup();
                }
            }

            ImGui.Columns(1, null, false);

            ImGui.EndChild();

            ImGui.EndChild();
        }

        private void DrawStatusPanel()
        {
            ImGui.BeginChild("statusbar", new Vector2(0, 30), true, 0);

            ImGui.Text($"{_bigArchive.FilePath} | Version: {_bigArchive.Version} | Size: {ImGuiUtility.GetFormatedSize(_bigArchive.Size)} | Files: {_bigArchive.Entries.Count}");

            ImGui.SameLine();

            if (_currentFileName.CompareTo("") != 0)
            {
                ImGui.Text($"| Selected file: {_currentFileName}");
            }

            ImGui.EndChild(); // end statusbar
        }

        private void Export(string path)
        {
            foreach (var entry in _files)
            {
                string filePath = $"{path}{Path.DirectorySeparatorChar}{Path.GetDirectoryName(entry.FullName.Replace('\\', Path.DirectorySeparatorChar))}";
                try
                {
                    Directory.CreateDirectory(filePath);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    return;
                }

                try
                {
                    using (var entryStream = entry.Open())
                    {
                        using (var fileStream = File.OpenWrite($"{filePath}{Path.DirectorySeparatorChar}{entry.Name}"))
                        {
                            entryStream.CopyTo(fileStream);
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }
    }
}
