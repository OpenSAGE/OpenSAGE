using System;
using System.Linq;
using System.Numerics;
using OpenSage.Content;
using OpenSage.Data.Ini;
using OpenSage.Data.Wnd;
using OpenSage.LowLevel;
using OpenSage.LowLevel.Graphics2D;
using OpenSage.LowLevel.Graphics3D;
using OpenSage.Mathematics;

namespace OpenSage.Gui.Elements
{
    public abstract class UIElement : DisposableBase
    {
        private bool _initialized;

        private WndWindow _wndWindow;

        private Texture _texture;
        private RenderTarget _renderTarget;

        private UIElementState _enabledState;
        private UIElementState _highlightState;

        private float _scale;

        private HeaderTemplate _headerTemplate;
        private string _text;

        protected string Text => _text;

        private bool _needsRender;

        public WndWindow Definition => _wndWindow;

        public Rectangle Frame { get; private set; }

        public string Name => _wndWindow.Name;

        public string DisplayName => $"{Name} ({_wndWindow.WindowType}, {(Visible ? "Visible" : "Hidden")})";

        public Texture Texture => _texture;

        public bool Visible { get; private set; }

        public UIElement Parent { get; internal set; }

        public UIElementCollection Children { get; } = new UIElementCollection();

        internal UIElementCallback SystemCallback { get; private set; }
        internal UIElementCallback InputCallback { get; private set; }
        internal UIElementCallback TooltipCallback { get; private set; }
        internal UIElementCallback DrawCallback { get; private set; }

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

        public UIElement FindChild(string name)
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

        public void Initialize(WndWindow wndWindow, ContentManager contentManager)
        {
            if (_initialized)
            {
                throw new InvalidOperationException();
            }

            _wndWindow = wndWindow;
            _needsRender = true;

            if (!string.Equals(wndWindow.HeaderTemplate, "[NONE]", StringComparison.InvariantCultureIgnoreCase))
            {
                _headerTemplate = contentManager.IniDataContext.HeaderTemplates.First(x => x.Name == wndWindow.HeaderTemplate);
            }

            _text = contentManager.TranslationManager.Lookup(wndWindow.Text);

            _enabledState = AddDisposable(new UIElementState(wndWindow, wndWindow.TextColor.Enabled, wndWindow.TextColor.EnabledBorder, wndWindow.EnabledDrawData, contentManager, contentManager.GraphicsDevice, HostPlatform.GraphicsDevice2D));

            if (wndWindow.WindowType == WndWindowType.PushButton)
            {
                _highlightState = AddDisposable(new UIElementState(wndWindow, wndWindow.TextColor.Hilite, wndWindow.TextColor.HiliteBorder, wndWindow.HiliteDrawData, contentManager, contentManager.GraphicsDevice, HostPlatform.GraphicsDevice2D));
            }

            Visible = !wndWindow.Status.HasFlag(WndWindowStatusFlags.Hidden) && wndWindow.InputCallback != "GameWinBlockInput";

            SystemCallback = CallbackUtility.GetUIElementCallback(wndWindow.SystemCallback);
            InputCallback = CallbackUtility.GetUIElementCallback(wndWindow.InputCallback);
            TooltipCallback = CallbackUtility.GetUIElementCallback(wndWindow.TooltipCallback);
            DrawCallback = CallbackUtility.GetUIElementCallback(wndWindow.DrawCallback);

            // TODO

            _initialized = true;
        }

        public void CreateSizeDependentResources(ContentManager contentManager, in Size windowSize)
        {
            Frame = CalculateFrame(_wndWindow.ScreenRect, windowSize, out _scale);

            RemoveAndDispose(ref _renderTarget);
            RemoveAndDispose(ref _texture);

            _texture = AddDisposable(Texture.CreateTexture2D(
                contentManager.GraphicsDevice,
                PixelFormat.Rgba8UNorm,
                Frame.Width,
                Frame.Height,
                TextureBindFlags.ShaderResource | TextureBindFlags.RenderTarget));

            _renderTarget = AddDisposable(new RenderTarget(
                contentManager.GraphicsDevice,
                _texture));

            _needsRender = true;
        }

        private static Rectangle CalculateFrame(in WndScreenRect wndScreenRect, in Size viewportSize, out float scale)
        {
            // Figure out the ratio.
            var ratioX = viewportSize.Width / (float) wndScreenRect.CreationResolution.Width;
            var ratioY = viewportSize.Height / (float) wndScreenRect.CreationResolution.Height;

            // Use whichever multiplier is smaller.
            var ratio = ratioX < ratioY ? ratioX : ratioY;

            scale = ratio;

            var originalWidth = wndScreenRect.BottomRight.X - wndScreenRect.UpperLeft.X;
            var originalHeight = wndScreenRect.BottomRight.Y - wndScreenRect.UpperLeft.Y;

            // Now we can get the new height and width
            var newWidth = (int) Math.Round(originalWidth * ratio);
            var newHeight = (int) Math.Round(originalHeight * ratio);

            newWidth = Math.Max(newWidth, 1);
            newHeight = Math.Max(newHeight, 1);

            var newX = (int) Math.Round(wndScreenRect.UpperLeft.X * ratio);
            var newY = (int) Math.Round(wndScreenRect.UpperLeft.Y * ratio);

            // Now calculate the X,Y position of the upper-left corner 
            // (one of these will always be zero for the top level window)
            var posX = (int) Math.Round((viewportSize.Width - (wndScreenRect.CreationResolution.Width * ratio)) / 2) + newX;
            var posY = (int) Math.Round((viewportSize.Height - (wndScreenRect.CreationResolution.Height * ratio)) / 2) + newY;

            return new Rectangle(posX, posY, newWidth, newHeight);
        }

        private bool _isMouseOver;
        public bool IsMouseOver
        {
            get => _isMouseOver;
            set
            {
                _isMouseOver = value;
                Invalidate();
            }
        }

        protected internal virtual void OnMouseEnter(EventArgs args) { }
        protected internal virtual void OnMouseExit(EventArgs args) { }

        protected void Invalidate()
        {
            _needsRender = true;
        }

        public void Render(GraphicsDevice graphicsDevice)
        {
            if (!_needsRender)
            {
                return;
            }
            
            var activeState = _isMouseOver
                ? _highlightState ?? _enabledState
                : _enabledState;

            var clearColour = _backgroundColorOverride?.ToColorRgba()
                ?? activeState.BackgroundColor
                ?? new ColorRgbaF(0, 0, 0, 0);

            using (var drawingContext = new DrawingContext(HostPlatform.GraphicsDevice2D, _texture))
            {
                drawingContext.Begin();

                drawingContext.Clear(clearColour);

                if (activeState.ImageTexture != null)
                {
                    drawingContext.DrawImage(
                        activeState.ImageTexture,
                        new RawRectangleF(0, 0, activeState.ImageTexture.Width, activeState.ImageTexture.Height),
                        new RawRectangleF(0, 0, Frame.Width, Frame.Height),
                        true);
                }

                if (activeState.BorderColor != null || _highlighted)
                {
                    var borderColor = _highlighted
                        ? new ColorRgbaF(1f, 0.41f, 0.71f, 1)
                        : activeState.BorderColor.Value;

                    var borderWidth = _highlighted ? 8 : 2;

                    drawingContext.DrawRectangle(
                        new RawRectangleF(0, 0, Frame.Width, Frame.Height),
                        borderColor,
                        borderWidth);
                }

                if (_overlayColorOverride != null)
                {
                    var overlayColor = _overlayColorOverride.Value.ToColorRgba();
                    overlayColor.A *= Opacity;

                    drawingContext.FillRectangle(
                        new RawRectangleF(0, 0, Frame.Width, Frame.Height),
                        overlayColor);
                }

                if (!string.IsNullOrEmpty(Text))
                {
                    string fontName;
                    int fontSize;
                    bool fontBold;
                    if (_headerTemplate != null)
                    {
                        fontName = _headerTemplate.Font;
                        fontSize = _headerTemplate.Point;
                        fontBold = _headerTemplate.Bold;
                    }
                    else
                    {
                        fontName = _wndWindow.Font.Name;
                        fontSize = _wndWindow.Font.Size;
                        fontBold = _wndWindow.Font.Bold;
                    }
                    
                    using (var textFormat = new TextFormat(HostPlatform.GraphicsDevice2D, fontName, fontSize * _scale, fontBold ? FontWeight.Bold : FontWeight.Normal))
                    {
                        var textColor = activeState.TextColor.ToColorRgba();
                        textColor.A *= TextOpacity;

                        var rect = new RawRectangleF { X = 0, Y = 0, Width = Frame.Width, Height = Frame.Height };
                        drawingContext.DrawText(Text, textFormat, textColor, rect);
                    }
                }

                OnRender(drawingContext);

                drawingContext.End();
            }

            _needsRender = false;
        }

        protected abstract void OnRender(DrawingContext drawingContext);
    }

    internal delegate void UIElementCallback(UIElement element, UIElementCallbackContext context);

    internal sealed class UIElementCallbackContext
    {
        public GuiWindow Window { get; }
        public WindowTransitionManager TransitionManager { get; }
        public Vector2 MousePosition { get; }
        public GameTime GameTime { get; }

        public UIElementCallbackContext(
            GuiWindow window,
            WindowTransitionManager transitionManager,
            in Vector2 mousePosition,
            GameTime gameTime)
        {
            Window = window;
            TransitionManager = transitionManager;
            MousePosition = mousePosition;
            GameTime = gameTime;
        }
    }
}
