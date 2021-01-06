using System;
using ImGuiNET;

namespace OpenSage.Tools.AptEditor.Util
{
    internal sealed class ImGuiModalPopUp
    {
        public string ID;
        public Func<bool> OpenCondition { get; set; }
        public Action DrawAction { get; set; }
        public void Open() => ImGui.OpenPopup(ID);
        public ImGuiWindowFlags Flags { get; set; }

        public ImGuiModalPopUp(string id, Func<bool> openCondition, Action drawAction, ImGuiWindowFlags flags = ImGuiWindowFlags.AlwaysAutoResize)
        {
            ID = id;
            OpenCondition = openCondition;
            DrawAction = drawAction;
            Flags = flags;
        }

        public bool Update()
        {
            if (OpenCondition())
            {
                if (!ImGui.IsPopupOpen(ID))
                {
                    ImGui.OpenPopup(ID);
                }
            }

            var popUpOpen = true;
            if (ImGui.BeginPopupModal(ID, ref popUpOpen, Flags))
            {
                DrawAction();
                ImGui.EndPopup();
                return true;
            }
            return false;
        }
    }

    internal static partial class ImGuiUtility
    {
        /*
        internal sealed class Selectable<T> where T : class
        {
            T _selectedItem;

            public void UpdateItem(T item, string name)
            {
                var selected = (item == _selectedItem);
                ImGui.Selectable(name, ref selected);
                if(selected)
                {
                    _selectedItem = item;
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
        */

        
    }
}
