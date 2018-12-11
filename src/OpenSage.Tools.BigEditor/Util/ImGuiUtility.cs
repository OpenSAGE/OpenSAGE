using System.Text;
using ImGuiNET;

namespace OpenSage.Tools.BigEditor.Util
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
    }
}
