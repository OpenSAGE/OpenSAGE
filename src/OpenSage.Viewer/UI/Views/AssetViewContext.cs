using OpenSage.Data;
using Veldrid;

namespace OpenSage.Viewer.UI.Views
{
    internal sealed class AssetViewContext
    {
        public Game Game { get; }
        public GraphicsDevice GraphicsDevice { get; }
        public ImGuiRenderer ImGuiRenderer { get; }
        public FileSystemEntry Entry { get; }

        public AssetViewContext(Game game, ImGuiRenderer imGuiRenderer, FileSystemEntry entry)
        {
            Game = game;
            GraphicsDevice = game.GraphicsDevice;
            ImGuiRenderer = imGuiRenderer;
            Entry = entry;
        }
    }
}
