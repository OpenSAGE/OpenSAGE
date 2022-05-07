using System.Text;
using ImGuiNET;
using OpenSage.Scripting;

namespace OpenSage.Diagnostics
{
    internal sealed class ScriptingView : DiagnosticView
    {
        private readonly StringBuilder _scriptStateContent;

        public override string DisplayName { get; } = "Scripting";

        public ScriptingView(DiagnosticViewContext context)
            : base(context)
        {
            _scriptStateContent = new StringBuilder();

            Game.Scripting.OnUpdateFinished += OnScriptingUpdateFinished;

            AddDisposeAction(() => Game.Scripting.OnUpdateFinished -= OnScriptingUpdateFinished);
        }

        private void OnScriptingUpdateFinished(object sender, ScriptingSystem scripting)
        {
            if (!IsVisible)
            {
                return;
            }

            _scriptStateContent.Clear();

            scripting.Dump(_scriptStateContent);
        }

        protected override void DrawOverride(ref bool isGameViewFocused)
        {
            if (Game.Scene3D == null)
            {
                ImGui.Text("Script diagnostics are only available when a map has been loaded.");
                return;
            }

            if (ImGui.Button(Game.Scripting.Active ? "Stop Scripts" : "Start Scripts"))
            {
                Game.Scripting.Active = !Game.Scripting.Active;
            }

            ImGui.Separator();

            ImGui.Text(_scriptStateContent.ToString());

            ImGui.Separator();

            var scriptsList = Game.Scene3D.MapFile.GetPlayerScriptsList();
            for (var i = 0; i < scriptsList.ScriptLists.Length; i++)
            {
                DrawScriptList(i, scriptsList.ScriptLists[i]);
            }
        }

        private void DrawScriptList(int index, ScriptList scriptList)
        {
            if (ImGui.TreeNodeEx($"Script List {index}", ImGuiTreeNodeFlags.DefaultOpen))
            {
                foreach (var childGroup in scriptList.ScriptGroups)
                {
                    DrawScriptGroup(childGroup);
                }

                foreach (var script in scriptList.Scripts)
                {
                    DrawScript(script);
                }

                ImGui.TreePop();
            }
        }

        private void DrawScriptGroup(ScriptGroup scriptGroup)
        {
            if (ImGui.TreeNodeEx(GetScriptDetails(scriptGroup.Name, scriptGroup.IsActive, scriptGroup.IsSubroutine), ImGuiTreeNodeFlags.DefaultOpen))
            {
                foreach (var childGroup in scriptGroup.Groups)
                {
                    DrawScriptGroup(childGroup);
                }

                foreach (var script in scriptGroup.Scripts)
                {
                    DrawScript(script);
                }

                ImGui.TreePop();
            }
        }

        private void DrawScript(Script script)
        {
            if (ImGui.Selectable(GetScriptDetails(script.Name, script.IsActive, script.IsSubroutine)))
            {
                Context.SelectedObject = script;
            }
        }

        private static string GetScriptDetails(string name, bool isActive, bool isSubroutine)
        {
            var result = name;

            result += isActive ? " (Active" : "(Inactive";

            if (isSubroutine)
            {
                result += ", Subroutine";
            }

            result += ")";

            return result;
        }
    }
}
