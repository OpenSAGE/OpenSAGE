using System;
using System.Collections.Generic;
using System.Linq;
using OpenSage.Content;
using OpenSage.Data.Wnd;
using OpenSage.Graphics;
using OpenSage.Mathematics;
using Veldrid;
using Rectangle = OpenSage.Mathematics.Rectangle;

namespace OpenSage.Gui.Wnd
{
    public partial class WndWindow : DisposableBase
    {
        private readonly WndWindowDefinition _wndWindow;
        private readonly ContentManager _contentManager;

        private Texture _texture;
        private DrawingContext2D _primitiveBatch;

        private readonly Dictionary<WndWindowState, WndWindowStateConfiguration> _stateConfigurations;

        internal DrawingContext2D PrimitiveBatch => _primitiveBatch;

        private WndWindowState _currentState;
        internal WndWindowState CurrentState
        {
            get => _currentState;
            set
            {
                _currentState = value;
                Invalidate();
            }
        }

        private float _scale;

        protected float Scale => _scale;

        public WndFont TextFont { get; }

        internal SixLabors.Fonts.Font GetFont() =>
            _contentManager.GetOrCreateFont(
                TextFont.Name,
                TextFont.Size * _scale,
                TextFont.Bold ? FontWeight.Bold : FontWeight.Normal);

        internal ColorRgbaF GetTextColor()
        {
            var textColor = ActiveState.TextColor.ToColorRgbaF();
            textColor.A *= TextOpacity;
            return textColor;
        }

        private string _text;
        public string Text
        {
            get => _text;
            set
            {
                _text = value;
                Invalidate();
            }
        }

        private TextAlignment _textAlignment;
        public TextAlignment TextAlignment
        {
            get => _textAlignment;
            set
            {
                _textAlignment = value;
                Invalidate();
            }
        }

        internal bool IsInvalidated { get; set; }

        public WndWindowDefinition Definition => _wndWindow;

        public Rectangle Frame { get; private set; }

        public Rectangle Bounds => new Rectangle(0, 0, Frame.Width, Frame.Height);

        public string Name => _wndWindow.Name;

        public string DisplayName => $"{Name} ({_wndWindow.WindowType}, {(Visible ? "Visible" : "Hidden")})";

        public bool Visible { get; private set; }

        public WndWindow Parent { get; internal set; }

        public WndTopLevelWindow Window { get; internal set; }

        public WndWindowCollection Children { get; } = new WndWindowCollection();

        public UIElementCallback SystemCallback { get; private set; }
        internal UIElementCallback InputCallback { get; private set; }
        internal UIElementCallback TooltipCallback { get; private set; }
        public UIElementDrawCallback DrawCallback { get; set; }

        private ColorRgba? _backgroundColorOverride;
        public ColorRgba? BackgroundColorOverride
        {
            get => _backgroundColorOverride;
            set
            {
                _backgroundColorOverride = value;
                Invalidate();
            }
        }

        private ColorRgba? _overlayColorOverride;
        public ColorRgba? OverlayColorOverride
        {
            get => _overlayColorOverride;
            set
            {
                _overlayColorOverride = value;
                Invalidate();
            }
        }

        public float Opacity { get; set; } = 1;

        private float _textOpacity = 1;
        public float TextOpacity
        {
            get => _textOpacity;
            set
            {
                _textOpacity = value;
                Invalidate();
            }
        }

        private bool _highlighted;
        public bool Highlighted
        {
            get => _highlighted;
            set
            {
                _highlighted = value;
                Invalidate();
            }
        }

        internal WndWindowStateConfiguration ActiveState => _stateConfigurations[_currentState];

        public static WndWindow Create(WndWindowDefinition wndWindow, ContentManager contentManager, WndCallbackResolver callbackResolver)
        {
            switch (wndWindow.WindowType)
            {
                case WndWindowType.GenericWindow:
                    return new WndWindowGenericWindow(wndWindow, contentManager, callbackResolver);

                case WndWindowType.ListBox:
                    return new WndWindowListBox(wndWindow, contentManager, callbackResolver);

                case WndWindowType.PushButton:
                    return new WndWindowPushButton(wndWindow, contentManager, callbackResolver);

                case WndWindowType.StaticText:
                    return new WndWindowStaticText(wndWindow, contentManager, callbackResolver);

                default:
                    // TODO: Implement other window types.
                    return new WndWindowGenericWindow(wndWindow, contentManager, callbackResolver);
            }
        }

        protected WndWindow(WndWindowDefinition wndWindow, ContentManager contentManager, WndCallbackResolver callbackResolver)
        {
            _stateConfigurations = new Dictionary<WndWindowState, WndWindowStateConfiguration>();

            _wndWindow = wndWindow;
            Invalidate();

            _contentManager = contentManager;

            if (wndWindow.HasHeaderTemplate)
            {
                var headerTemplate = contentManager.IniDataContext.HeaderTemplates.First(x => x.Name == wndWindow.HeaderTemplate);
                TextFont = new WndFont
                {
                    Name = headerTemplate.Font,
                    Size = headerTemplate.Point,
                    Bold = headerTemplate.Bold
                };
            }
            else
            {
                TextFont = wndWindow.Font;
            }

            _text = contentManager.TranslationManager.Lookup(wndWindow.Text);

            _textAlignment = TextAlignment.Center;

            void createStateConfiguration(WndWindowState state)
            {
                _stateConfigurations[state] = WndWindowStateConfiguration.Create(
                    wndWindow,
                    state,
                    contentManager);
            }

            createStateConfiguration(WndWindowState.Enabled);
            createStateConfiguration(WndWindowState.Highlighted);
            createStateConfiguration(WndWindowState.Disabled);

            Visible = !wndWindow.Status.HasFlag(WndWindowStatusFlags.Hidden)
                && wndWindow.InputCallback != "GameWinBlockInput"; // TODO: This isn't right.

            SystemCallback = callbackResolver.GetUIElementCallback(wndWindow.SystemCallback) ?? DefaultSystem;

            InputCallback = callbackResolver.GetUIElementCallback(wndWindow.InputCallback) ?? DefaultInput;

            TooltipCallback = callbackResolver.GetUIElementCallback(wndWindow.TooltipCallback);

            DrawCallback = callbackResolver.GetDrawCallback(wndWindow.DrawCallback) ?? DefaultDraw;
        }

        public WndWindow FindChild(string name)
        {
            foreach (var child in Children)
            {
                if (child.Name == name)
                {
                    return child;
                }

                var foundChild = child.FindChild(name);
                if (foundChild != null)
                {
                    return foundChild;
                }
            }

            return null;
        }

        public void Show()
        {
            Visible = true;
        }

        public void Hide()
        {
            Visible = false;
        }

        public void CreateSizeDependentResources(ContentManager contentManager, in Size windowSize)
        {
            var wndScreenRect = _wndWindow.ScreenRect;
            Frame = RectangleF.CalculateRectangleFittingAspectRatio(
                new RectangleF(
                    wndScreenRect.UpperLeft.X,
                    wndScreenRect.UpperLeft.Y,
                    wndScreenRect.BottomRight.X - wndScreenRect.UpperLeft.X,
                    wndScreenRect.BottomRight.Y - wndScreenRect.UpperLeft.Y),
                new SizeF(
                    wndScreenRect.CreationResolution.Width,
                    wndScreenRect.CreationResolution.Height),
                windowSize,
                out _scale);

            RemoveAndDispose(ref _primitiveBatch);
            RemoveAndDispose(ref _texture);

            _texture = AddDisposable(contentManager.GraphicsDevice.ResourceFactory.CreateTexture(
                TextureDescription.Texture2D(
                    (uint) Frame.Width,
                    (uint) Frame.Height,
                    1,
                    1,
                    PixelFormat.R8_G8_B8_A8_UNorm,
                    TextureUsage.Sampled | TextureUsage.RenderTarget)));

            _primitiveBatch = AddDisposable(new DrawingContext2D(contentManager, _texture));

            Invalidate();
        }

        protected void Invalidate()
        {
            IsInvalidated = true;
        }

        protected void DefaultInput(WndWindow element, WndWindowMessage message, UIElementCallbackContext context)
        {
            DefaultInputOverride(message, context);
        }

        protected virtual void DefaultInputOverride(WndWindowMessage message, UIElementCallbackContext context) { }

        protected void DefaultSystem(WndWindow element, WndWindowMessage message, UIElementCallbackContext context)
        {
            DefaultSystemOverride(message, context);
        }

        protected virtual void DefaultSystemOverride(WndWindowMessage message, UIElementCallbackContext context) { }

        public void DefaultDraw(WndWindow window, Game game)
        {
            var activeState = window.ActiveState;

            var clearColour = window.BackgroundColorOverride?.ToColorRgbaF()
                ?? activeState.BackgroundColor
                ?? new ColorRgbaF(0, 0, 0, 0);

            window.PrimitiveBatch.Begin(game.ContentManager.LinearClampSampler, clearColour);

            DefaultDrawOverride(game);

            if (!string.IsNullOrEmpty(Text))
            {
                var font = GetFont();
                var textColor = GetTextColor();

                PrimitiveBatch.DrawText(
                    Text,
                    font,
                    TextAlignment,
                    textColor,
                    Bounds.ToRectangleF());
            }

            if (activeState.BorderColor != null || window.Highlighted)
            {
                var borderColor = window.Highlighted
                    ? new ColorRgbaF(1f, 0.41f, 0.71f, 1)
                    : activeState.BorderColor.Value;

                var borderWidth = window.Highlighted ? 4 : 1;

                window.PrimitiveBatch.DrawRectangle(
                    window.Bounds.ToRectangleF(),
                    borderColor,
                    borderWidth);
            }

            if (window.OverlayColorOverride != null)
            {
                var overlayColor = window.OverlayColorOverride.Value.ToColorRgbaF();
                overlayColor.A *= window.Opacity;

                window.PrimitiveBatch.FillRectangle(
                    window.Bounds,
                    overlayColor);
            }

            window.PrimitiveBatch.End();
        }

        protected virtual void DefaultDrawOverride(Game game) { }

        internal void Render(SpriteBatch spriteBatch)
        {
            var color = ColorRgbaF.White;
            color.A = Opacity;

            spriteBatch.DrawImage(
                _texture,
                null,
                Frame.ToRectangleF(),
                color);
        }

        public Point2D PointToClient(in Point2D topLevelWindowPosition)
        {
            return new Point2D(
                topLevelWindowPosition.X - Frame.X,
                topLevelWindowPosition.Y - Frame.Y);
        }
    }

    public delegate void UIElementDrawCallback(WndWindow window, Game game);

    public delegate void UIElementCallback(WndWindow element, WndWindowMessage message, UIElementCallbackContext context);

    public sealed class UIElementCallbackContext
    {
        public WndWindowManager WindowManager { get; }
        public Game Game { get; }

        public UIElementCallbackContext(WndWindowManager windowManager, Game game)
        {
            WindowManager = windowManager;
            Game = game;
        }
    }
}
