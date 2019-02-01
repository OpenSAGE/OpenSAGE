using OpenSage.Content;
using OpenSage.Data.Apt;
using OpenSage.Gui.Apt.ActionScript;
using OpenSage.Mathematics;
using Veldrid;
using Rectangle = OpenSage.Mathematics.Rectangle;

namespace OpenSage.Gui.Apt
{
    public sealed class AptWindow : DisposableBase
    {
        private readonly AptContext _context;
        private readonly Game _game;
        private Size _destinationSize;
        private readonly AptCallbackResolver _resolver;

        /// <summary>
        /// The background color of the movie set by the BackgroundColor frameItem
        /// <see cref="Data.Apt.FrameItems.BackgroundColor"/>
        /// </summary>
        private ColorRgbaF _backgroundColor { get; set; }

        public AptFile AptFile { get; }
        public string Name => AptFile.MovieName;
        public AptRenderer Renderer { get; }
        public SpriteItem Root { get; }
        public MappedImageLoader ImageLoader { get; }
        public ContentManager ContentManager { get; }
        public AptInputMessageHandler InputHandler { get; set; }

        /// <summary>
        /// Used for shellmap in MainMenu. Not sure if the correct place.
        /// </summary>
        public MappedImageTexture BackgroundImage { get; set; }

        public AptWindow(Game game, ContentManager contentManager, AptFile aptFile)
        {
            _game = game;
            ContentManager = contentManager;
            AptFile = aptFile;

            //Create our context
            _context = new AptContext(this);

            //First thing to do here is to initialize the display list
            Root = new SpriteItem
            {
                Transform = ItemTransform.None,
                SetBackgroundColor = (c) => _backgroundColor = c
            };
            Root.Create(aptFile.Movie, _context);

            _context.Root = Root;
            _context.Avm.CommandHandler = HandleCommand;

            Renderer = new AptRenderer(contentManager);

            ImageLoader = new MappedImageLoader(contentManager);

            _resolver = new AptCallbackResolver(game);
        }

        internal void Layout(GraphicsDevice gd, in Size windowSize)
        {
            _destinationSize = windowSize;
            Renderer.Resize(_destinationSize);
        }

        internal void Update(TimeInterval gt, GraphicsDevice gd)
        {
            _context.Avm.UpdateIntervals(gt);
            Root.Update(gt);
            Root.RunActions(gt);
        }

        internal void Render(DrawingContext2D drawingContext)
        {
            var fullSizeRect = new Rectangle(Point2D.Zero, _destinationSize);

            if (BackgroundImage != null)
            {
                drawingContext.DrawImage(BackgroundImage.Texture, BackgroundImage.SourceRect, fullSizeRect);
            }

            //The background color, which is set by the APT. Should be the clear color?
            //drawingContext.FillRectangle(fullsizeRect, _backgroundColor);

            var transform = ItemTransform.None;

            Root.Render(Renderer, transform, drawingContext);
        }

        internal void HandleCommand(ActionContext context, string cmd, string param)
        {
            _resolver.GetCallback(cmd).Invoke(param, context, this, _game);
        }

        public delegate void ActionscriptCallback(string param, ActionContext context, AptWindow window, Game game);
    }
}
