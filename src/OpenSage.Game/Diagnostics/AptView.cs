using System.Numerics;
using ImGuiNET;
using OpenSage.Data.Apt.Characters;
using OpenSage.Diagnostics.Util;
using OpenSage.Gui.Apt;
using OpenSage.Gui.Apt.ActionScript;

namespace OpenSage.Diagnostics
{
    internal sealed class AptView : DiagnosticView
    {
        public override string DisplayName { get; } = "APT Windows";
        private DisplayItem _selectedItem;


        public AptView(DiagnosticViewContext context)
            : base(context)
        {

        }

        private object CreateObject(Value value)
        {
            switch (value.Type)
            {
                case ValueType.String:
                    return value.ToString();
                case ValueType.Boolean:
                    return value.ToBoolean();
                case ValueType.Short:
                case ValueType.Integer:
                    return value.ToInteger();
                case ValueType.Float:
                    return value.ToFloat();
                case ValueType.Object:
                    return "[OBJECT]";
                case ValueType.Function:
                    return "[FUNCTION]";
                case ValueType.Array:
                    return "[ARRAY]";
                case ValueType.Undefined:
                    return "[UNDEFINED]";
            }

            return null;
        }

        protected override void DrawOverride(ref bool isGameViewFocused)
        {
            foreach (var aptWindow in Game.Scene2D.AptWindowManager.WindowStack)
            {
                ImGui.BeginChild("DisplayLists", new Vector2(400, 0), true, ImGuiWindowFlags.HorizontalScrollbar);

                if (ImGui.TreeNodeEx(aptWindow.Name, ImGuiTreeNodeFlags.DefaultOpen | ImGuiTreeNodeFlags.OpenOnArrow))
                {
                    DrawDisplayListRecursive(0, aptWindow.Root);
                }
                ImGui.EndChild();

                ImGui.SameLine();
                ImGui.BeginChild("ScriptObject");
                if (_selectedItem != null)
                {
                    if(_selectedItem is SpriteItem)
                    {
                        var spriteItem = _selectedItem as SpriteItem;
                        ImGuiUtility.BeginPropertyList();
                        ImGuiUtility.PropertyRow("CurrentFrame", spriteItem.CurrentFrame);
                        ImGuiUtility.PropertyRow("State", spriteItem.State);
                        ImGuiUtility.EndPropertyList();

                        if (ImGui.CollapsingHeader("FrameLabels", ImGuiTreeNodeFlags.DefaultOpen))
                        {
                            ImGuiUtility.BeginPropertyList();
                            foreach (var frameLabels in spriteItem.FrameLabels)
                            {
                                ImGuiUtility.PropertyRow(frameLabels.Key, frameLabels.Value);
                            }
                            ImGuiUtility.EndPropertyList();
                        }
                    }

                    if (ImGui.CollapsingHeader("Variables", ImGuiTreeNodeFlags.DefaultOpen))
                    {
                        ImGuiUtility.BeginPropertyList();
                        foreach (var variable in _selectedItem.ScriptObject?.Variables)
                        {
                            ImGuiUtility.PropertyRow(variable.Key, CreateObject(variable.Value));
                        }
                        ImGuiUtility.EndPropertyList();
                    }

                    if (ImGui.CollapsingHeader("Constants", ImGuiTreeNodeFlags.DefaultOpen))
                    {
                        ImGuiUtility.BeginPropertyList();
                        int index = 0;
                        foreach (var variable in _selectedItem.ScriptObject?.Constants)
                        {
                            ImGuiUtility.PropertyRow($"{index++}", CreateObject(variable));
                        }
                        ImGuiUtility.EndPropertyList();
                    }
                }
                ImGui.EndChild();
            }
        }

        private void DrawDisplayListRecursive(int depth, DisplayItem item)
        {
            var treeNodeFlags = ImGuiTreeNodeFlags.DefaultOpen | ImGuiTreeNodeFlags.OpenOnDoubleClick;

            if (!(item is SpriteItem) || ((SpriteItem) item).Content.Items.Count == 0)
            {
                treeNodeFlags = ImGuiTreeNodeFlags.Leaf;
            }

            if (item == _selectedItem)
            {
                treeNodeFlags |= ImGuiTreeNodeFlags.Selected;
            }

            var opened = ImGui.TreeNodeEx($"[{depth}] {item.Name}", treeNodeFlags);
            ImGuiUtility.DisplayTooltipOnHover("MovieClip: " + item.Character.Container.MovieName);

            if (ImGuiNative.igIsItemClicked(0) > 0)
            {
                SelectDisplayItem(item);
            }

            if (opened)
            {
                if (item is SpriteItem)
                {
                    var spriteItem = item as SpriteItem;
                    foreach (var pair in spriteItem.Content.Items)
                    {
                        DrawDisplayListRecursive(pair.Key, pair.Value);
                    }
                }

                ImGui.TreePop();
            }
        }

        private void SelectDisplayItem(DisplayItem item)
        {
            if (_selectedItem != null)
            {
                _selectedItem = null;
            }
            _selectedItem = item;
            Context.SelectedAptWindow = _selectedItem.Context.Window;
        }
    }
}
