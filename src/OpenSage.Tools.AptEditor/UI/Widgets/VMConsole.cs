﻿using ImGuiNET;
using OpenSage.Tools.AptEditor.Apt.Editor;
using OpenSage.Tools.AptEditor.Util;
using OpenSage.Gui.Apt;
using OpenSage.Gui.Apt.ActionScript;
using System.Numerics;
using OpenSage.Gui.Apt.ActionScript.Opcodes;
using OpenSage.Tools.AptEditor.ActionScript;

namespace OpenSage.Tools.AptEditor.UI.Widgets
{
    internal class ObjectDescription
    {
        public ASObject obj;
        public ASObject tv;
        public ExecutionContext actx;
        public string[] disp;
        public string[] val;
        public string addr;
        public string obj_str;
        public ObjectDescription sub;
        public int elem_sel;

        public ObjectDescription(ASObject obj, ExecutionContext actx = null)
        {
            this.obj = obj;
            tv = obj;
            this.actx = actx;
            addr = "";
            if (obj != null)
            {
                var objd = obj.ToListDisp(actx);
                disp = objd.Item1;
                val = objd.Item2;
            }
        }
    }
    internal class VMConsole : IWidget
    {
        InstructionGraph _instructions = null;
        AptContext _context = null;
        ASObject _dispobj = null;
        ExecutionContext _acontext = null;
        InputComboBox _editBox = new AutoSuggestionBox
        {
            Suggestions = InstructionUtility.InstructionNames
        };
        int _editingIndex = -1;
        string _title = "";

        //public VMConsole(LogicalInstructions instructions)
        //{

        //}

        string input_type = "Number";
        string inputVal = "114514";
        int int_input = 0;
        string execCodeRef = "";
        string lastExecFunc = "Initial output";

        int selectedStack = 0;
        int selectedReg = 0;
        int selected_cstack = 0;
        int selected_cqueue = 0;

        ObjectDescription obj_disp = null;
        int obj_elem_sel = 0;

        Vector4 _breakColor = new Vector4(1f, 0.1f, 0.1f, 1f);
        Vector4 _unbreakColor = new Vector4(0.1f, 0.1f, 0.1f, 1f);
        string _breakDesc = "*";

        public void CheckEnv(LogicalMainForm manager)
        {
            var scene = manager.Scene;
            if (scene.CurrentWindow == null || scene.CurrentItem is null)
            {
                _context = null;
                _acontext = null;
                _instructions = null;
                _title = "";
                _dispobj = null;
            }
            else
            {
                _instructions = manager.CurrentActions;
                _dispobj = scene.CurrentItem.ScriptObject;
                _context = scene.CurrentItem.Context;
                _title = manager.CurrentTitle;
            }
        }

        public void Draw(LogicalMainForm manager)
        {
            CheckEnv(manager);
            var cur_ctx = _context == null ? null : _context.Avm.CurrentContext();

            if (ImGui.Begin("VM Console"))
            {
                ImGui.Columns(2);

                ImGui.InputInt("Line", ref int_input);
                ImGui.SameLine();
                if (ImGui.Button("Goto") && cur_ctx != null && cur_ctx.Stream != null)
                {
                    cur_ctx.Stream.GotoIndex(int_input);
                }
                // ImGui.SameLine();
                if (ImGui.Button("Exec1"))
                {
                    lastExecFunc = manager.Scene.ExecuteOnce();
                    
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
                        InstructionBase lef = null;
                        if (_context.Avm.Paused())
                        {
                            _context.Avm.Resume();
                            lef = _context.Avm.ExecuteUntilHalt();
                            _context.Avm.Pause();
                        }
                        else
                            lef = _context.Avm.ExecuteUntilHalt();
                        lastExecFunc = lef == null ? "null" : lef.ToString(_acontext);
                    }
                    ImGui.SameLine();
                    if (ImGui.Button("EUGlob"))
                    {
                        InstructionBase lef = null;
                        if (_context.Avm.Paused())
                        {
                            _context.Avm.Resume();
                            lef = _context.Avm.ExecuteUntilGlobal();
                            _context.Avm.Pause();
                        }
                        else
                            lef = _context.Avm.ExecuteUntilGlobal();
                        lastExecFunc = lef == null ? "null" : lef.ToString(_acontext);
                    }
                }
                
                ImGui.InputText("Type", ref input_type, 1000);
                //ImGui.SameLine();
                ImGui.InputText("Value", ref inputVal, 1000);
                //ImGui.SameLine();
                if (ImGui.Button("Push"))
                {
                    throw new System.NotImplementedException(inputVal);
                }
                ImGui.SameLine();
                if (ImGui.Button("Pop"))
                {
                    _acontext.Pop();
                }

                
                ImGui.InputTextMultiline("Code", ref execCodeRef, 114514, new Vector2(300, 200));
                if (ImGui.Button("Exec Code")) {
                    throw new System.NotImplementedException(execCodeRef);
                }

                ImGui.Text("Last Execution:");
                ImGui.Text(lastExecFunc);

                if (_acontext != null)
                {
                    var stacks = _acontext.ListStack();
                    var regs = _acontext.ListRegister();
                    if (ImGui.ListBox($"Regs[{regs.Length}]", ref selectedReg, regs, regs.Length))
                    {
                        var obj = _acontext.GetRegister(selectedReg).ToObject();
                        obj_disp = new ObjectDescription(obj, _acontext);
                    }
                    if (ImGui.ListBox($"Stack[{stacks.Length}]", ref selectedStack, stacks, stacks.Length))
                    {
                        var obj = _acontext.GetStackElement(selectedStack).ToObject();
                        obj_disp = new ObjectDescription(obj, _acontext);
                    }
                }

                if (_context != null)
                {
                    var cs = _context.Avm.ListContextStack();
                    var cq = _context.Avm.ListContextQueue();
                    var flag = false;
                    if (ImGui.ListBox($"E.Cont.[{cs.Length}]", ref selected_cstack, cs, cs.Length))
                    {
                        _acontext = _context.Avm.GetStackContext(selected_cstack);
                        flag = true;
                    }
                    if (ImGui.ListBox($"Queue[{cq.Length}]", ref selected_cqueue, cq, cq.Length))
                    {
                        _acontext = _context.Avm.GetQueueContext(selected_cqueue);
                        flag = true;
                    }

                    if (flag)
                    {
                        var str = _acontext!.Stream;
                        var insts = str == null ? null : str.Instructions;
                        manager.CurrentActions = InstUtils.Graphify(insts, _context.AptFile.Constants.Entries);
                        manager.CurrentTitle = $"ActionContext {_acontext}";
                    }

                }
                if (ImGui.Button("This"))
                {
                    obj_disp = _acontext == null ? null : new ObjectDescription(_acontext.This, _acontext) { addr = "this" };
                }
                ImGui.SameLine();
                if (ImGui.Button("Global"))
                {
                    obj_disp = _acontext == null ? null : new ObjectDescription(_acontext.Global, _acontext) { addr = "_global" };
                }
                ImGui.SameLine();
                if (ImGui.Button("Display Item"))
                {
                    obj_disp = _dispobj == null ? null : new ObjectDescription(_dispobj, _acontext) { addr = _dispobj.ToString() };
                }
                ImGui.SameLine();
                if (ImGui.Button("Root"))
                {
                    obj_disp = _context == null ? null : new ObjectDescription(_context.Root.ScriptObject, _acontext) { addr = "_root" };
                }
                ImGui.SameLine();
                if (ImGui.Button("Extern"))
                {
                    obj_disp = _context == null ? null : new ObjectDescription(_context.Avm.ExternObject, _acontext) { addr = "extern" };
                }

                ImGui.NextColumn();

                DrawObject(obj_disp, manager);
               


                if (_instructions != null && ImGui.Begin($"Code: \"{_title}\""))
                {
                    DrawCodes(_instructions);
                }
                ImGui.End();
            }
            ImGui.End();
        }

        internal bool SameLine() { ImGui.SameLine(); return true; }
        internal void DrawObject(ObjectDescription od, LogicalMainForm manager, int layer = 0)
        {
            if (od == null || od.disp == null) return;
            var id = od.GetHashCode();
            using var _ = new ImGuiIDHelper("ObjectPropertyEntry", ref id);
            if (ImGui.ListBox(od.addr, ref od.elem_sel, od.disp, od.disp.Length))
            {
                var od_val = od.tv.GetMember(od.val[od.elem_sel]);
                if (od_val.Type != ValueType.Object)
                    od.obj_str = od_val.ToStringWithType(_acontext);
                od.sub = new ObjectDescription(od_val.Type == ValueType.Object ? od_val.ToObject() : null, od.actx) { addr = od.addr + "\n." + od.val[od.elem_sel] };
            }
            if (od.obj_str != null && od.obj_str != "")
                ImGui.Text($"Selected: {od.obj_str}");
            if (ImGui.Button("__proto__"))
                od.sub = new ObjectDescription(od.obj.__proto__, od.actx) { addr = od.addr + "\n.__proto__" };
            ImGui.SameLine();
            if (ImGui.Button("Keep \"this\""))
                od.sub = new ObjectDescription(od.obj.__proto__, od.actx) { addr = od.addr + "\n.__proto__", tv = od.tv };
            ImGui.SameLine();
            if (ImGui.Button("constructor"))
                od.sub = new ObjectDescription(od.obj.constructor, od.actx) { addr = od.addr + "\n.constructor" };
            if (od.obj is DefinedFunction && SameLine() && ImGui.Button("Code"))
            {
                var dod = od.obj as DefinedFunction;
                _acontext = dod.GetContext(_context.Avm, null, null);
                var reg = dod.IsNewVersion ? LogicalFunctionContext.Preload(dod.Flags, dod.Parameters) : null;
                manager.CurrentActions = InstUtils.Graphify(dod.Instructions, _context.AptFile.Constants.Entries, dod.Constants, reg);
                manager.CurrentTitle = $"Function {dod}";
            }
                
            DrawObject(od.sub, manager, layer + 1);
        }
        internal void DrawCodes(InstructionGraph insts, int indent = 0)
        {
            foreach (var c in insts.Items)
            {
                // TODO Edit
                /*
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
                */
                {
                    var ind = c.Key + insts.IndexOffset;
                    var inst = c.Value;

                    var id = inst.GetHashCode();
                    using var _ = new ImGuiIDHelper("Instructions", ref id);

                    ImGui.Text($" {ind}");
                    ImGui.SameLine(0, 0);
                    ImGui.TextColored(inst.Breakpoint ? _breakColor : _unbreakColor, _breakDesc);
                    ImGui.SameLine(48, indent);
                
                    if (ImGui.Button(inst.ToString(_acontext)))
                    {
                        inst.Breakpoint = !inst.Breakpoint;
                    }

                    if (inst is LogicalFunctionContext lcc)
                    {
                        DrawCodes(lcc.Instructions, indent + 16);
                    }
                }
            }
        }
    }
}