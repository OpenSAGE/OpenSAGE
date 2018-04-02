using System;
using System.Numerics;
using ImGuiNET;
using OpenSage.Data;
using Veldrid;

namespace OpenSage.Viewer.UI.Views
{
    internal sealed class TgaView : IView
    {
        private readonly IntPtr _imagePointer;

        public TgaView(
            GraphicsDevice graphicsDevice,
            ImGuiRenderer renderer,
            Game game,
            FileSystemEntry entry)
        {
            var texture = game.ContentManager.Load<Texture>(entry.FilePath);

            _imagePointer = renderer.GetOrCreateImGuiBinding(
                graphicsDevice.ResourceFactory,
                texture);
        }

        public void Draw()
        {
            ImGui.Image(
                _imagePointer,
                ImGui.GetContentRegionAvailable(),
                Vector2.Zero,
                Vector2.One,
                Vector4.One,
                Vector4.Zero);
        }
    }
}
