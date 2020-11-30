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

namespace OpenSage.Tools.AptEditor.UI
{
    internal sealed class FileListWindow
    {
        public FileSystem TargetFileSystem { get; set; }
        public ref bool Visible => ref _open;
        public ref int MaxCount => ref _maxCount;
        public string MappedPath { get; set; } = string.Empty;
        public Action? Next { get; set; }

        //private readonly FileSuggestionBox _inputBox = new FileSuggestionBox { DirectoryOnly = true };
        private readonly HashSet<string> _autoDetect = new HashSet<string>();
        private readonly ConcurrentQueue<string> _list = new ConcurrentQueue<string>();
        private readonly List<string> _mapped = new List<string>();
        private readonly GameWindow _window;
        private readonly string _rootDirectory;
        private readonly AptFileSelector _aptFileSelector;

        private bool _open;
        private int _maxCount = 1000;
        private int _truncatedTo = 0;
        private string? _lastInput = null;
        private CancellationTokenSource? _cancellation;
        private Task? _loadingTask;

        public FileListWindow(Game game, AptFileSelector aptFileSelector)
        {
            TargetFileSystem = game.ContentManager.FileSystem;
            _window = game.Window;
            _aptFileSelector = aptFileSelector;
            _rootDirectory = TargetFileSystem.RootDirectory;
        }

        public void Draw()
        {
            if (!Visible)
            {
                Reset(null);
                return;
            }
            ImGui.SetNextWindowPos(new Vector2(_window.ClientBounds.Width *2 / 3, 45), ImGuiCond.Always, Vector2.Zero);
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
                    if(ImGui.Button(wasVisible ? path : string.Empty))
                    {
                        _aptFileSelector.SetValue(path);
                    }
                    
                    wasVisible = ImGui.IsItemVisible();
                }

            }
            ImGui.End();
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

        private void AutoLoadImpl(string sourcePath,
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
            Load(filtered.ToArray(), !isFromGameFileSystem, loadArtOnly);
        }

        private void Load(IEnumerable<(string, string)> listAndMapped,
                          bool isPhysicalFile,
                          bool loadArtOnly)
        {
            var art = FileSystem.NormalizeFilePath("art/textures/");
            foreach (var (from, to) in listAndMapped)
            {
                var (open, length) = GetFile(from, isPhysicalFile);
                if (!loadArtOnly)
                {
                    TargetFileSystem.Update(new FileSystemEntry(TargetFileSystem,
                                                                FileSystem.NormalizeFilePath(to),
                                                                length,
                                                                open));
                }

                // load art
                var normalizedArt = FileSystem.NormalizeFilePath(from);
                var index = normalizedArt.IndexOf(art);
                if (index == -1)
                {
                    continue;
                }
                TargetFileSystem.Update(new FileSystemEntry(TargetFileSystem,
                                                            normalizedArt[index..],
                                                            length,
                                                            open));
            }
        }

        private (Func<Stream>, uint) GetFile(string path, bool isPhysicalFile)
        {
            if (isPhysicalFile)
            {
                var info = new FileInfo(path);
                return (info.OpenRead, (uint) info.Length);
            }
            var entry = TargetFileSystem.GetFile(path);
            return (entry.Open, entry.Length);
        }

        private void TryGetFiles()
        {
            var directory = Directory.Exists(_rootDirectory)
                ? _rootDirectory
                : Path.GetDirectoryName(_rootDirectory);
            if (_lastInput == directory && _truncatedTo == MaxCount)
            {
                return;
            }
            Reset(directory);

            if (string.IsNullOrWhiteSpace(directory) || !Directory.Exists(directory))
            {
                return;
            }

            _cancellation = new CancellationTokenSource();
            var token = _cancellation.Token;
            var max = _truncatedTo = MaxCount;
            _loadingTask = Task.Run(() =>
            {
                var files = Directory.EnumerateFiles(directory, "*", new EnumerationOptions
                {
                    RecurseSubdirectories = true,
                    ReturnSpecialDirectories = false
                });

                token.ThrowIfCancellationRequested();
                foreach (var file in files)
                {
                    token.ThrowIfCancellationRequested();
                    if (file.EndsWith(".apt"))
                    {
                        _list.Enqueue(Path.GetRelativePath(directory, file));
                        if (_list.Count >= max) // sanity check
                        {
                            break;
                        }
                    }
                }
            }, token);
        }

        private void Reset(string? directory)
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
