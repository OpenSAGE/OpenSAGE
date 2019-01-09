﻿using System.Numerics;
using ImGuiNET;

namespace OpenSage.Viewer.UI.Views
{
    internal abstract class GameView : AssetView
    {
        protected AssetViewContext Context { get; }

        protected GameView(AssetViewContext context)
        {
            Context = context;
        }

        protected Vector2 GetTopLeftUV()
        {
            return Context.GraphicsDevice.IsUvOriginTopLeft ?
                new Vector2(0, 0) :
                new Vector2(0, 1);
        }

        protected Vector2 GetBottomRightUV()
        {
            return Context.GraphicsDevice.IsUvOriginTopLeft ?
                new Vector2(1, 1) :
                new Vector2(1, 0);
        }

        public override void Draw(ref bool isGameViewFocused)
        {
            var windowPos = ImGui.GetCursorScreenPos();
            var availableSize = ImGui.GetContentRegionAvail();

            // If there's not enough space for the game view, don't draw anything.
            if (availableSize.X <= 0 || availableSize.Y <= 0)
            {
                return;
            }

            Context.GamePanel.EnsureFrame(
                new Mathematics.Rectangle(
                    (int) windowPos.X,
                    (int) windowPos.Y,
                    (int) availableSize.X,
                    (int) availableSize.Y));

            Context.Game.Tick(Context.GamePanel.Framebuffer);

            ImGuiNative.igSetItemAllowOverlap();

            var imagePointer = Context.ImGuiRenderer.GetOrCreateImGuiBinding(
                Context.GraphicsDevice.ResourceFactory,
                Context.Game.Panel.Framebuffer.ColorTargets[0].Target);

            if (ImGui.ImageButton(
                imagePointer,
                ImGui.GetContentRegionAvail(),
                GetTopLeftUV(),
                GetBottomRightUV(),
                0,
                Vector4.Zero,
                Vector4.One))
            {
                isGameViewFocused = true;
            }
            ImGui.SetCursorScreenPos(windowPos + new Vector2(1.0f));
            ImGui.TextColored(Vector4.UnitW, $"{ImGui.GetIO().Framerate:N2} FPS");
            ImGui.SetCursorScreenPos(windowPos);
            ImGui.Text($"{ImGui.GetIO().Framerate:N2} FPS");
        }
    }
}
