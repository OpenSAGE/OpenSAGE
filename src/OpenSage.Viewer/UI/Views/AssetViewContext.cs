using OpenSage.Data;
using OpenSage.Viewer.Framework;
using Veldrid;

namespace OpenSage.Viewer.UI.Views
{
    internal sealed class AssetViewContext
    {
        public Game Game { get; }
        public ImGuiGamePanel GamePanel { get; }
        public GraphicsDevice GraphicsDevice { get; }
        public ImGuiRenderer ImGuiRenderer { get; }
        public FileSystemEntry Entry { get; }

        public AssetViewContext(Game game, ImGuiGamePanel gamePanel, ImGuiRenderer imGuiRenderer, FileSystemEntry entry)
        {
            Game = game;
            GamePanel = gamePanel;
            GraphicsDevice = game.GraphicsDevice;
            ImGuiRenderer = imGuiRenderer;
            Entry = entry;
        }
    }
}
