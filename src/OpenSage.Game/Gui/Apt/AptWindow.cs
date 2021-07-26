using System;
using System.Numerics;
using OpenSage.Content;
using OpenSage.Data.Apt;
using OpenSage.Data.Apt.Characters;
using OpenSage.Data.Apt.FrameItems;
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
        private readonly AptRenderingContext _renderingContext;
        private Vector2 _movieSize;
        private Vector2 _destinationSize;

        /// <summary>
        /// The background color of the movie set by the BackgroundColor frameItem
        /// <see cref="Data.Apt.FrameItems.BackgroundColor"/>
        /// </summary>
        private ColorRgbaF _backgroundColor { get; set; }
        private ItemTransform _windowTransform = ItemTransform.None;
        public ref ItemTransform WindowTransform => ref _windowTransform;

        public AptFile AptFile { get; }
        public string Name => AptFile.MovieName;
        public SpriteItem Root { get; }
        public ContentManager ContentManager { get; }
        public AptWindowManager Manager { get; set; }
        internal AssetStore AssetStore { get; }
        public AptInputMessageHandler InputHandler { get; set; }
        public AptContext Context => _context;

        // event handler related
        public DisplayItem MouseFocus { get; private set; }
        public DisplayItem MouseFocusOnLastStateChanged { get; private set; }
        public bool MouseState { get; private set; }
        public DisplayItem KeyFocus { get; private set; }

        /// <summary>
        /// Used for shellmap in MainMenu. Not sure if the correct place.
        /// </summary>
        public MappedImage BackgroundImage { get; set; }

        public AptWindow(Game game, ContentManager contentManager, AptFile aptFile)
        {
            _game = game;
            ContentManager = contentManager;
            AssetStore = game.AssetStore;
            AptFile = aptFile;

            //Create our context
            _context = new AptContext(this);
            _context.Avm.SetHandlers(HandleCommand, HandleVariable, HandleMovie);

            //First thing to do here is to initialize the display list
            Root = AddDisposable(new SpriteItem
            {
                Transform = ItemTransform.None,
                SetBackgroundColor = (c) => _backgroundColor = c
            });
            Root.Create(aptFile.Movie, _context);

            _context.Root = Root;

            _context.LoadContext();

            var m = Root.Character as Movie;
            _movieSize = new Vector2(m.ScreenWidth, m.ScreenHeight);

            _renderingContext = AddDisposable(new AptRenderingContext(this, contentManager, game.GraphicsLoadContext, _context));

            _resolver = new AptCallbackResolver(game);
        }

        internal void Layout(GraphicsDevice gd, in Size windowSize)
        {
            _destinationSize = new Vector2(windowSize.Width, windowSize.Height);

        }

        internal bool HandleInput(Point2D mousePos, bool mouseDown)
        {
            var windowScaling = new Vector2(1, 1) / GetScaling();
            var screenPos = Vector2.Transform(mousePos.ToVector2(), Matrix3x2.CreateScale(windowScaling));

            var curFocus = Root.GetMouseFocus(screenPos);

            var state_changed_flag = mouseDown != MouseState;
            var focus_changed_flag = curFocus != MouseFocus;

            // MouseUp, MouseDown, MouseMove
            // Press, Release, ReleaseOutside
            if (state_changed_flag)
            {
                if (Root != null)
                    Root.HandleEventOnTree(mouseDown ? ClipEventFlags.MouseDown : ClipEventFlags.MouseUp);
                if (curFocus != null)
                    curFocus.HandleEvent(mouseDown ? ClipEventFlags.Press : ClipEventFlags.Release);
                if (focus_changed_flag && !mouseDown)
                    if (MouseFocusOnLastStateChanged != null)
                        MouseFocusOnLastStateChanged.HandleEvent(ClipEventFlags.ReleaseOutside);
            }
            else
                if (Root != null)
                    Root.HandleEventOnTree(ClipEventFlags.MouseMove);

            // DragOut, DragOver, RollOut, RollOver
            if (focus_changed_flag)
            {
                if (curFocus != null)
                    curFocus.HandleEvent(mouseDown ? ClipEventFlags.DragOver : ClipEventFlags.RollOver);
                if (MouseFocus != null)
                    MouseFocus.HandleEvent(mouseDown ? ClipEventFlags.DragOut : ClipEventFlags.RollOut);
            }

            // KeyFocus Related
            if (state_changed_flag && mouseDown)
            {
                if (KeyFocus != curFocus)
                {
                    // curfocus handle event onsetfocus
                    // keyfocus handle event onkillfocus
                    KeyFocus = curFocus;
                }
            }

            MouseState = mouseDown;
            MouseFocus = curFocus;
            if (state_changed_flag) MouseFocusOnLastStateChanged = curFocus;

            // TODO should be removed eventually
            return Root.HandleInput(mousePos, mouseDown);
        }

        internal void Update(TimeInterval gt, GraphicsDevice gd)
        {
            var vm = _context.Avm;

            vm.ExecuteUntilEmpty(); // clear remaining codes
            vm.UpdateIntervals(gt);
            Root.Update(gt);
            Root.EnqueueActions(gt);
            vm.ExecuteUntilEmpty();
            
        }

        internal Vector2 GetScaling()
        {
            return _destinationSize / _movieSize;
        }

        internal void Render(DrawingContext2D drawingContext)
        {
            var destinationSize = new Size((int) _destinationSize.X, (int) _destinationSize.Y);
            var fullSizeRect = new Rectangle(Point2D.Zero, destinationSize);

            if (BackgroundImage != null)
            {
                drawingContext.DrawImage(BackgroundImage.Texture.Value, BackgroundImage.Coords, fullSizeRect);
            }

            //The background color, which is set by the APT. Should be the clear color?
            //drawingContext.FillRectangle(fullsizeRect, _backgroundColor);

            _renderingContext.SetWindowSize(destinationSize);
            _renderingContext.SetDrawingContext(drawingContext);
            _renderingContext.PushTransform(WindowTransform);

            Root.Render(_renderingContext);

            _renderingContext.PopTransform();
        }

        internal void HandleCommand(ActionContext context, string cmd, string param)
        {
            _resolver.GetCallback(cmd).Invoke(param, context, this, _game);
        }

        internal Value HandleVariable(string variable)
        {
            //Mostly no idea what those mean, but they are all booleans
            switch (variable)
            {
                case "InGame":
                    return Value.FromBoolean(_game.InGame);
                case "InBetaDemo":
                    return Value.FromBoolean(false);
                case "InDreamMachineDemo":
                    return Value.FromBoolean(false);
                case "PalantirMinLOD":
                    return Value.FromBoolean(false);
                case "MinLOD":
                    return Value.FromBoolean(false);
                case "DoTrace":
                    return Value.FromBoolean(true);
                default:
                    throw new NotImplementedException();
            }
        }

        internal AptFile HandleMovie(string movie)
        {
            var aptFileName = System.IO.Path.ChangeExtension(movie, ".apt");
            var entry = ContentManager.FileSystem.GetFile(aptFileName);
            var aptFile = AptFile.FromFileSystemEntry(entry);

            return aptFile;
        }

        public delegate void ActionscriptCallback(string param, ActionContext context, AptWindow window, Game game);
    }
}
