using System.Numerics;
using ImGuiNET;
using Veldrid;

namespace OpenSage.Viewer.UI.Views
{
    internal abstract class GameView : AssetView
    {
        private readonly AssetViewContext _context;

        protected GameView(AssetViewContext context)
        {
            _context = context;
        }

        private Vector2 GetTopLeftUV()
        {
            return _context.GraphicsDevice.IsUvOriginTopLeft ?
                new Vector2(0, 0) :
                new Vector2(0, 1);
        }
        private Vector2 GetBottomRightUV()
        {
            return _context.GraphicsDevice.IsUvOriginTopLeft ?
                new Vector2(1, 1) :
                new Vector2(1, 0);
        }

        public override void Draw(ref bool isGameViewFocused)
        {
            var windowPos = ImGui.GetCursorScreenPos();
            var availableSize = ImGui.GetContentRegionAvail();
            _context.GamePanel.EnsureFrame(
                new Mathematics.Rectangle(
                    (int) windowPos.X,
                    (int) windowPos.Y,
                    (int) availableSize.X,
                    (int) availableSize.Y));

            _context.Game.Tick();

            ImGuiNative.igSetItemAllowOverlap();

            var imagePointer = _context.ImGuiRenderer.GetOrCreateImGuiBinding(
                _context.GraphicsDevice.ResourceFactory,
                _context.Game.Panel.Framebuffer.ColorTargets[0].Target);

            if (ImGui.ImageButton(
                imagePointer,
                ImGui.GetContentRegionAvail(),
                GetTopLeftUV(),
                GetBottomRightUV(),
                0,
                Vector4.Zero,
                Vector4.One))
            {
                isGameViewFocused = true;
            }
        }
    }
}
