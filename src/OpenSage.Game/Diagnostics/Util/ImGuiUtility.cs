using System.Collections.Generic;
using System.Text;
using ImGuiNET;
using OpenSage.Input;
using OpenSage.Mathematics;

namespace OpenSage.Diagnostics.Util
{
    internal static class ImGuiUtility
    {
        public static unsafe bool InputText(string label, byte[] textBuffer, out string result)
        {
            var temp = ImGui.InputText(label, textBuffer, (uint) textBuffer.Length, ImGuiInputTextFlags.None, data => 0);

            result = Encoding.UTF8.GetString(textBuffer).TrimEnd('\0');

            return temp;
        }

        /// <summary>
        /// Trims a string to only contain the data before the first null terminator.
        /// This is necessary because imgui optimizes input clearing by replacing the first character with a zero byte.
        /// </summary>
        public static string TrimToNullByte(string input)
        {
            if (input == null)
            {
                return null;
            }

            var nullIndex = input.IndexOf('\0');

            return nullIndex >= 0 ? input.Substring(0, nullIndex) : input;
        }

        // Displays a tooltip with specified text when mouse is hovered on the last item.
        public static void DisplayTooltipOnHover(string text)
        {
            if (ImGui.IsItemHovered())
            {
                ImGui.BeginTooltip();
                ImGui.SetTooltip(text);
                ImGui.EndTooltip();
            }
        }

        public static IEnumerable<InputMessage> TranslateInputMessages(
            Rectangle panelFrame,
            IEnumerable<InputMessage> inputMessages)
        {
            foreach (var message in inputMessages)
            {
                Point2D getPositionInPanel()
                {
                    var pos = message.Value.MousePosition;
                    pos = new Point2D(pos.X - panelFrame.X, pos.Y - panelFrame.Y);
                    pos = new Point2D(
                        MathUtility.Clamp(pos.X, 0, panelFrame.Width),
                        MathUtility.Clamp(pos.Y, 0, panelFrame.Height));
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
