using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using ImGuiNET;
using OpenSage.Mathematics;

namespace OpenSage.Tools.AptEditor.Util
{
    internal static class ImGuiUtility
    {
        public static bool InputText(string label, byte[] textBuffer, out string result)
        {
            var input = ImGui.InputText(label, textBuffer, (uint) textBuffer.Length, ImGuiInputTextFlags.None);
            result = TrimToNullByte(Encoding.UTF8.GetString(textBuffer));
            return input;
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

        internal sealed class Selectable<T> where T : class
        {
            T selectedItem;

            public void UpdateItem(T item, string name)
            {
                var selected = (item == selectedItem);
                ImGui.Selectable(name, ref selected);
                if(selected)
                {
                    selectedItem = item;
                }
            }

            public void UpdateAll(IEnumerable<T> items, Func<T, string> getName)
            {
                foreach(var item in items)
                {
                    UpdateItem(item, getName(item));
                }
            }
        }

        internal sealed class ModalPopUp
        {
            public string ID;
            public Func<bool> OpenCondition { get; set; }
            public Action DrawAction { get; set; }
            public void Open() => ImGui.OpenPopup(ID);
            public ImGuiWindowFlags Flags { get; set; }
            private bool _open = true;
            public ModalPopUp(string id, Func<bool> openCondition, Action drawAction, ImGuiWindowFlags flags = ImGuiWindowFlags.AlwaysAutoResize)
            {
                ID = id;
                OpenCondition = openCondition;
                DrawAction = drawAction;
                Flags = flags;
            }

            public void Update()
            {
                if(OpenCondition())
                {
                    if(_open == false)
                    {
                        ImGui.OpenPopup(ID);
                        _open = true;
                    }
                }
                else
                {
                    _open = false;
                }

                bool popUpOpen = true;
                if (ImGui.BeginPopupModal(ID, ref popUpOpen, Flags))
                {
                    DrawAction();
                    ImGui.EndPopup();
                }
            }
        }

        public static Vector4 ToVector4(this ColorRgbaF color)
        {
            return new Vector4(color.R, color.B, color.R, color.A);
        }


    }
}
