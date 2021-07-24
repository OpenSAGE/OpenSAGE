using ImGuiNET;
using OpenSage.Tools.AptEditor.Apt.Editor;
using OpenSage.Tools.AptEditor.Util;
using OpenSage.Gui.Apt;
using OpenSage.Gui.Apt.ActionScript;
using System.Numerics;

namespace OpenSage.Tools.AptEditor.UI.Widgets
{
    internal class ObjectDescription
    {
        public ObjectContext obj;
        public ObjectContext tv;
        public ActionContext actx;
        public string[] disp;
        public string[] val;
        public string addr;
        public ObjectDescription sub;
        public int elem_sel;

        public ObjectDescription(ObjectContext obj, ActionContext actx = null)
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

        int selected_stack = 0;
        int selected_reg = 0;
        int selected_cstack = 0;
        int selected_cqueue = 0;

        ObjectDescription obj_disp = null;
        int obj_elem_sel = 0;

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
                ImGui.Columns(2);
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
                        last_executed_func = lef == null ? "null" : lef.ToString(_acontext);
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

                ImGui.Text("Last Execution:");
                ImGui.Text(last_executed_func);

                if (_acontext != null)
                {
                    var stacks = _acontext.ListStack();
                    var regs = _acontext.ListRegister();
                    if (ImGui.ListBox($"Regs[{regs.Length}]", ref selected_reg, regs, regs.Length))
                    {
                        var obj = _acontext.GetRegister(selected_reg).ToObject();
                        obj_disp = new ObjectDescription(obj, _acontext);
                    }
                    if (ImGui.ListBox($"Stack[{stacks.Length}]", ref selected_stack, stacks, stacks.Length))
                    {
                        var obj = _acontext.GetStackElement(selected_stack).ToObject();
                        obj_disp = new ObjectDescription(obj, _acontext);
                    }
                }

                if (_context != null)
                {
                    var cs = _context.Avm.ListContextStack();
                    var cq = _context.Avm.ListContextQueue();
                    if (ImGui.ListBox($"E.Cont.[{cs.Length}]", ref selected_cstack, cs, cs.Length))
                    {
                        var str = _context.Avm.GetStackContext(selected_cstack).Stream;
                        manager.CurrentActions = str == null ? null : new LogicalInstructions(str.Instructions);
                    }
                    if (ImGui.ListBox($"Queue[{cq.Length}]", ref selected_cqueue, cq, cq.Length))
                    {
                        var str = _context.Avm.GetQueueContext(selected_cqueue).Stream;
                        manager.CurrentActions = str == null ? null : new LogicalInstructions(str.Instructions);
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

                var od = obj_disp;
                while (od != null && od.disp != null)
                {
                    // ImGui.Text(od.addr);
                    if (ImGui.ListBox(od.addr, ref od.elem_sel, od.disp, od.disp.Length))
                    {
                        od.sub = new ObjectDescription(od.tv.GetMember(od.val[od.elem_sel]).ToObject(), od.actx) { addr = od.addr + "\n." + od.val[od.elem_sel] };
                    }
                    if (ImGui.Button("__proto__"))
                        od.sub = new ObjectDescription(od.obj.__proto__, od.actx) { addr = od.addr + "\n.__proto__" };
                    ImGui.SameLine();
                    if (ImGui.Button("__proto__(keep \"this\")"))
                        od.sub = new ObjectDescription(od.obj.__proto__, od.actx) { addr = od.addr + "\n.__proto__", tv = od.obj };
                    ImGui.SameLine();
                    if (ImGui.Button("constructor"))
                        od.sub = new ObjectDescription(od.obj.constructor, od.actx) { addr = od.addr + "\n.constructor" };
                    if (od.obj is DefinedFunction && SameLine() && ImGui.Button("code"))
                        manager.CurrentActions = new LogicalInstructions(((DefinedFunction) od.obj).Instructions);
                    od = od.sub;
                }
               


                if (_instructions != null && ImGui.Begin("Codes"))
                {
                    DrawCodes(_instructions);
                }
                ImGui.End();
            }
            ImGui.End();
        }

        internal bool SameLine() { ImGui.SameLine(); return true; }

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
