using System;
using System.Linq;
using ImGuiNET;
using OpenSage.Data;
using OpenSage.Tools.AptEditor.Util;

namespace OpenSage.Tools.AptEditor.UI
{
    internal sealed class ExportPathSelector
    {
        public ref bool Visible => ref _open;
        private readonly FileSuggestionBox _filePathInput = new FileSuggestionBox
        {
            DirectoryOnly = true
        };
        private string? _value;
        private bool _open;

        public string? GetValue()
        {
            var value = _value;
            _value = null;
            return value;
        }

        public void Draw()
        {
            if (!Visible)
            {
                return;
            }

            if (ImGui.Begin("Save Apt File...", ref Visible))
            {
                ImGui.Text("Save path");
                ImGui.SameLine();
                _filePathInput.Draw();

                if (ImGui.Button("Save"))
                {
                    _value = _filePathInput.Value;
                    Visible = false;
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
