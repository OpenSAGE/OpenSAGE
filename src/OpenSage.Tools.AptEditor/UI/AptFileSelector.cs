using System;
using System.Linq;
using ImGuiNET;
using OpenSage.Data;
using OpenSage.Tools.AptEditor.Util;

namespace OpenSage.Tools.AptEditor.UI
{
    internal sealed class AptFileSelector
    {
        public ref bool Visible => ref _open;
        private readonly FileSystem _fileSystem;
        private readonly CustomSuggestionBox _filePathInput;
        private string? _value;
        private bool _open;
        private bool _showHint;

        public AptFileSelector(FileSystem fileSystem)
        {
            _fileSystem = fileSystem;
            _filePathInput = new CustomSuggestionBox
            {
                SuggestionsProvider = value => _fileSystem
                    .FindFiles(entry => entry.FilePath.Contains(value) && entry.FilePath.EndsWith(".apt", StringComparison.OrdinalIgnoreCase))
                    .Select(entry => entry.FilePath)
            };
        }

        public string? GetValue()
        {
            var value = _value;
            _value = null;
            return value;
        }
        public void SetValue(string? value)
        {
            _value = value;
            return;
        }

        public void Draw()
        {
            if (!Visible)
            {
                return;
            }

            if (ImGui.Begin("Open Apt File...", ref Visible))
            {
                ImGui.Text("Apt path");
                ImGui.SameLine();
                _filePathInput.Draw();

                if (_showHint)
                {
                    ImGui.TextWrapped("You need to add the containing folder to the search path by using File -> Add Search Path.");
                }

                if (ImGui.Button("Open"))
                {
                    var value = _filePathInput.Value;
                    if(_fileSystem.GetFile(value) != null)
                    {
                        _value = value;
                        Visible = false;
                    }
                    else
                    {
                        _showHint = true;
                    }
                }

                ImGui.SetItemDefaultFocus();

                ImGui.SameLine();

                if (ImGui.Button("Cancel"))
                {
                    Visible = false;
                }
            }
            ImGui.End();
        }
    }
}
