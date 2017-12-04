using LL.Graphics2D;
using LL.Graphics3D;
using OpenSage.Mathematics;

namespace OpenSage.Gui.Elements
{
    public abstract class UIElement : DisposableBase
    {
        private bool _needsRender = true;

        private Rectangle _frame;
        public Rectangle Frame
        {
            get => _frame;
            set
            {
                var needsNewRenderTarget =
                    _frame.Width != value.Width
                    || _frame.Height != value.Height;

                _frame = value;

                if (needsNewRenderTarget)
                {
                    InvalidateRenderTarget();
                }
            }
        }

        public ColorRgbaF BackgroundColor { get; set; }
        public ColorRgbaF BorderColor { get; set; }

        public UIElementCollection Children { get; } = new UIElementCollection();

        private LL.Graphics2D.RenderTarget _renderTarget;
        public Texture Texture => _renderTarget.Texture;

        private void InvalidateRenderTarget()
        {
            RemoveAndDispose(ref _renderTarget);
            _needsRender = true;
        }

        protected void Invalidate()
        {
            _needsRender = true;
        }

        public void Render(RenderContext context)
        {
            if (!_needsRender)
            {
                return;
            }

            if (_renderTarget == null)
            {
                _renderTarget = AddDisposable(new LL.Graphics2D.RenderTarget(
                    context.GraphicsDevice,
                    context.GraphicsDevice2D,
                    Frame.Width,
                    Frame.Height));
            }

            var drawingContext = _renderTarget.Open();

            drawingContext.Clear(BackgroundColor);

            OnRender(drawingContext);

            drawingContext.Close();

            _needsRender = false;
        }

        protected abstract void OnRender(DrawingContext drawingContext);
    }
}
