using System.Numerics;
using ImGuiNET;

namespace OpenSage.Viewer.UI.Views
{
    internal abstract class GameView : AssetView
    {
        private readonly AssetViewContext _context;

        protected GameView(AssetViewContext context)
        {
            _context = context;
        }

        public override void Draw()
        {
            _context.Game.Tick();

            var imagePointer = _context.ImGuiRenderer.GetOrCreateImGuiBinding(
                _context.GraphicsDevice.ResourceFactory,
                _context.Game.Panel.Framebuffer.ColorTargets[0].Target);

            ImGui.Image(
                imagePointer,
                ImGui.GetContentRegionAvailable(),
                Vector2.Zero,
                Vector2.One,
                Vector4.One,
                Vector4.Zero);
        }
    }
}
