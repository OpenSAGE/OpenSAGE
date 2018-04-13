using System.IO;
using ImGuiNET;

namespace OpenSage.Viewer.UI.Views
{
    internal sealed class AniView : AssetView
    {
        private Cursor _cursor;

        public AniView(AssetViewContext context)
        {
            _cursor = new Cursor(Path.Combine(context.Game.ContentManager.FileSystem.RootDirectory, context.Entry.FilePath));
            context.Game.SetCursor(_cursor);

            AddDisposeAction(() => context.Game.SetCursor("Arrow"));
        }

        public override void Draw(ref bool isGameViewFocused)
        {
            ImGui.Text("Move mouse pointer into the area below to see the cursor.");
        }
    }
}
