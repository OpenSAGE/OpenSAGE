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

        private AptRenderer _renderer;
        //private float _scale;

        public AptFile AptFile { get; }

        public string Name => AptFile.MovieName;

        public AptRenderer Renderer => _renderer;

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

            AptFile = aptFile;

            _renderer = new AptRenderer(contentManager);
        }

        internal void Layout(GraphicsDevice gd, in Size windowSize)
        {
            _renderer.Resize(windowSize);
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

            Root.Render(_renderer, transform, drawingContext);
        }
    }
}
