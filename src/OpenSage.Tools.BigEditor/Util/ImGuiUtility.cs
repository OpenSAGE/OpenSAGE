using System.Text;
using System.IO;
using ImGuiNET;

namespace OpenSage.Tools.BigEditor.Util
{
    public enum FileBrowserType
    {
        Open = 0,
        Save = 1,
        Export = 2,
        Import = 4,
        ExportToDir = 8,
        ImportFromDir = 16,
    }

    internal static class ImGuiUtility
    {
        public static unsafe bool InputText(string label, byte[] textBuffer, out string result)
        {
            var temp = ImGui.InputText(label, textBuffer, (uint)textBuffer.Length, ImGuiInputTextFlags.None, data => 0);

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

        public static string GetFormatedSize(long size)
        {
            string s = size.ToString();

            if (size / 1024 > 1)
            {
                s = $"{size / 1024}K";
            }
            if (size / 1024 > 1024)
            {
                s = $"{size / 1024 / 1024}M";
            }

            return s;
        }

        public static string NormalizePath(string path)
        {
            string s = path;

            if (Directory.Exists(path) && path.Substring(path.Length - 1).CompareTo($"{Path.DirectorySeparatorChar}") != 0)
            {
                s = $"{path}{Path.DirectorySeparatorChar}";
            }

            return s;
        }
    }
}
