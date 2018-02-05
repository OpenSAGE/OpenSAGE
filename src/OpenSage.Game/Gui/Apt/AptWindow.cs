using OpenSage.Content;
using OpenSage.Data.Apt;
using OpenSage.Graphics;
using OpenSage.Mathematics;
using Veldrid;

namespace OpenSage.Gui.Apt
{
    public sealed class AptWindow : DisposableBase
    {
        private readonly ContentManager _contentManager;
        private readonly AptContext _context;

        private Texture _texture;
        private DrawingContext2D _primitiveBatch;
        private Rectangle _frame;
        //private float _scale;

        public SpriteItem Root { get; }

        public AptWindow(ContentManager contentManager, AptFile aptFile)
        {
            _contentManager = contentManager;

            //Create our context
            _context = new AptContext(aptFile, contentManager);

            //First thing to do here is to initialize the display list
            Root = new SpriteItem { Transform = ItemTransform.None };

            Root.Create(aptFile.Movie, _context);
            _context.Root = Root;
        }

        internal void Layout(GraphicsDevice gd, in Size windowSize)
        {
            //_frame = RectangleF.CalculateRectangleFittingAspectRatio(
            //    new RectangleF(0, 0, 1024, 768),
            //    new SizeF(1024, 768),
            //    windowSize,
            //    out _scale);
            _frame = new Rectangle(0, 0, 1024, 768);

            _texture = gd.ResourceFactory.CreateTexture(
                TextureDescription.Texture2D(
                    (uint) _frame.Width,
                    (uint) _frame.Height,
                    1,
                    1,
                    PixelFormat.R8_G8_B8_A8_UNorm,
                    TextureUsage.Sampled | TextureUsage.RenderTarget));

            _primitiveBatch = new DrawingContext2D(_contentManager, _texture);
        }

        internal void Update(GameTime gt, GraphicsDevice gd)
        {
            _primitiveBatch.Begin(
                _contentManager.LinearClampSampler,
                ColorRgbaF.Transparent);

            //draw the movieclip.
            //var transform = new ItemTransform(
            //    ColorRgbaF.White,
            //    Matrix3x2.CreateScale(_scale),
            //    Vector2.Zero);
            var transform = ItemTransform.None;

            Root.Update(gt);
            Root.RunActions(gt);
            Root.Render(transform, _primitiveBatch);

            _primitiveBatch.End();
        }

        internal void Render(SpriteBatch spriteBatch)
        {
            spriteBatch.DrawImage(
                _texture,
                null,
                _frame.ToRectangleF(),
                ColorRgbaF.White);
        }
    }
}
