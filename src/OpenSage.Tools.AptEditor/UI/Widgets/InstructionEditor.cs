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
            if (ImGui.Begin("VM Console"))
            {
                var exec_opr = ImGui.Button("Exec");
                ImGui.SameLine();
                var execj_opr = ImGui.Button("Exec(Jump Code Blocks)");
                if (exec_opr)
                {

                }
                if (execj_opr)
                {

                }

                string exec_code_ref = "";
                ImGui.InputTextMultiline("Code", ref exec_code_ref, 114514, new System.Numerics.Vector2(200, 200));
                if (ImGui.Button("Exec Code")) {
                    throw new System.NotImplementedException(exec_code_ref);
                }

                ImGui.Separator();

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
                        ImGui.Button(_instructions.Items[i].ToString());
                    }
                }
            }
            ImGui.End();
        }
    }
}
