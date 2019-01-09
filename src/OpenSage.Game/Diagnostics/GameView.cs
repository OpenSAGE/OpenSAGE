using System.Collections.Generic;
using System.Numerics;
using ImGuiNET;
using OpenSage.Input;
using OpenSage.Mathematics;
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

            Game.Panel.EnsureFrame(
                new Mathematics.Rectangle(
                    (int) windowPos.X,
                    (int) windowPos.Y,
                    (int) availableSize.X,
                    (int) availableSize.Y));

            var inputMessages = TranslateInputMessages(Game.Window.MessageQueue);
            Game.Tick(inputMessages);

            var imagePointer = ImGuiRenderer.GetOrCreateImGuiBinding(
                Game.GraphicsDevice.ResourceFactory,
                Game.Panel.Framebuffer.ColorTargets[0].Target);

            if (ImGui.ImageButton(
                imagePointer,
                availableSize,
                GetTopLeftUV(),
                GetBottomRightUV(),
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

        private IEnumerable<InputMessage> TranslateInputMessages(IEnumerable<InputMessage> inputMessages)
        {
            foreach (var message in inputMessages)
            {
                var windowPos = Game.Panel.Frame.Location;

                Point2D getPositionInPanel()
                {
                    var pos = message.Value.MousePosition;
                    pos = new Point2D(pos.X - windowPos.X, pos.Y - windowPos.Y);
                    return pos;
                }

                var translatedMessage = message;

                switch (message.MessageType)
                {
                    case InputMessageType.MouseLeftButtonDown:
                    case InputMessageType.MouseLeftButtonUp:
                    case InputMessageType.MouseMiddleButtonDown:
                    case InputMessageType.MouseMiddleButtonUp:
                    case InputMessageType.MouseRightButtonDown:
                    case InputMessageType.MouseRightButtonUp:
                        translatedMessage = InputMessage.CreateMouseButton(
                            message.MessageType,
                            getPositionInPanel());
                        break;

                    case InputMessageType.MouseMove:
                        translatedMessage = InputMessage.CreateMouseMove(
                            getPositionInPanel());
                        break;
                }

                yield return translatedMessage;
            }
        }
    }
}
