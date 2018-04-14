using System.Collections.Generic;
using System.Numerics;
using System.Text;
using ImGuiNET;
using OpenSage.Data.Map;
using OpenSage.Scripting;

namespace OpenSage.Viewer.UI.Views
{
    internal sealed class MapView : GameView
    {
        private readonly Game _game;
        private readonly StringBuilder _scriptStateContent;

        public MapView(AssetViewContext context)
            : base(context)
        {
            _game = context.Game;
            _game.Scene3D = _game.ContentManager.Load<Scene3D>(context.Entry.FilePath);

            _scriptStateContent = new StringBuilder();

            _game.Scripting.OnUpdateFinished += OnScriptingUpdateFinished;

            AddDisposeAction(() => _game.Scripting.OnUpdateFinished -= OnScriptingUpdateFinished);
        }

        private void OnScriptingUpdateFinished(object sender, ScriptingSystem scripting)
        {
            _scriptStateContent.Clear();

            _scriptStateContent.AppendLine("Counters:");

            foreach (var kv in scripting.Counters)
            {
                _scriptStateContent.AppendFormat("  {0}: {1}\n", kv.Key, kv.Value);
            }

            _scriptStateContent.AppendLine("Flags:");

            foreach (var kv in scripting.Flags)
            {
                _scriptStateContent.AppendFormat("  {0}: {1}\n", kv.Key, kv.Value);
            }

            _scriptStateContent.AppendLine("Timers:");

            foreach (var kv in scripting.Timers)
            {
                _scriptStateContent.AppendFormat("  {0}: {1} ({2})\n", kv.Key, kv.Value, scripting.Counters[kv.Key]);
            }
        }

        public override void Draw(ref bool isGameViewFocused)
        {
            ImGui.BeginChild("map panels", new Vector2(250, 0), true, 0);

            if (ImGui.CollapsingHeader("General", TreeNodeFlags.DefaultOpen))
            {
                var lighting = _game.Scene3D.Lighting;

                ImGui.Text("Time of day");
                {
                    foreach (var timeOfDay in GetTimesOfDay())
                    {
                        if (ImGui.RadioButtonBool(timeOfDay.ToString(), lighting.TimeOfDay == timeOfDay))
                        {
                            lighting.TimeOfDay = timeOfDay;
                        }
                    }
                }

                ImGui.Separator();

                var enableCloudShadows = lighting.EnableCloudShadows;
                if (ImGui.Checkbox("Enable cloud shadows", ref enableCloudShadows))
                {
                    lighting.EnableCloudShadows = enableCloudShadows;
                }

                ImGui.Separator();

                var enableMacroTexture = _game.Scene3D.Terrain.EnableMacroTexture;
                if (ImGui.Checkbox("Enable macro texture", ref enableMacroTexture))
                {
                    _game.Scene3D.Terrain.EnableMacroTexture = enableMacroTexture;
                }
            }

            if (ImGui.CollapsingHeader("Scripts", 0))
            {
                if (ImGui.Button(_game.Scripting.Active ? "Stop Scripts" : "Start Scripts"))
                {
                    _game.Scripting.Active = !_game.Scripting.Active;
                }

                ImGui.Separator();

                ImGui.Text(_scriptStateContent.ToString());

                ImGui.Separator();

                var scriptsList = _game.Scene3D.MapFile.GetPlayerScriptsList();
                for (var i = 0; i < scriptsList.ScriptLists.Length; i++)
                {
                    DrawScriptList(i, scriptsList.ScriptLists[i]);
                }
            }

            ImGui.EndChild();

            ImGui.SameLine();

            base.Draw(ref isGameViewFocused);
        }

        private static IEnumerable<TimeOfDay> GetTimesOfDay()
        {
            yield return TimeOfDay.Morning;
            yield return TimeOfDay.Afternoon;
            yield return TimeOfDay.Evening;
            yield return TimeOfDay.Night;
        }

        private static void DrawScriptList(int index, ScriptList scriptList)
        {
            if (ImGui.TreeNodeEx($"Script List {index}", TreeNodeFlags.DefaultOpen))
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

        private static void DrawScriptGroup(ScriptGroup scriptGroup)
        {
            if (ImGui.TreeNodeEx(GetScriptDetails(scriptGroup.Name, scriptGroup.IsActive, scriptGroup.IsSubroutine), TreeNodeFlags.DefaultOpen))
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

        private static void DrawScript(Script script)
        {
            ImGui.BulletText(GetScriptDetails(script.Name, script.IsActive, script.IsSubroutine));
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
