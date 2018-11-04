using System.Text;
using ImGuiNET;

namespace OpenSage.Viewer.Util
{
    internal static class ImGuiUtility
    {
        public static unsafe bool InputText(string label, byte[] textBuffer, out string result)
        {
            var temp = ImGui.InputText(label, textBuffer, (uint) textBuffer.Length, ImGuiInputTextFlags.None, data => 0);

            result = Encoding.UTF8.GetString(textBuffer).TrimEnd('\0');

            return temp;
        }
    }
}
