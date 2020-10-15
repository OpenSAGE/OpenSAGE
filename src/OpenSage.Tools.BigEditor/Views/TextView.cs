using System.IO;
using ImGuiNET;

namespace OpenSage.Tools.BigEditor.Views
{
    class TextView : View
    {
        private string _fileText;

        public TextView(Stream stream)
        {
            using (var reader = new StreamReader(stream))
            {
                _fileText = reader.ReadToEnd();
            }
        }

        public override void Draw()
        {
            ImGui.TextUnformatted(_fileText);

            if (ImGui.BeginPopupContextItem())
            {
                if (ImGui.Selectable("Copy to clipboard"))
                {
                    ImGui.SetClipboardText(_fileText);
                }
                ImGui.EndPopup();
            }
        }
    }
}
