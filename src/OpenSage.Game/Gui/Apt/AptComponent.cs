using System.Numerics;
using OpenSage.Content;
using OpenSage.Data.Apt;
using OpenSage.Graphics;
using OpenSage.LowLevel.Graphics3D;
using OpenSage.Mathematics;

namespace OpenSage.Gui.Apt
{
    public sealed class AptComponent : EntityComponent
    {
        private Texture _texture;
        private DrawingContext2D _primitiveBatch;
        private AptContext _context;
        private Rectangle _frame;
        private float _scale;

        public AptFile Apt { get; set; }
        public SpriteItem Root { get; private set; }

        public void Initialize(ContentManager contentManager)
        {
            //Create our context
            _context = new AptContext(Apt, contentManager);

            //First thing to do here is to initialize the display list
            Root = new SpriteItem() { Transform = ItemTransform.None };

            Root.Create(Apt.Movie, _context);
            _context.Root = Root;
        }

        public void Layout(GraphicsDevice gd, in Size windowSize)
        {
            //_frame = RectangleF.CalculateRectangleFittingAspectRatio(
            //    new RectangleF(0, 0, 1024, 768),
            //    new SizeF(1024, 768),
            //    windowSize,
            //    out _scale);
            _frame = new Rectangle(0, 0, 1024, 768);

            _texture = Texture.CreateTexture2D(
                gd,
                PixelFormat.Rgba8UNorm,
                _frame.Width,
                _frame.Height,
                TextureBindFlags.ShaderResource | TextureBindFlags.RenderTarget);

            _primitiveBatch = new DrawingContext2D(ContentManager, _texture);
        }

        public void Update(GameTime gt, GraphicsDevice gd)
        {
            _primitiveBatch.Begin(gd.SamplerLinearClamp, ColorRgbaF.Transparent);

            //draw the movieclip.
            //var transform = new ItemTransform(
            //    ColorRgbaF.White,
            //    Matrix3x2.CreateScale(_scale));
            var transform = ItemTransform.None;
            Root.Update(transform, gt, _primitiveBatch);
            Root.RunActions(gt);

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
