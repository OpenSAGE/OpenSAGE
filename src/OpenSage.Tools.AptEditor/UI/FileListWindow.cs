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


namespace OpenSage.Tools.AptEditor.UI
{
    internal sealed class FileListWindow
    {

        public ref bool Visible => ref _open;
        public int MaxCount = 1000;

        private bool _open;
        private readonly GameWindow _window;
        private readonly AptFileSelector _aptFileSelector;
        private int _truncatedTo = 0;
        private string? _lastInput = null;

        private readonly HashSet<string> _autoDetect = new HashSet<string>();

        private CancellationTokenSource? _cancellation;
        private Task? _loadingTask;
        private readonly string _rootDirectory;

        private readonly ConcurrentQueue<string> _list = new ConcurrentQueue<string>();

        public FileListWindow(Game game, AptFileSelector aptFileSelector)
        {
            _aptFileSelector = aptFileSelector;
            _rootDirectory = game.ContentManager.FileSystem.RootDirectory;
            _window = game.Window;
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
            _list.Clear();

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

        public void Draw()
        {
            if (!Visible)
            {
                Reset(null);
                _list.Clear();

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
}
