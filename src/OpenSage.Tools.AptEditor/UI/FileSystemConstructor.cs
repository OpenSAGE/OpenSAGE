using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using ImGuiNET;
using OpenSage.Data;
using OpenSage.Mathematics;
using OpenSage.Tools.AptEditor.Util;
using OpenSage.Tools.AptEditor.Apt;
using OpenSage.FileFormats.Apt;
using System.Diagnostics.CodeAnalysis;

namespace OpenSage.Tools.AptEditor.UI
{
    internal sealed class FileListWindow: FileSystemConstructor
    {

        public ref bool Visible => ref _open;
        private bool _open;
        private readonly GameWindow _window;
        private readonly AptFileSelector _aptFileSelector;

        public FileListWindow(Game game, AptFileSelector aptFileSelector) : base(game)
        {
            _aptFileSelector = aptFileSelector;

            _window = game.Window;
        }

        public void Draw()
        {
            if (!Visible)
            {
                Reset(null);
                return;
            }
            ImGui.SetNextWindowPos(new Vector2(_window.ClientBounds.Width * 2 / 3, 45), ImGuiCond.Always, Vector2.Zero);
            ImGui.SetNextWindowSize(new Vector2(_window.ClientBounds.Width / 3, 0), ImGuiCond.Always);

            var draw = ImGui.Begin("File List", ref Visible);
            if (draw)
            {
                TryGetFiles();

                if (_list.Count >= _truncatedTo && _list.Any())
                {
                    ImGui.SameLine();
                    var message = $"File count exceeded {_truncatedTo}! Only first {_truncatedTo} files will be loaded";
                    ImGui.TextColored(ColorRgbaF.Red.ToVector4(), message);
                }

                if (_loadingTask?.IsCompleted != false && _list.Any())
                {
                    if (ImGui.Button("Enable Autoload in following files"))
                    {
                        // _inputBox.Value is valid, becasue TryGetFiles() is called after _inputBox.Draw()s
                        _autoDetect.UnionWith(_list.Select(path => Path.Combine(_rootDirectory, path)));
                    }
                    if (_autoDetect.Any())
                    {
                        ImGui.SameLine();
                    }
                }
                if (_autoDetect.Any())
                {
                    if (ImGui.Button($"Clear {_autoDetect.Count} items in autoload list"))
                    {
                        _autoDetect.Clear();
                    }
                }

                ImGui.Text("Apt Files");
                ImGui.Separator();

                var beginY = ImGui.GetCursorPosY();
                var list = _list.Take(MaxCount);
                var wasVisible = true;
                foreach (var path in list)
                {
                    if (ImGui.Button(wasVisible ? path : string.Empty))
                    {
                        _aptFileSelector.SetValue(path);
                    }

                    wasVisible = ImGui.IsItemVisible();
                }

            }
            ImGui.End();
        }
    }

    internal class FileSystemConstructor
    {
        public FileSystem TargetFileSystem { get; set; }

        public ref int MaxCount => ref _maxCount;
        public string MappedPath { get; set; } = string.Empty;
        public Action? Next { get; set; }

        //private readonly FileSuggestionBox _inputBox = new FileSuggestionBox { DirectoryOnly = true };
        protected readonly HashSet<string> _autoDetect = new HashSet<string>();
        protected readonly ConcurrentQueue<string> _list = new ConcurrentQueue<string>();
        protected readonly List<string> _mapped = new List<string>();
        protected readonly string _rootDirectory;

        protected int _maxCount = 1000;
        protected int _truncatedTo = 0;
        protected string? _lastInput = null;
        protected CancellationTokenSource? _cancellation;
        protected Task? _loadingTask;

        public FileSystemConstructor(Game game)
        {
            TargetFileSystem = game.ContentManager.FileSystem;
            _rootDirectory = TargetFileSystem.RootDirectory;
        }

        public void CheckImportTree(AptFile apt)
        {
            foreach (var import in apt.Movie.Imports)
            {
                var importPath = Path.Combine(apt.RootDirectory, Path.ChangeExtension(import.Movie, ".apt"));
                var importEntry = TargetFileSystem.GetFile(importPath);
                if (importEntry == null)
                    throw new FileNotFoundException("Cannot find imported file", importPath);

                // Some dirty tricks to avoid the above exception
                var getter = new StandardStreamGetter(importPath, (s, m) => TargetFileSystem.GetFile(s).Open());
                var f = AptFile.Parse(getter);
                CheckImportTree(f);
            }
        }

        public AptFile LoadApt(string path)
        {
            var entry = TargetFileSystem.GetFile(path);
            if (entry == null)
            {
                throw new FileNotFoundException("Cannot find file", path);
            }

            AutoLoad(path, loadArtOnly: true); // here it's used to prepare art folder
            var aptFile = AptFileHelper.FromFileSystemEntry(entry);

            return aptFile;
        }

        [SuppressMessage("Microsoft.Performance", "CA2200", MessageId = "intentional")]
        public void LoadImportTree(AptFile apt)
        {
            string? lastFailed = null;
            while (true)
            {
                try
                {
                    CheckImportTree(apt);
                }
                catch (FileNotFoundException loadFailure)
                {
                    if (loadFailure.FileName is string file)
                    {
                        if (file != lastFailed)
                        {
                            lastFailed = file;
                            if (AutoLoad(file, loadArtOnly: false))
                            {
                                continue;
                            }
                        }
                    }
                    throw loadFailure;
                }
                break;
            }
        }

        public bool AutoLoad(string requiredAptFile, bool loadArtOnly)
        {
            requiredAptFile = FileSystem.NormalizeFilePath(requiredAptFile);
            var mappedPath = Path.GetDirectoryName(requiredAptFile);
            var name = Path.GetFileName(requiredAptFile);

            var detectedFromGameFileSystem = TargetFileSystem
                .FindFiles(entry => Path.GetFileName(entry.FilePath) == name)
                .Select(entry => entry.FilePath)
                .ToArray();
            var detectedFromCustomFiles = _autoDetect
                .Where(path => FileSystem.NormalizeFilePath(Path.GetFileName(path)) == name);
            if (!detectedFromGameFileSystem.Any() && !detectedFromCustomFiles.Any())
            {
                return false;
            }

            void Trace(string sourcePath, string from)
            {
                var text = $"Automatically loaded game:{sourcePath}  => game:{mappedPath} for {requiredAptFile}";
                Console.WriteLine(text + (loadArtOnly ? " (art)" : string.Empty));
            }

            foreach (var file in detectedFromCustomFiles)
            {
                var sourcePath = Path.GetDirectoryName(file);
                AutoLoadImpl(sourcePath!, mappedPath, isFromGameFileSystem: false, loadArtOnly);
                Trace(sourcePath!, "file");
            }

            foreach (var gameFile in detectedFromGameFileSystem)
            {
                var sourcePath = Path.GetDirectoryName(gameFile);
                AutoLoadImpl(sourcePath!, mappedPath, isFromGameFileSystem: true, loadArtOnly);
                Trace(sourcePath!, "game");
            }

            return true;
        }

        public void AutoLoadImpl(string sourcePath,
                                  string? mappedPath,
                                  bool isFromGameFileSystem,
                                  bool loadArtOnly)
        {
            if (!Path.EndsInDirectorySeparator(sourcePath))
            {
                sourcePath += Path.DirectorySeparatorChar;
            }
            var paths = isFromGameFileSystem
                ? TargetFileSystem.FindFiles(_ => true).Select(entry => entry.FilePath)
                : _autoDetect;
            var normalizedSourcePath = FileSystem.NormalizeFilePath(sourcePath);
            var filtered = from path in paths
                           let normalized = FileSystem.NormalizeFilePath(path)
                           where normalized.StartsWith(normalizedSourcePath)
                           let relative = normalized[(normalizedSourcePath.Length)..]
                           let mapped = mappedPath is null
                               ? relative
                               : Path.Combine(mappedPath, relative)
                           select (path, mapped);
            TargetFileSystem.LoadFiles(filtered.ToArray(), !isFromGameFileSystem, loadArtOnly);
        }


        public void TryGetFiles()
        {
            var directory = Directory.Exists(_rootDirectory)
                ? _rootDirectory
                : Path.GetDirectoryName(_rootDirectory);
            if (_lastInput == directory && _truncatedTo == MaxCount)
            {
                return;
            }
            Reset(directory);

            var max = _truncatedTo = MaxCount;

            _cancellation = new CancellationTokenSource();
            var token = _cancellation.Token;
            
            _loadingTask = Task.Run(() =>
            {
                void cancel() { token.ThrowIfCancellationRequested(); }
                bool filter(string file) { return file.EndsWith(".apt"); }

                var files = FileUtilities.GetFilesByDirectory(directory, max, filter, cancel);
                foreach (var file in files)
                    _list.Enqueue(file);
            }, token);
        }

        public void Reset(string? directory)
        {
            if (_lastInput == directory && _truncatedTo == MaxCount)
            {
                return;
            }
            _lastInput = directory;
            _list.Clear();
            _mapped.Clear();

            _cancellation?.Cancel();
            _cancellation?.Dispose();
            _cancellation = null;

            try
            {
                _loadingTask?.Wait();
            }
            catch (OperationCanceledException) { }
            catch (AggregateException e) when (e.InnerExceptions.FirstOrDefault() is TaskCanceledException) { }
            _loadingTask?.Dispose();
            _loadingTask = null;
        }
    }
}
