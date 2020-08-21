using System.Numerics;
using OpenSage.Content;
using OpenSage.Content.Util;
using OpenSage.Data.Wnd;
using OpenSage.Gui.Wnd.Images;
using OpenSage.Mathematics;

namespace OpenSage.Gui.Wnd.Controls
{
    public class Window : Control
    {
        private readonly Size _creationResolution;

        private Matrix3x2 _rootTransform;
        private Matrix3x2 _rootTransformInverse;

        public WindowCallback LayoutInit { get; set; }
        public WindowCallback LayoutUpdate { get; set; }
        public WindowCallback LayoutShutdown { get; set; }

        public Control Root { get; }
        public Game Game { get; set; }
        public ImageLoader ImageLoader { get; }

        public Window(WndFile wndFile, Game game, WndCallbackResolver wndCallbackResolver)
            : this(wndFile.RootWindow.ScreenRect.CreationResolution, game.GraphicsLoadContext)
        {
            Game = game;
            Bounds = wndFile.RootWindow.ScreenRect.ToRectangle();
            LayoutInit = wndCallbackResolver.GetWindowCallback(wndFile.LayoutBlock.LayoutInit);
            LayoutUpdate = wndCallbackResolver.GetWindowCallback(wndFile.LayoutBlock.LayoutUpdate);
            LayoutShutdown = wndCallbackResolver.GetWindowCallback(wndFile.LayoutBlock.LayoutShutdown);

            Root = CreateRecursive(
                wndFile.RootWindow,
                ImageLoader,
                game.ContentManager,
                game.AssetStore,
                wndCallbackResolver,
                wndFile.RootWindow.ScreenRect.UpperLeft);
            Controls.Add(Root);
        }

        public Window(in Size creationResolution, Control root, Game game)
            : this(creationResolution, game.GraphicsLoadContext)
        {
            Root = root;
            Controls.Add(root);
        }

        private Window(in Size creationResolution, GraphicsLoadContext loadContext)
        {
            _creationResolution = creationResolution;

            Window = this;

            var imageTextureCache = AddDisposable(new ImageTextureCache(loadContext));
            ImageLoader = new ImageLoader(imageTextureCache);
        }

        protected override void OnSizeChanged(in Size newSize)
        {
            _rootTransform = RectangleF.CalculateTransformForRectangleFittingAspectRatio(
                Bounds.ToRectangleF(),
                _creationResolution.ToSizeF(),
                newSize);

            Matrix3x2.Invert(_rootTransform, out _rootTransformInverse);
        }

        public override Point2D PointToClient(in Point2D point)
        {
            return Point2D.Transform(point, _rootTransformInverse);
        }

        internal void Update()
        {
            Layout();
        }

        internal void Render(DrawingContext2D drawingContext)
        {
            drawingContext.PushTransform(_rootTransform);

            void drawControlRecursive(Control control)
            {
                if (!control.Visible)
                {
                    return;
                }

                drawingContext.PushTransform(Matrix3x2.CreateTranslation(control.Bounds.X, control.Bounds.Y));

                control.DrawCallback(control, drawingContext);

                // Draw child controls.
                foreach (var child in control.Controls.AsList())
                {
                    drawControlRecursive(child);
                }

                drawingContext.PopTransform();
            }

            drawControlRecursive(Root);

            drawingContext.PopTransform();
        }
    }

    public delegate void WindowCallback(Window window, Game game);
}
