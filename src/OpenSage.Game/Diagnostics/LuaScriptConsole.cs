using System;
using System.Numerics;
using ImGuiNET;
using MoonSharp.Interpreter;

namespace OpenSage.Diagnostics
{
    internal sealed class LuaScriptConsole : DiagnosticView
    {
        private static string _scriptConsoleText = "";
        public static string _scriptConsoleTextAll = "";
        public static Vector4 _consoleTextColor = new Vector4(0, 150, 0, 1);

        public override string DisplayName { get; } = "LUA script console";

        public LuaScriptConsole(DiagnosticViewContext context) : base(context) { }

        protected override void DrawOverride(ref bool isGameViewFocused)
        {
            ImGui.PushItemWidth(-1);
            ImGui.InputTextMultiline("", ref _scriptConsoleText, 1000, Vector2.Zero);
            ImGui.PopItemWidth();

            if (ImGui.Button("Run") || ImGui.IsKeyPressed(ImGuiKey.Enter))
            {
                _scriptConsoleTextAll = string.Concat(_scriptConsoleTextAll, ">> ", _scriptConsoleText.Replace("\n", "\n>> "), "\n");
                ExecuteLuaScript(_scriptConsoleText);
            }

            ImGui.SameLine();
            if (ImGui.Button("Clear"))
            {
                _scriptConsoleTextAll = "";
            }

            ImGui.SameLine();
            var loadPopupID = "load";
            if (ImGui.Button("Load"))
            {
                ImGui.OpenPopup(loadPopupID);
            }

            var savePopupID = "save";
            ImGui.SameLine();
            if (ImGui.Button("Save"))
            {
                ImGui.OpenPopup(savePopupID);
            }

            ImGui.PushStyleColor(ImGuiCol.Border, new Vector4(0, 0, 150, 1));
            ImGui.Separator();
            ImGui.BeginChild("cmd", Vector2.Zero, true);
            ImGui.PushStyleColor(ImGuiCol.Text, _consoleTextColor);
            ImGui.Text(_scriptConsoleTextAll);
            ImGui.PopStyleColor();
            ImGui.EndChild();
            ImGui.PopStyleColor();

            if (ImGui.BeginPopup(loadPopupID))
            {
                ImGui.Text("Soon");
                ImGui.EndPopup();
            }

            if (ImGui.BeginPopup(savePopupID))
            {
                ImGui.Text("Soon");
                ImGui.EndPopup();
            }
        }

        private void ExecuteLuaScript(string userInputScript)
        {
            try
            {
                //Game.Lua.MainScript.DoString(userInputScript);
                Game.Lua.ExecuteUserCode(userInputScript);
                _consoleTextColor = new Vector4(0, 150, 0, 1);
            }
            catch (SyntaxErrorException exeption)
            {
                _scriptConsoleTextAll = string.Concat(_scriptConsoleTextAll, "LUA SYNTAX ERROR: ", exeption.DecoratedMessage, "\n");
                _consoleTextColor = new Vector4(150, 0, 0, 1);
            }
            catch (ScriptRuntimeException exeption)
            {
                _scriptConsoleTextAll = string.Concat(_scriptConsoleTextAll, "LUA RUNTIME ERROR: ", exeption.DecoratedMessage, "\n");
                _consoleTextColor = new Vector4(150, 0, 0, 1);
            }
            catch (Exception exeption)
            {
                _scriptConsoleTextAll = string.Concat(_scriptConsoleTextAll, "FATAL ERROR: ", exeption, "\n");
                _consoleTextColor = new Vector4(150, 0, 0, 1);
            }
        }
    }
}
