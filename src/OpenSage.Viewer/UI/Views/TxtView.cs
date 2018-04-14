using System.IO;
using ImGuiNET;

namespace OpenSage.Viewer.UI.Views
{
    internal sealed class TxtView : AssetView
    {
        private readonly string _text;

        public TxtView(AssetViewContext context)
        {
            using (var fileStream = context.Entry.Open())
            using (var streamReader = new StreamReader(fileStream))
            {
                _text = streamReader.ReadToEnd();
            }
        }

        public override void Draw(ref bool isGameViewFocused)
        {
            ImGui.TextWrapped(_text);
        }
    }
}
