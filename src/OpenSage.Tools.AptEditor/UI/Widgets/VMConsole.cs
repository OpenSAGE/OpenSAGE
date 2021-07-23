using ImGuiNET;
using OpenSage.Tools.AptEditor.Apt.Editor;
using OpenSage.Tools.AptEditor.Util;
using OpenSage.Gui.Apt;
using OpenSage.Gui.Apt.ActionScript;
using System.Numerics;

namespace OpenSage.Tools.AptEditor.UI.Widgets
{
    internal class VMConsole : IWidget
    {
        LogicalInstructions _instructions = null;
        AptContext _context = null;
        ObjectContext _dispobj = null;
        ActionContext _acontext = null;
        InputComboBox _editBox = new AutoSuggestionBox
        {
            Suggestions = InstructionUtility.InstructionNames
        };
        int _editingIndex = -1;

        //public VMConsole(LogicalInstructions instructions)
        //{

        //}

        string input_type = "Number";
        string input_val = "114514";
        int int_input = 0;
        string exec_code_ref = "";
        string last_executed_func = "Initial output";

        Vector4 _breakColor = new Vector4(1f, 0.1f, 0.1f, 1f);
        Vector4 _unbreakColor = new Vector4(0.1f, 0.1f, 0.1f, 1f);
        string _breakDesc = "*";

        public void CheckEnv(AptSceneManager manager)
        {
            if (manager.CurrentWindow == null || manager.CurrentDisplay is null)
            {
                _context = null;
                _acontext = null;
                _instructions = null;
                _dispobj = null;
            }
            else
            {
                _instructions = manager.CurrentActions;
                _dispobj = manager.CurrentDisplay.ScriptObject;
                _context = manager.CurrentDisplay.Context;
                _acontext = _context.Avm.CurrentContext();
            }
        }

        public void Draw(AptSceneManager manager)
        {
            CheckEnv(manager);

            if (ImGui.Begin("VM Console"))
            {
                // ImGui.Columns(2);
                ImGui.InputInt("Line", ref int_input);
                ImGui.SameLine();
                if (ImGui.Button("Goto"))
                {

                }
                // ImGui.SameLine();
                if (ImGui.Button("Exec1"))
                {
                    if (!_acontext.IsOutermost())
                    {
                        var lef = _context.Avm.ExecuteOnce(true); 
                        last_executed_func = lef.ToString(_acontext);
                    }
                    else if (_context != null)
                    {
                        _context.Avm.PushContext(_context.Avm.DequeueContext());
                        last_executed_func = "New Actions Pushed";
                    }
                    else
                    {
                        last_executed_func = "AptContext Not Loaded";
                    }
                    
                }
                if (_context != null)
                {
                    ImGui.SameLine();
                    if (ImGui.Button(_context.Avm.Paused() ? "Resume" : "Pause"))
                    {
                        if (_context.Avm.Paused())
                            _context.Avm.Resume();
                        else
                            _context.Avm.Pause();
                    }
                    ImGui.SameLine();
                    if (ImGui.Button("EUHalt"))
                    {
                        if (_context.Avm.Paused())
                        {
                            _context.Avm.Resume();
                            _context.Avm.ExecuteUntilHalt();
                            _context.Avm.Pause();
                        }
                        else
                            _context.Avm.ExecuteUntilHalt();
                    }
                    ImGui.SameLine();
                    if (ImGui.Button("EUGlob"))
                    {
                        if (_context.Avm.Paused())
                        {
                            _context.Avm.Resume();
                            _context.Avm.ExecuteUntilGlobal();
                            _context.Avm.Pause();
                        }
                        else
                            _context.Avm.ExecuteUntilGlobal();
                    }
                }
                
                ImGui.InputText("Type", ref input_type, 1000);
                //ImGui.SameLine();
                ImGui.InputText("Value", ref input_val, 1000);
                //ImGui.SameLine();
                if (ImGui.Button("Push"))
                {

                }
                ImGui.SameLine();
                if (ImGui.Button("Pop"))
                {

                }
                ImGui.SameLine();
                if (ImGui.Button("Pop & Show"))
                {

                }

                
                ImGui.InputTextMultiline("Code", ref exec_code_ref, 114514, new System.Numerics.Vector2(200, 200));
                if (ImGui.Button("Exec Code")) {
                    throw new System.NotImplementedException(exec_code_ref);
                }

                ImGui.Separator();

                ImGui.Text("Last Execution:");
                ImGui.Text(last_executed_func);

                ImGui.Text("Registers:");
                ImGui.Text(_acontext == null ? "Not Applicable" : _acontext.DumpRegister());

                ImGui.Text("Stack:");
                ImGui.Text(_acontext == null ? "Not Applicable" : _acontext.DumpStack());

                ImGui.Text("Execution Context:");
                ImGui.Text(_context == null ? "Not Applicable" : _context.Avm.DumpContextStack());

                ImGui.Text("Code Queue:");
                ImGui.Text(_context == null ? "Not Applicable" : _context.Avm.DumpContextQueue());

                

                ImGui.Text("KW This:");
                ImGui.Text(_acontext == null ? "null" : (
                    _acontext.This == _acontext.Global ? "Same as Global" : _acontext.This.ToStringDisp(_acontext)
                    ));

                ImGui.Text("Display Item:");
                ImGui.Text(_dispobj == null ? "null" : _dispobj.ToStringDisp(_acontext));

                ImGui.Text("KW Global:");
                ImGui.Text(_acontext == null ? "null" : _acontext.Global.ToStringDisp(_acontext));

                

                if (_instructions != null && ImGui.Begin("Codes"))
                {
                    DrawCodes(_instructions);
                }
                ImGui.End();
            }
            ImGui.End();
        }

        internal void DrawCodes(LogicalInstructions insts, int indent = 0)
        {
            for (var i = 0; i < insts.Items.Count; ++i)
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
                    var inst = insts.Items[i];

                    ImGui.TextColored(inst.Breakpoint ? _breakColor : _unbreakColor, _breakDesc);
                    ImGui.SameLine(24, indent);
                
                    if (ImGui.Button(inst.ToString(_acontext)))
                    {
                        inst.Breakpoint = !inst.Breakpoint;
                    }

                    if (inst is LogicalCodeContext lcc)
                    {
                        DrawCodes(lcc.Instructions, indent + 16);
                    }
                }
            }
        }
    }
}
