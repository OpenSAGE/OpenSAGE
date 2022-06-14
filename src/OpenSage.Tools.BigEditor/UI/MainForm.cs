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
                        wasOpenClicked = true;
                    }
                    if (ImGui.MenuItem("Close", null, false, _bigArchive != null))
                    {
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

                    if (ImGui.MenuItem("Import Directory", "Ctrl+Shift+I", false, _bigArchive != null))
                    {
                        wasImportDirectoryClicked = true;
                    }

                    if (ImGui.MenuItem("Import File...", "Ctrl+I", false, _bigArchive != null))
                    {
                        wasImportFileClicked = true;
                    }

                    ImGui.EndMenu();
                }

                ImGui.EndMenuBar();
            }

            if (wasOpenClicked)
            {
                var openDialog = new OpenFileDialog("Open .big file");
                openDialog.Open(result => OpenBigFile(result), "Big archive(*.big) | *.big");
            }
            if (wasExportAllClicked)
            {
                var dirDialog = new DirectoryDialog("Select folder for export files");
                dirDialog.Open(result => Export(result));
            }
            if (wasImportDirectoryClicked)
            {
                var dirDialog = new DirectoryDialog("Select folder for import files");
                dirDialog.Open(result => ImportDirectory(result));
            }
            if (wasImportFileClicked)
            {
                var openDialog = new OpenFileDialog("Select file which you want to import");
                openDialog.Open(result => ImportFile(result));
            }

            if (_bigArchive != null)
            {
                ImGui.BeginChild("body", new Vector2(0, -36), false, 0);

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

        private void OpenBigFile(DialogResult result)
        {
            if (!result.Success)
            {
                return;
            }

            Reset();

            if (File.Exists(result.FileName))
            {
                try
                {
                    _bigArchive = AddDisposable(new BigArchive(result.FileName, BigArchiveMode.Update));
                }
                catch (Exception e)
                {
                    logger.Error(e.Message);
                }
            }
        }

        private void DrawFilesList(Vector2 windowSize)
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

                    if (ImGui.Selectable("Delete"))
                    {
                        entry.Delete();
                        UpdateFilesList();
                        _currentFile = -1;
                        _currentFileName = "";
                        _currentView = null;
                    }

                    ImGui.EndPopup();
                }

                ImGui.NextColumn();

                ImGui.Text(ImGuiUtility.GetFormatedSize(entry.Length));
                ImGui.NextColumn();

                if (shouldOpenSaveDialog)
                {
                    var saveDialog = new SaveFileDialog("Export file");
                    saveDialog.DefaultFileName = entry.Name;
                    saveDialog.Save(result => ExportFile(entry, result));
                }
            }

            ImGui.Columns(1, null, false);

            ImGui.EndChild();

            ImGui.EndChild();
        }

        private void DrawStatusPanel()
        {
            ImGui.BeginChild("statusbar", new Vector2(0, 30), true, 0);

            ImGui.Text($"{Path.GetFileName(_bigArchive.FilePath)} | Version: {_bigArchive.Version} | Size: {ImGuiUtility.GetFormatedSize(_bigArchive.Size)} | Files: {_bigArchive.Entries.Count}");

            ImGui.SameLine();

            if (_currentFileName != null)
            {
                ImGui.Text($"| Selected file: {_currentFileName}");
            }

            ImGui.EndChild(); // end statusbar
        }

        private void Export(DialogResult result)
        {
            if (!result.Success)
            {
                return;
            }

            foreach (var entry in _files)
            {
                string filePath = Path.Combine(result.FileName, Path.GetDirectoryName(entry.FullName.Replace('\\', Path.DirectorySeparatorChar)));
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

        private void ExportFile(BigArchiveEntry entry, DialogResult result)
        {
            if (!result.Success)
            {
                return;
            }

            using (var entryStream = entry.Open())
            {
                using (var fileStream = File.OpenWrite(result.FileName))
                {
                    entryStream.CopyTo(fileStream);
                }
            }
        }

        private void ImportDirectory(DialogResult result)
        {
            if (!result.Success)
            {
                return;
            }

            foreach (var filePath in Directory.GetFiles(result.FileName))
            {
                using (var fileStream = File.OpenRead(filePath))
                {
                    var entry = _bigArchive.CreateEntry(filePath);

                    using (var entryStream = entry.Open())
                    {
                        fileStream.CopyTo(entryStream);
                    }
                }
            }

            UpdateFilesList();
        }

        private void ImportFile(DialogResult result)
        {
            if(!result.Success)
            {
                return;
            }

            using (var fileStream = File.OpenRead(result.FileName))
            {
                var entry = _bigArchive.CreateEntry(result.FileName);

                using (var entryStream = entry.Open())
                {
                    fileStream.CopyTo(entryStream);
                }
            }

            UpdateFilesList();
        }
    }
}
