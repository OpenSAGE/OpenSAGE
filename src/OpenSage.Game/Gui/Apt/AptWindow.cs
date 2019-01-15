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
        private readonly Texture _background;
        private Size _destinationSize;

        public AptFile AptFile { get; }
        public string Name => AptFile.MovieName;
        public AptRenderer Renderer { get; }
        public SpriteItem Root { get; }

        public Texture Background { get; set; }

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

            Renderer = new AptRenderer(contentManager);
        }

        internal void Layout(GraphicsDevice gd, in Size windowSize)
        {
            _destinationSize = windowSize;
            Renderer.Resize(_destinationSize);
        }

        internal void Update(GameTime gt, GraphicsDevice gd)
        {
            Root.Update(gt);
            Root.RunActions(gt);
        }

        internal void Render(DrawingContext2D drawingContext)
        {
            var sourceRect = new Rectangle(0, 0,
                (int) Background.Width,
                (int) (Background.Height * 0.75f));

            drawingContext.DrawImage(Background, sourceRect, new Rectangle(Point2D.Zero, _destinationSize));

            var transform = ItemTransform.None;

            Root.Render(Renderer, transform, drawingContext);
        }
    }
}
