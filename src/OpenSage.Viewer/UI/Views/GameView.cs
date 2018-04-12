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

        public override void Draw(ref bool isGameViewFocused)
        {
            _context.Game.Tick();

            ImGuiNative.igSetItemAllowOverlap();

            var imagePointer = _context.ImGuiRenderer.GetOrCreateImGuiBinding(
                _context.GraphicsDevice.ResourceFactory,
                _context.Game.Panel.Framebuffer.ColorTargets[0].Target);

            if (ImGui.ImageButton(
                imagePointer,
                ImGui.GetContentRegionAvailable(),
                Vector2.Zero,
                Vector2.One,
                0,
                Vector4.Zero,
                Vector4.One))
            {
                isGameViewFocused = true;
            }
        }
    }
}
