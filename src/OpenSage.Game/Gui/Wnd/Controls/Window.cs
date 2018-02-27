using System.Numerics;
using OpenSage.Content;
using OpenSage.Graphics;
using OpenSage.Mathematics;
using Veldrid;

namespace OpenSage.Gui.Wnd.Controls
{
    public class Window : Control
    {
        private readonly Size _creationResolution;
        private readonly ContentManager _contentManager;

        private DrawingContext2D _drawingContext;
        private Texture _texture;

        private Matrix3x2 _rootTransform;
        private Matrix3x2 _rootTransformInverse;

        public WindowCallback LayoutInit { get; set; }
        public WindowCallback LayoutUpdate { get; set; }
        public WindowCallback LayoutShutdown { get; set; }

        public Control Root { get; }

        public Window(in Size creationResolution, Control root, ContentManager contentManager)
        {
            _creationResolution = creationResolution;
            _contentManager = contentManager;

            Window = this;

            Root = root;
            Controls.Add(root);
        }

        protected override void OnSizeChanged(in Size newSize)
        {
            _rootTransform = RectangleF.CalculateTransformForRectangleFittingAspectRatio(
                new RectangleF(Vector2.Zero, _creationResolution.ToSizeF()),
                _creationResolution.ToSizeF(),
                newSize);

            Matrix3x2.Invert(_rootTransform, out _rootTransformInverse);

            RemoveAndDispose(ref _drawingContext);
            RemoveAndDispose(ref _texture);

            _texture = AddDisposable(_contentManager.GraphicsDevice.ResourceFactory.CreateTexture(
                TextureDescription.Texture2D(
                    (uint) newSize.Width,
                    (uint) newSize.Height,
                    1,
                    1,
                    PixelFormat.R8_G8_B8_A8_UNorm,
                    TextureUsage.Sampled | TextureUsage.RenderTarget)));

            _drawingContext = AddDisposable(new DrawingContext2D(_contentManager, _texture));
        }

        public override Point2D PointToClient(in Point2D point)
        {
            return Point2D.Transform(point, _rootTransformInverse);
        }

        internal void UpdateTexture()
        {
            Layout();

            _drawingContext.Begin(_contentManager.LinearClampSampler, ColorRgbaF.Transparent);

            _drawingContext.PushTransform(_rootTransform);

            void drawControlRecursive(Control control)
            {
                if (!control.Visible)
                {
                    return;
                }

                control.DrawCallback(control, _drawingContext);

                // Draw child controls.
                foreach (var child in control.Controls)
                {
                    _drawingContext.PushTransform(Matrix3x2.CreateTranslation(child.Bounds.X, child.Bounds.Y));

                    drawControlRecursive(child);

                    _drawingContext.PopTransform();
                }
            }

            drawControlRecursive(Root);

            _drawingContext.PopTransform();

            _drawingContext.End();
        }

        internal void Render(SpriteBatch spriteBatch)
        {
            spriteBatch.DrawImage(
                _texture,
                null,
                new RectangleF(0, 0, _texture.Width, _texture.Height),
                ColorRgbaF.White);
        }
    }

    public delegate void WindowCallback(Window window, Game game);
}
