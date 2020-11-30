using System.Collections.Generic;
using System.Numerics;
using ImGuiNET;
using OpenSage.Mathematics;
using OpenSage.Tools.AptEditor.Apt.Editor;

namespace OpenSage.Tools.AptEditor.UI.Widgets
{
    internal sealed class GeometryEditor : IWidget
    {
        private GeometryUtilities? _utilities;
        private KeyValuePair<int, string>? _selected;
        private int _newGeometryId = 0;
        private string _currentInput = string.Empty;
        private string? _lastError;

        public void Draw(AptSceneManager manager)
        {
            if (_utilities?.Manager != manager.AptManager)
            {
                _utilities = manager.AptManager is null
                    ? null
                    : new GeometryUtilities(manager.AptManager);
                _selected = null;
                _lastError = null;
                _currentInput = string.Empty;
            }

            if (_utilities is null)
            {
                return;
            }

            if (ImGui.Begin("Apt Geometries"))
            {
                if (_selected.HasValue)
                {
                    ImGui.Columns(2);
                    ImGui.Value("Geometry", _selected.Value.Key);
                }

                ImGui.InputInt("New geomery id", ref _newGeometryId);
                if (_utilities.HasGeometry(_newGeometryId))
                {
                    ImGui.Text($"Geometry #{_newGeometryId} already exists.");
                }
                else if (ImGui.Button("New geometry"))
                {
                    _utilities.AddGeometry(_newGeometryId);
                }

                DrawGeometryList();

                if (_selected is KeyValuePair<int, string> selected)
                {
                    ImGui.NextColumn();
                    DrawInput(selected);
                }

            }
            ImGui.End();
        }

        private void DrawGeometryList()
        {
            if (ImGui.BeginChild("Geometry list"))
            {
                foreach (var data in _utilities!.Data)
                {
                    var wasSelected = data.Key == _selected?.Key;
                    var isSelected = wasSelected;
                    ImGui.Selectable($"Geometry #{data.Key}", ref isSelected);
                    if (isSelected)
                    {
                        _selected = data;
                        if (!wasSelected)
                        {
                            _lastError = null;
                            _currentInput = data.Value;
                        }
                    }
                }
            }
            ImGui.EndChild();
        }

        private void DrawInput(in KeyValuePair<int, string> selected)
        {
            const string caption = "Geometry commands";
            const string inputId = "##" + caption;

            ImGui.Text(caption);
            ImGui.SameLine();
            var reset = ImGui.Button("Reset");
            if (_lastError is not null)
            {
                ImGui.TextColored(ColorRgbaF.Red.ToVector4(), _lastError);
            }
            var input = ImGui.InputTextMultiline(inputId, ref _currentInput, 1024, Vector2.One * -1);

            if (input)
            {
                try
                {
                    var id = ImGui.GetID(inputId).ToString();
                    _utilities!.UpdateGeometry(id, selected.Key, _currentInput);
                    _lastError = null;
                }
                catch (AptEditorException e)
                {
                    _lastError = e.ErrorType switch
                    {
                        ErrorType.InvalidTextureIdInGeometry => $"Invalid texture id in textured triangle: {e.Message}",
                        ErrorType.FailedToParseGeometry => $"Syntax error: {e.Message}",
                        _ => $"Unknown error: {e.Message}"
                    };
                }
            }

            if (reset)
            {
                _currentInput = selected.Value;
            }
        }

        public void RefreshInput()
        {
            foreach (var data in _utilities!.Data)
            {
                var wasSelected = data.Key == _selected?.Key;
                if (wasSelected)
                {
                    _selected = data;
                    _lastError = null;
                    _currentInput = data.Value;
                }
            }
        }

    }
}
