using OpenSage.Content;
using OpenSage.Data.Apt;
using OpenSage.Mathematics;
using Veldrid;
using Rectangle = OpenSage.Mathematics.Rectangle;

namespace OpenSage.Gui.Apt
{
    public sealed class AptWindow : DisposableBase
    {
        private readonly ContentManager _contentManager;
        private readonly AptContext _context;

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
        }

        internal void Update(GameTime gt, GraphicsDevice gd)
        {
            Root.Update(gt);
            Root.RunActions(gt);
        }

        internal void Render(DrawingContext2D drawingContext)
        {
            //draw the movieclip.
            //var transform = new ItemTransform(
            //    ColorRgbaF.White,
            //    Matrix3x2.CreateScale(_scale),
            //    Vector2.Zero);
            var transform = ItemTransform.None;

            Root.Render(transform, drawingContext);
        }
    }
}
