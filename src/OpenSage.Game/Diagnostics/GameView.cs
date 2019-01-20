using System;
using System.Numerics;
using ImGuiNET;
using OpenSage.Diagnostics.Util;
using OpenSage.Input;
using Veldrid;

namespace OpenSage.Diagnostics
{
    internal sealed class GameView : DiagnosticView
    {
        public override string DisplayName { get; } = "Game View";

        public override bool Closable { get; } = false;

        public GameView(DiagnosticViewContext context)
            : base(context)
        {

        }

        protected override void DrawOverride(ref bool isGameViewFocused)
        {
            var windowPos = ImGui.GetCursorScreenPos();

            var availableSize = ImGui.GetContentRegionAvail();
            availableSize.Y -= ImGui.GetTextLineHeightWithSpacing();

            if (availableSize.X <= 0 || availableSize.Y <= 0)
            {
                return;
            }

            Game.Panel.EnsureFrame(
                new Mathematics.Rectangle(
                    (int) windowPos.X,
                    (int) windowPos.Y,
                    (int) availableSize.X,
                    (int) availableSize.Y));

            var inputMessages = isGameViewFocused
                ? ImGuiUtility.TranslateInputMessages(Game.Panel.Frame, Game.Window.MessageQueue)
                : Array.Empty<InputMessage>();

            Game.Update(inputMessages);
            Game.Render();

            var imagePointer = ImGuiRenderer.GetOrCreateImGuiBinding(
                Game.GraphicsDevice.ResourceFactory,
                Game.Panel.Framebuffer.ColorTargets[0].Target);

            if (ImGui.ImageButton(
                imagePointer,
                availableSize,
                Vector2.Zero,
                Vector2.One,
                0,
                Vector4.Zero,
                Vector4.One))
            {
                isGameViewFocused = true;
            }

            if (isGameViewFocused)
            {
                ImGui.TextColored(
                    new Vector4(1.0f, 0.0f, 0.0f, 1.0f),
                    "Press [ESC] to unfocus the game view.");
            }
            else
            {
                ImGui.Text("Click in the game view to capture mouse input.");
            }
        }
    }
}
