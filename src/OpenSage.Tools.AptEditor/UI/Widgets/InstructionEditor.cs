using ImGuiNET;
using OpenSage.Tools.AptEditor.Apt.Editor;
using OpenSage.Tools.AptEditor.Util;

namespace OpenSage.Tools.AptEditor.UI.Widgets
{
    internal class InstructionEditor : IWidget
    {
        LogicalInstructions _instructions;
        InputComboBox _editBox = new AutoSuggestionBox
        {
            Suggestions = InstructionUtility.InstructionNames
        };
        int _editingIndex = -1;

        public InstructionEditor(LogicalInstructions instructions)
        {
            _instructions = instructions;
        }

        public void Draw(AptSceneManager manager)
        {
            if (ImGui.Begin("Instruction Editor"))
            {
                for (var i = 0; i < _instructions.Items.Count; ++i)
                {
                    if (i == _editingIndex)
                    {
                        _editBox.Draw();
                        ImGui.SameLine();
                        if (ImGui.Button("Done"))
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
}
