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
        private enum ShowState
        {
            Hidden,
            FocusTextBox,
            Visible,
            FocusMenu,
            
        }
        public string ID { get; set; } = Guid.NewGuid().ToString();
        public string PopUpID => $"##{ID}.Suggestions";
        public string Value => _current;

        private string[] _cached = Array.Empty<string>();
        private string _last = string.Empty;
        private string _current = string.Empty;
        private Vector2 _position;
        private Vector2 _size;
        private float _lastWidth;
        private float _lastHeight;
        private bool _hasOverflow;
        private ShowState _show;

        protected abstract IEnumerable<string> GetSuggestions();

        public void Draw()
        {
            switch (_show)
            {
                case ShowState.FocusMenu:
                case ShowState.Visible:
                    ImGui.Dummy(_size);
                    DrawSuggestions();
                    break;
                case ShowState.FocusTextBox:
                    ImGui.SetWindowFocus();
                    ImGui.SetKeyboardFocusHere();
                    _show = ShowState.Hidden;
                    goto case ShowState.Hidden;
                case ShowState.Hidden:
                    if (ImGui.InputText($"##{ID}", ref _current, 1024) || ImGui.IsItemClicked())
                    {
                        _show = ShowState.FocusMenu;
                    }
                    _position = ImGui.GetItemRectMin();
                    _size = ImGui.GetItemRectSize();
                    break;

            }
        }

        /// Inspired by Harold Brenes' auto complete widget https://github.com/ocornut/imgui/issues/718
        private void DrawSuggestions()
        {
            if (_last != _current)
            {
                _cached = GetSuggestions().ToArray();
            }
            _last = _current;

            if (_show == ShowState.FocusMenu)
            {
                ImGui.SetNextWindowFocus();
            }

            ImGui.SetNextWindowPos(_position);
            ImGui.SetNextWindowSize(new Vector2(_size.X, _lastHeight > _size.Y * 12 ? _size.Y * 8 : 0));
            const ImGuiWindowFlags flags =
                ImGuiWindowFlags.NoTitleBar |
                ImGuiWindowFlags.NoResize |
                ImGuiWindowFlags.NoMove |
                ImGuiWindowFlags.HorizontalScrollbar |
                ImGuiWindowFlags.NoSavedSettings;

            if (ImGui.Begin($"##{ID}.Tooltip", flags))
            {
                var width = _size.X - 16;
                if (_hasOverflow)
                {
                    ImGui.SetCursorPos(new Vector2(ImGui.GetScrollX() + 4, ImGui.GetCursorPosY()));
                    width -= 12;
                }
                ImGui.SetNextItemWidth(width);
                if (_show == ShowState.FocusMenu)
                {
                    ImGui.SetKeyboardFocusHere();
                }
                ImGui.InputText($"##{ID}", ref _current, 1024);
                _show = ImGui.IsWindowFocused() ? ShowState.Visible : ShowState.Hidden;
                var lastWidth = _lastWidth;
                _lastWidth = _size.X;
                foreach (var suggestion in _cached)
                {
                    width = ImGui.CalcTextSize(suggestion).X;
                    if (_hasOverflow)
                    {
                        ImGui.SetCursorPos(new Vector2(Math.Max(8, lastWidth - width + 8), ImGui.GetCursorPosY()));
                    }
                    if (ImGui.Selectable(suggestion))
                    {
                        _show = ShowState.FocusTextBox;
                        _current = suggestion;
                    }
                    _lastWidth = Math.Max(width + 16, _lastWidth);
                }
                _hasOverflow = _lastWidth - ImGui.GetWindowSize().X > 1;
                if (_hasOverflow)
                {
                    ImGui.TextUnformatted(string.Empty);
                    ImGui.SetScrollX(ImGui.GetScrollMaxX());
                }
                _lastHeight = ImGui.GetCursorPosY();
            }
            ImGui.End();
        }
    }

    internal sealed class AutoSuggestionBox : InputComboBox
    {
        public IEnumerable<string> Suggestions { get; set; } = Enumerable.Empty<string>();

        protected override IEnumerable<string> GetSuggestions()
        {
            const StringComparison noCase = StringComparison.OrdinalIgnoreCase;
            var topSuggestions = Suggestions.Where(input => input.StartsWith(Value, noCase));
            var otherSuggestions = Suggestions.Except(topSuggestions).Where(input => input.Contains(Value, noCase));
            return topSuggestions.Concat(otherSuggestions);
        }
    }

    internal sealed class FileSuggestionBox : InputComboBox
    {
        public bool DirectoryOnly { get; set; }
        protected override IEnumerable<string> GetSuggestions()
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
            return func(@base, $"{input}*").Take(16);
        }
    }

    internal sealed class CustomSuggestionBox : InputComboBox
    {
        public Func<string, IEnumerable<string>> SuggestionsProvider { get; set; } = _ => Array.Empty<string>();
        protected override IEnumerable<string> GetSuggestions()
        {
            return SuggestionsProvider(Value);
        }
    }
}
