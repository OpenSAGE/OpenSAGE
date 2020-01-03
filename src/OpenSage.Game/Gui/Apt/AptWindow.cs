using System.Numerics;
using OpenSage.Content;
using OpenSage.Data.Apt;
using OpenSage.Data.Apt.Characters;
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
        private readonly AptCallbackResolver _resolver;
        private Vector2 _movieSize;
        private Vector2 _destinationSize;

        /// <summary>
        /// The background color of the movie set by the BackgroundColor frameItem
        /// <see cref="Data.Apt.FrameItems.BackgroundColor"/>
        /// </summary>
        private ColorRgbaF _backgroundColor { get; set; }

        public AptFile AptFile { get; }
        public string Name => AptFile.MovieName;
        public AptRenderer Renderer { get; }
        public SpriteItem Root { get; }
        public ContentManager ContentManager { get; }
        internal AssetStore AssetStore { get; }
        public AptInputMessageHandler InputHandler { get; set; }

        /// <summary>
        /// Used for shellmap in MainMenu. Not sure if the correct place.
        /// </summary>
        public MappedImage BackgroundImage { get; set; }

        internal AptWindow(Game game, ContentManager contentManager, AptFile aptFile)
        {
            _game = game;
            ContentManager = contentManager;
            AssetStore = game.AssetStore;
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

            var m = Root.Character as Movie;
            _movieSize = new Vector2(m.ScreenWidth, m.ScreenHeight);

            Renderer = new AptRenderer(this, contentManager);

            _resolver = new AptCallbackResolver(game);
        }

        internal void Layout(GraphicsDevice gd, in Size windowSize)
        {
            _destinationSize = new Vector2(windowSize.Width, windowSize.Height);
        }

        internal bool HandleInput(Point2D mousePos, bool mouseDown)
        {
            return Root.HandleInput(mousePos, mouseDown);
        }

        internal void Update(TimeInterval gt, GraphicsDevice gd)
        {
            _context.Avm.UpdateIntervals(gt);
            Root.Update(gt);
            Root.RunActions(gt);
        }

        internal Vector2 GetScaling()
        {
            return _destinationSize / _movieSize;
        }

        internal void Render(DrawingContext2D drawingContext)
        {
            var fullSizeRect = new Rectangle(0, 0, (int) _destinationSize.X, (int) _destinationSize.Y);

            if (BackgroundImage != null)
            {
                drawingContext.DrawImage(BackgroundImage.Texture.Value, BackgroundImage.Coords, fullSizeRect);
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
