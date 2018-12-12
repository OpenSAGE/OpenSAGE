using System.Numerics;
using ImGuiNET;
using OpenSage.Gui.Wnd.Controls;
using OpenSage.Mathematics;
using OpenSage.Viewer.Util;

namespace OpenSage.Viewer.UI.Views
{
    internal sealed class WndView : GameView
    {
        private readonly Control _rootControl;

        private Control _selectedControl;
        private ColorRgbaF _originalBorderColor;
        private int _originalBorderWidth;

        public WndView(AssetViewContext context)
            : base(context)
        {
            var window = context.Game.ContentManager.Load<Window>(context.Entry.FilePath, new Content.LoadOptions { CacheAsset = false });
            context.Game.Scene2D.WndWindowManager.PushWindow(window);

            _rootControl = window.Root;

            AddDisposeAction(() => context.Game.Scene2D.WndWindowManager.PopWindow());
        }

        public override void Draw(ref bool isGameViewFocused)
        {
            ImGui.BeginChild("wnd tree", new Vector2(400, 0), true, 0);

            DrawControlTreeItemRecursive(_rootControl);

            ImGui.EndChild();

            ImGui.SameLine();

            base.Draw(ref isGameViewFocused);
        }

        private void DrawControlTreeItemRecursive(Control control)
        {
            var opened = ImGui.TreeNodeEx(control.DisplayName, ImGuiTreeNodeFlags.DefaultOpen | ImGuiTreeNodeFlags.OpenOnDoubleClick);
            ImGuiUtility.DisplayTooltipOnHover(control.DisplayName);

            if (ImGuiNative.igIsItemClicked(0)>0)
            {
                SelectControl(control);
            }

            if (opened)
            {
                foreach (var child in control.Controls)
                {
                    DrawControlTreeItemRecursive(child);
                }

                ImGui.TreePop();
            }
        }

        private void SelectControl(Control control)
        {
            if (_selectedControl != null)
            {
                _selectedControl.BorderColor = _originalBorderColor;
                _selectedControl.BorderWidth = _originalBorderWidth;
                _selectedControl = null;
            }

            _selectedControl = control;

            _originalBorderColor = _selectedControl.BorderColor;
            _originalBorderWidth = _selectedControl.BorderWidth;

            _selectedControl.BorderColor = new ColorRgbaF(1f, 0.41f, 0.71f, 1);
            _selectedControl.BorderWidth = 4;
        }
    }
}
