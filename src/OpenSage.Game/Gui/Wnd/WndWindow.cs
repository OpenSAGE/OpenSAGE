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
    public sealed partial class WndWindow : DisposableBase
    {
        private readonly WndWindowDefinition _wndWindow;

        private Texture _texture;
        private DrawingContext2D _primitiveBatch;

        private readonly Dictionary<WndWindowState, WndWindowStateConfiguration> _stateConfigurations;

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

        public WndFont TextFont { get; }

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
        internal Action<WndWindow, Game> DrawCallback { get; private set; }

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

        public WndWindow(WndWindowDefinition wndWindow, ContentManager contentManager, WndCallbackResolver callbackResolver)
        {
            _stateConfigurations = new Dictionary<WndWindowState, WndWindowStateConfiguration>();

            _wndWindow = wndWindow;
            Invalidate();

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
            if (wndWindow.WindowType == WndWindowType.StaticText && !wndWindow.StaticTextData.Centered)
            {
                _textAlignment = TextAlignment.Leading;
            }

            void createStateConfiguration(WndWindowState state)
            {
                _stateConfigurations[state] = AddDisposable(WndWindowStateConfiguration.Create(
                    wndWindow,
                    state,
                    contentManager));
            }

            createStateConfiguration(WndWindowState.Enabled);
            createStateConfiguration(WndWindowState.Highlighted);
            createStateConfiguration(WndWindowState.Disabled);

            if (wndWindow.WindowType == WndWindowType.PushButton)
            {
                createStateConfiguration(WndWindowState.HighlightedPushed);
            }

            Visible = !wndWindow.Status.HasFlag(WndWindowStatusFlags.Hidden)
                && wndWindow.InputCallback != "GameWinBlockInput"; // TODO: This isn't right.

            SystemCallback = callbackResolver.GetUIElementCallback(wndWindow.SystemCallback) ?? DefaultSystem;

            InputCallback = callbackResolver.GetUIElementCallback(wndWindow.InputCallback);
            if (InputCallback == null)
            {
                switch (wndWindow.WindowType)
                {
                    case WndWindowType.PushButton:
                        InputCallback = DefaultPushButtonInput;
                        break;

                    default:
                        InputCallback = DefaultInput;
                        break;
                }
            }

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

        private void Invalidate()
        {
            IsInvalidated = true;
        }

        private static void DefaultInput(WndWindow element, WndWindowMessage message, UIElementCallbackContext context)
        {

        }

        private static void DefaultSystem(WndWindow element, WndWindowMessage message, UIElementCallbackContext context)
        {

        }

        private static void DefaultDraw(WndWindow window, Game game)
        {
            var activeState = window.ActiveState;

            var clearColour = window.BackgroundColorOverride?.ToColorRgbaF()
                ?? activeState.BackgroundColor
                ?? new ColorRgbaF(0, 0, 0, 0);

            window._primitiveBatch.Begin(game.ContentManager.LinearClampSampler, clearColour);

            if (activeState.ImageTexture != null)
            {
                window._primitiveBatch.DrawImage(
                    activeState.ImageTexture,
                    new Rectangle(0, 0, (int) activeState.ImageTexture.Width, (int) activeState.ImageTexture.Height),
                    window.Bounds);
            }

            if (activeState.BorderColor != null || window.Highlighted)
            {
                var borderColor = window.Highlighted
                    ? new ColorRgbaF(1f, 0.41f, 0.71f, 1)
                    : activeState.BorderColor.Value;

                var borderWidth = window.Highlighted ? 4 : 1;

                window._primitiveBatch.DrawRectangle(
                    window.Bounds.ToRectangleF(),
                    borderColor,
                    borderWidth);
            }

            if (window.OverlayColorOverride != null)
            {
                var overlayColor = window.OverlayColorOverride.Value.ToColorRgbaF();
                overlayColor.A *= window.Opacity;

                window._primitiveBatch.FillRectangle(
                    window.Bounds,
                    overlayColor);
            }

            DrawText(window, game, window._primitiveBatch);

            window._primitiveBatch.End();
        }

        private static void DrawText(
            WndWindow window,
            Game game,
            DrawingContext2D drawingContext)
        {
            if (!string.IsNullOrEmpty(window.Text))
            {
                var font = game.ContentManager.GetOrCreateFont(
                    window.TextFont.Name,
                    window.TextFont.Size * window._scale,
                    window.TextFont.Bold ? FontWeight.Bold : FontWeight.Normal);

                var textColor = window.ActiveState.TextColor.ToColorRgbaF();
                textColor.A *= window.TextOpacity;

                drawingContext.DrawText(
                    window.Text,
                    font,
                    window.TextAlignment,
                    textColor,
                    window.Bounds.ToRectangleF());
            }
        }

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
    }

    public delegate void UIElementCallback(WndWindow element, WndWindowMessage message, UIElementCallbackContext context);

    public sealed class UIElementCallbackContext
    {
        public WndWindowManager WindowManager { get; }

        public UIElementCallbackContext(WndWindowManager windowManager)
        {
            WindowManager = windowManager;
        }
    }
}
