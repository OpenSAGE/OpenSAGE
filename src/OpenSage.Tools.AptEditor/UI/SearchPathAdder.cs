using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ImGuiNET;
using OpenSage.Data;
using OpenSage.Mathematics;
using OpenSage.Tools.AptEditor.Util;

namespace OpenSage.Tools.AptEditor.UI
{
    internal sealed class SearchPathAdder
    {
        public FileSystem TargetFileSystem { get; set; }
        public ref bool Visible => ref _open;
        public int MaxCount { get; set; } = 1000;

        private readonly FileSuggestionBox _inputBox = new FileSuggestionBox { DirectoryOnly = true };
        private readonly ConcurrentQueue<string> _list = new ConcurrentQueue<string>();
        private readonly List<string> _mapped = new List<string>();
        private bool _open;
        private string? _lastInput = null;
        private string _mappedPath = string.Empty;
        private CancellationTokenSource? _cancellation;
        private Task? _loadingTask;

        public SearchPathAdder(Game game)
        {
            TargetFileSystem = game.ContentManager.FileSystem;
        }

        public void Draw()
        {
            if (!Visible)
            {
                Reset(null);
                return;
            }

            var draw = ImGui.Begin("Add Search paths...", ref Visible);
            if (draw)
            {
                ImGui.Columns(2);
                ImGui.Text("Load files from path: ");
                _inputBox.Draw();
                TryGetFiles();
                ImGui.Text("File list");
                if (_list.Count >= MaxCount)
                {
                    ImGui.SameLine();
                    ImGui.TextColored(ColorRgbaF.Red.ToVector4(), $"File count exceeded {MaxCount}!");
                }
                var list = _list.Take(MaxCount);
                foreach (var path in list)
                {
                    ImGui.TextUnformatted(path);
                }

                ImGui.NextColumn();
                ImGui.Text("Alias load path as: ");
                var lastMappedPath = _mappedPath;
                ImGui.InputText("##mapedPath", ref _mappedPath, 1024);
                ImGui.SameLine();
                var load = ImGui.Button("Load");
                ImGui.Text("Mapped files");
                if (lastMappedPath != _mappedPath)
                {
                    _mapped.Clear();
                }
                var newPaths = list.Skip(_mapped.Count);
                foreach (var path in newPaths)
                {
                    _mapped.Add(string.IsNullOrWhiteSpace(_mappedPath) ? path : Path.Combine(_mappedPath, path));
                }
                foreach (var path in _mapped)
                {
                    ImGui.TextUnformatted(path);
                }

                if (load)
                {
                    foreach (var (from, to) in list.Zip(_mapped))
                    {
                        var info = new FileInfo(Path.Combine(_inputBox.Value, from));
                        TargetFileSystem.Update(new FileSystemEntry(TargetFileSystem,
                                                                    FileSystem.NormalizeFilePath(to),
                                                                    (uint) info.Length,
                                                                    () => info.OpenRead()));
                    }
                    Visible = false;
                }
            }
            ImGui.End();
        }

        private void TryGetFiles()
        {
            var directory = Directory.Exists(_inputBox.Value)
                ? _inputBox.Value
                : Path.GetDirectoryName(_inputBox.Value);
            if (_lastInput == directory)
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
            var max = MaxCount;
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
                    _list.Enqueue(Path.GetRelativePath(directory, file));
                    if (_list.Count >= max) // sanity check
                    {
                        break;
                    }
                }
            }, token);
        }

        private void Reset(string? directory)
        {
            if (_lastInput == directory)
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
