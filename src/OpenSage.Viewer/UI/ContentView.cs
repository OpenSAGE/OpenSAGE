using System.IO;
using ImGuiNET;
using OpenSage.Data;
using OpenSage.Viewer.UI.Views;
using Veldrid;

namespace OpenSage.Viewer.UI
{
    internal sealed class ContentView : DisposableBase
    {
        private readonly FileSystemEntry _entry;

        private readonly IView _view;

        public ContentView(
            GraphicsDevice graphicsDevice,
            ImGuiRenderer renderer,
            Game game,
            FileSystemEntry entry)
        {
            _entry = entry;

            _view = CreateViewForFileSystemEntry(
                graphicsDevice,
                renderer,
                game,
                entry);
        }

        private IView CreateViewForFileSystemEntry(
            GraphicsDevice graphicsDevice,
            ImGuiRenderer renderer,
            Game game,
            FileSystemEntry entry)
        {
            switch (Path.GetExtension(entry.FilePath).ToLower())
            {
                case ".tga":
                    return new TgaView(
                        graphicsDevice,
                        renderer,
                        game,
                        entry);

                default:
                    return null;
            }
        }

        public void Draw()
        {
            ImGui.Text(_entry.FilePath);

            if (_view != null)
            {
                _view.Draw();
            }
            else
            {
                // TODO
            }
        }
    }
}
