using System.Numerics;
using ImGuiNET;
using OpenSage.Graphics;
using OpenSage.Graphics.Rendering;
using OpenSage.Mathematics;
using Veldrid;

namespace OpenSage.Diagnostics
{
    internal sealed class RenderedView : DisposableBase
    {
        private readonly DiagnosticViewContext _context;
        private readonly Scene3D _scene3D;
        private readonly RenderTarget _renderTarget;

        private Vector2 _cachedSize;

        public RenderPipeline RenderPipeline { get; }

        public RenderedView(DiagnosticViewContext context, Scene3D scene3D)
        {
            _context = context;
            _scene3D = scene3D;

            RenderPipeline = AddDisposable(new RenderPipeline(context.Game));

            _renderTarget = AddDisposable(new RenderTarget(context.Game.GraphicsDevice));
        }

        public void Draw()
        {
            var currentSize = ImGui.GetContentRegionAvail();
            if (_cachedSize != currentSize)
            {
                var newSize = new Size((int) currentSize.X, (int) currentSize.Y);
                _renderTarget.EnsureSize(newSize);
                _cachedSize = currentSize;

                _scene3D?.Camera.OnViewportSizeChanged();
            }

            _scene3D.Update(_context.Game.UpdateTime);

            RenderPipeline.Execute(new RenderContext
            {
                ContentManager = _context.Game.ContentManager,
                GameTime = _context.Game.UpdateTime,
                GraphicsDevice = _context.Game.GraphicsDevice,
                RenderTarget = _renderTarget.Framebuffer,
                Scene2D = null,
                Scene3D = _scene3D
            });

            var imagePointer = _context.ImGuiRenderer.GetOrCreateImGuiBinding(
                _context.Game.GraphicsDevice.ResourceFactory,
                _renderTarget.ColorTarget);

            ImGui.Image(
                imagePointer,
                currentSize,
                _context.Game.GetTopLeftUV(),
                _context.Game.GetBottomRightUV());
        }
    }
}
