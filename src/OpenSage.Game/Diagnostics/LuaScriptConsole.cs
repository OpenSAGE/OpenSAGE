using System;
using System.Collections.Generic;
using System.Numerics;
using ImGuiNET;
using OpenSage.Diagnostics.Util;
using MoonSharp.Interpreter;
using OpenSage.Scripting;

namespace OpenSage.Diagnostics
{
    internal sealed class LuaScriptConsole : DiagnosticView
    {
        private static string _scriptConsoleText = "";
        public static string _scriptConsoleTextAll = "";
        public static Vector4 _consoleTextColor = new Vector4(0, 150, 0, 1);

        public override Vector2 DefaultSize { get; } = new Vector2(150, 50);

        public override string DisplayName { get; } = "LUA script console";

        public LuaScriptConsole(DiagnosticViewContext context) : base(context){}

        protected override void DrawOverride(ref bool isGameViewFocused)
        {
            ImGui.PushItemWidth(-1);
            ImGui.InputTextMultiline("", ref _scriptConsoleText, 1000, Vector2.Zero);
            ImGui.PopItemWidth();

            //ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0, 120, 0, 1));
            //ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0, 0, 204, 255));
            if (ImGui.Button("run script") || ImGui.IsKeyPressed(0x0D))
            {
                _scriptConsoleTextAll = string.Concat(_scriptConsoleTextAll, ">> ", _scriptConsoleText.Replace("\n","\n>> "), "\n");
                Console.Write(string.Concat(">> ", _scriptConsoleText.Replace("\n", "\n>> "), "\n"));
                ExecuteLuaScript(_scriptConsoleText);
            }
            //ImGui.PopStyleColor();
            //ImGui.PopStyleColor();

            ImGui.SameLine();
            if (ImGui.Button("clear"))
            {
                _scriptConsoleTextAll = "";
            }

            ImGui.SameLine();
            var loadPopupID = "load";
            if (ImGui.Button("load"))
            {
                ImGui.OpenPopup(loadPopupID);
            }

            var savePopupID = "save";
            ImGui.SameLine();
            if (ImGui.Button("save"))
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

            if (ImGui.BeginPopup("load"))
            {
                ImGui.Text("soon");
                ImGui.EndPopup();
            }


            if (ImGui.BeginPopup("save"))
            {
                ImGui.Text("soon");
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
                Console.WriteLine("LUA SYNTAX ERROR: ", exeption.DecoratedMessage);
                _scriptConsoleTextAll = string.Concat(_scriptConsoleTextAll, "LUA SYNTAX ERROR: ", exeption.DecoratedMessage, "\n");
                _consoleTextColor = new Vector4(150, 0, 0, 1);
            }
            catch (ScriptRuntimeException exeption)
            {
                Console.WriteLine($"LUA RUNTIME ERROR: ", exeption.DecoratedMessage);
                _scriptConsoleTextAll = string.Concat(_scriptConsoleTextAll, "LUA RUNTIME ERROR: ", exeption.DecoratedMessage, "\n");
                _consoleTextColor = new Vector4(150, 0, 0, 1);
            }
            catch (Exception exeption)
            {
                Console.WriteLine($"LUA RUNTIME ERROR: {0}", exeption);
                _scriptConsoleTextAll = string.Concat(_scriptConsoleTextAll, "FATAL ERROR: ", exeption, "\n");
                _consoleTextColor = new Vector4(150, 0, 0, 1);
            }
        }
    }
}
