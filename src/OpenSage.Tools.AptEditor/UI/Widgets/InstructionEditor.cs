using System;
using System.Collections.Generic;
using System.Numerics;
using System.Linq;
using ImGuiNET;
using OpenSage.Tools.AptEditor.Apt.Editor;
using OpenSage.Tools.AptEditor.Util;

namespace OpenSage.Tools.AptEditor.UI.Widgets
{
    internal class InstructionEditor : IWidget
    {
        LogicalInstructions _instructions;
        InputComboBox _editBox = new InputComboBox(InstructionUtility.InstructionNames);
        int _editingIndex = -1;

        public InstructionEditor(LogicalInstructions instructions)
        {
            _instructions = instructions;
        }

        public void Draw(AptSceneManager manager)
        {
            if(ImGui.Begin("Instruction Editor"))
            {
                for (var i = 0; i < _instructions.Items.Count; ++i)
                {
                    if(i == _editingIndex)
                    {
                        _editBox.Draw();
                        ImGui.SameLine();
                        if(ImGui.Button("Done"))
                        {
                            _editingIndex = -1;
                        }
                    }
                    else
                    {
                        ImGui.Button(_instructions.Items[i].InstructionName());
                    }
                }
            }
            ImGui.End();
        }
    }

    internal class InputComboBox
    {
        public string ID { get; set; } = Guid.NewGuid().ToString();
        public string PopUpID => $"##{ID}.Suggestions";
        public IEnumerable<string> Suggestions { get; set; }
        private readonly ImGuiTextBox _textBox = new ImGuiTextBox();
        private string _current = string.Empty;
        private bool _listing;

        public InputComboBox(IEnumerable<string> suggestions)
        {
            Suggestions = suggestions;
        }

        public void Draw()
        {
            _textBox.InputText($"##{ID}", out _current);
            _textBox.Hint = null;
            if (ImGui.IsItemActive() && _current.Any())
            {
                var inputPosition = ImGui.GetItemRectMin();
                var inputSize = ImGui.GetItemRectSize();
                var popUpPosition = new Vector2(inputPosition.X, inputPosition.Y + inputSize.Y);
                var popUpSize = new Vector2(inputSize.X, inputSize.Y * 5);
                DrawSuggestions(popUpPosition, popUpSize);
            }
            else
            {
                _listing = false;
            }
        }

        /// Inspired by Harold Brenes' auto complete widget https://github.com/ocornut/imgui/issues/718
        private void DrawSuggestions(Vector2 position, Vector2 size)
        {
            if(Suggestions == null)
            {
                return;
            }

            const StringComparison noCase = StringComparison.OrdinalIgnoreCase;

            var topSuggestions = Suggestions.Where(input => input.StartsWith(_current, noCase));
            var otherSuggestions = Suggestions.Where(input => !input.StartsWith(_current, noCase) && input.Contains(_current, noCase));
            var allSuggestions = topSuggestions.Concat(otherSuggestions);
            if(!allSuggestions.Any())
            {
                return;
            }

            if(!_listing)
            {
                _listing = true;
            }

            ImGui.SetNextWindowPos(position);
            ImGui.SetNextWindowSize(size);

            const ImGuiWindowFlags flags =
                ImGuiWindowFlags.NoTitleBar |
                ImGuiWindowFlags.NoResize |
                ImGuiWindowFlags.NoMove |
                ImGuiWindowFlags.HorizontalScrollbar |
                ImGuiWindowFlags.NoSavedSettings;

            if(ImGui.Begin(PopUpID, flags))
            {
                ImGui.PushAllowKeyboardFocus(false);

                foreach(var suggestion in allSuggestions)
                {
                    if(ImGui.Selectable(suggestion))
                    {
                        _textBox.Hint = suggestion;
                    }
                }
                ImGui.PopAllowKeyboardFocus();
            }
            ImGui.End();
        }
    }

}
