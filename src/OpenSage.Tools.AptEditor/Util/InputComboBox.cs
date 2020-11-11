using System;
using System.Collections.Generic;
using System.Numerics;
using System.Linq;
using ImGuiNET;
using System.IO;

namespace OpenSage.Tools.AptEditor.Util
{
    internal abstract class InputComboBox
    {
        public string ID { get; set; } = Guid.NewGuid().ToString();
        public string PopUpID => $"##{ID}.Suggestions";
        public string Value => _current;

        private string[] _cached = Array.Empty<string>();
        private string _last = string.Empty;
        private string _current = string.Empty;
        private bool _listing;

        protected abstract string[] GetSuggestions();

        public void Draw()
        {
            ImGui.InputText($"##{ID}", ref _current, 1024);

            _listing = _listing || ImGui.IsItemActive();
            if (_listing)
            {
                var isFocused = ImGui.IsWindowFocused();
                var inputSize = ImGui.GetItemRectSize();
                var popUpPosition = new Vector2(ImGui.GetItemRectMin().X, ImGui.GetItemRectMax().Y);
                var popUpSize = new Vector2(inputSize.X, 0);
                DrawSuggestions(isFocused, popUpPosition, popUpSize);
            }

            _last = _current;
        }

        /// Inspired by Harold Brenes' auto complete widget https://github.com/ocornut/imgui/issues/718
        private void DrawSuggestions(bool isFocused, Vector2 position, Vector2 size)
        {
            if (_last != _current)
            {
                _cached = GetSuggestions();
            }

            if (!_cached.Any())
            {
                return;
            }

            if (!_listing)
            {
                _listing = true;
            }

            ImGui.SetNextWindowPos(position);
            ImGui.SetNextWindowSize(size);

            const ImGuiWindowFlags flags =
                ImGuiWindowFlags.NoTitleBar |
                ImGuiWindowFlags.NoResize |
                ImGuiWindowFlags.NoMove |
                ImGuiWindowFlags.Tooltip |
                ImGuiWindowFlags.HorizontalScrollbar |
                ImGuiWindowFlags.NoSavedSettings;

            ImGui.BeginTooltip();
            {
                isFocused = isFocused || ImGui.IsWindowFocused();
                foreach (var suggestion in _cached)
                {
                    if (ImGui.Selectable(suggestion))
                    {
                        _current = suggestion;
                        _listing = false;
                    }
                }
            }
            ImGui.End();
            _listing = _listing && isFocused;
        }
    }

    internal sealed class AutoSuggestionBox : InputComboBox
    {
        public IEnumerable<string> Suggestions { get; set; } = Enumerable.Empty<string>();

        protected override string[] GetSuggestions()
        {
            const StringComparison noCase = StringComparison.OrdinalIgnoreCase;
            var topSuggestions = Suggestions.Where(input => input.StartsWith(Value, noCase));
            var otherSuggestions = Suggestions.Except(topSuggestions).Where(input => input.Contains(Value, noCase));
            return topSuggestions.Concat(otherSuggestions).ToArray();
        }
    }

    internal sealed class FileSuggestionBox : InputComboBox
    {
        public bool DirectoryOnly { get; set; }
        protected override string[] GetSuggestions()
        {
            var @base = Path.GetDirectoryName(Value);
            if (string.IsNullOrWhiteSpace(@base))
            {
                @base = Directory.Exists(Value) ? Value : ".";
            }

            if (!Directory.Exists(@base))
            {
                return Array.Empty<string>();
            }

            var input = Path.GetFileName(Value);
            var func = DirectoryOnly
                ? (Func<string, string, string[]>)Directory.GetDirectories
                : Directory.GetFileSystemEntries;
            return func(@base, $"{input}*").Take(10).ToArray();
        }
    }
}
