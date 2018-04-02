using ImGuiNET;
using OpenSage.Data;

namespace OpenSage.Viewer.UI
{
    internal sealed class ContentView : DisposableBase
    {
        private readonly FileSystemEntry _entry;

        public ContentView(FileSystemEntry entry)
        {
            _entry = entry;
        }

        public void Draw()
        {
            ImGui.Text(_entry.FilePath);
        }
    }
}
