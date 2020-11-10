using System;
using System.Text;
using ImGuiNET;

namespace OpenSage.Tools.AptEditor.Util
{
    internal class ImGuiTextBox
    {
        public byte[] InputBuffer { get; }
        public string? Hint { get; set; }
        public bool HintValid { get; set; } = true;

        public ImGuiTextBox(int bufferSize = 64)
        {
            InputBuffer = new byte[bufferSize];
        }

        public bool InputText(string label, out string result)
        {
            if (HintValid && Hint != null)
            {
                var bytesWritten = Encoding.UTF8.GetBytes(Hint, InputBuffer);
                if (bytesWritten < InputBuffer.Length)
                {
                    InputBuffer[bytesWritten] = 0;
                }
            }
            var input = ImGui.InputText(label, InputBuffer, (uint) InputBuffer.Length, ImGuiInputTextFlags.None);
            var nulIndex = Array.IndexOf<byte>(InputBuffer, 0);
            result = nulIndex switch
            {
                0 => string.Empty,
                -1 => Encoding.UTF8.GetString(InputBuffer),
                _ => Encoding.UTF8.GetString(InputBuffer, 0, nulIndex)
            };
            HintValid = result == Hint;
            return input;
        }
    }
}
