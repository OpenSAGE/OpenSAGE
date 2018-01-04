using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using OpenSage.Content;
using OpenSage.Data.Ini;
using OpenSage.Data.Wnd;
using OpenSage.Graphics;
using OpenSage.Graphics.Effects;
using OpenSage.LowLevel;
using OpenSage.LowLevel.Graphics2D;
using OpenSage.LowLevel.Graphics3D;
using OpenSage.Mathematics;

namespace OpenSage.Gui.Wnd.Elements
{
    public abstract partial class UIElement : DisposableBase
    {
        private bool _initialized;

        private WndWindow _wndWindow;

        private Texture _texture;
        private Buffer<SpriteVertex> _vertexBuffer;

        private readonly Dictionary<UIElementState, UIElementStateConfiguration> _stateConfigurations;

        private UIElementState _currentState;
        internal UIElementState CurrentState
        {
            get => _currentState;
            set
            {
                _currentState = value;
                Invalidate();
            }
        }

        private float _scale;

        private HeaderTemplate _headerTemplate;

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

        private bool _needsRender;

        public WndWindow Definition => _wndWindow;

        public Rectangle Frame { get; private set; }

        public string Name => _wndWindow.Name;

        public string DisplayName => $"{Name} ({_wndWindow.WindowType}, {(Visible ? "Visible" : "Hidden")})";

        public Buffer<SpriteVertex> VertexBuffer => _vertexBuffer;

        private ConstantBuffer<SpriteMaterial.MaterialConstants> _materialConstantsBuffer;

        public SpriteMaterial Material { get; private set; }

        public bool Visible { get; private set; }

        public UIElement Parent { get; internal set; }

        public GuiWindow Window { get; internal set; }

        public UIElementCollection Children { get; } = new UIElementCollection();

        internal UIElementCallback SystemCallback { get; private set; }
        internal UIElementCallback InputCallback { get; private set; }
        internal UIElementCallback TooltipCallback { get; private set; }
        internal Action<UIElement, GraphicsDevice> DrawCallback { get; private set; }

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

        public float Opacity
        {
            get => _materialConstantsBuffer.Value.Opacity;
            set
            {
                _materialConstantsBuffer.Value.Opacity = value;
                _materialConstantsBuffer.Update();
            }
        }

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

        protected UIElement()
        {
            _stateConfigurations = new Dictionary<UIElementState, UIElementStateConfiguration>();
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

            if (wndWindow.HasHeaderTemplate)
            {
                _headerTemplate = contentManager.IniDataContext.HeaderTemplates.First(x => x.Name == wndWindow.HeaderTemplate);
            }

            _text = contentManager.TranslationManager.Lookup(wndWindow.Text);

            _textAlignment = TextAlignment.Center;
            if (wndWindow.WindowType == WndWindowType.StaticText && !wndWindow.StaticTextData.Centered)
            {
                _textAlignment = TextAlignment.Leading;
            }

            void createStateConfiguration(UIElementState state)
            {
                _stateConfigurations[state] = AddDisposable(UIElementStateConfiguration.Create(
                    wndWindow,
                    state,
                    contentManager,
                    contentManager.GraphicsDevice,
                    HostPlatform.GraphicsDevice2D));
            }

            createStateConfiguration(UIElementState.Enabled);
            createStateConfiguration(UIElementState.Highlighted);
            createStateConfiguration(UIElementState.Disabled);

            if (wndWindow.WindowType == WndWindowType.PushButton)
            {
                createStateConfiguration(UIElementState.HighlightedPushed);
            }

            Visible = !wndWindow.Status.HasFlag(WndWindowStatusFlags.Hidden)
                && wndWindow.InputCallback != "GameWinBlockInput"; // TODO: This isn't right.

            SystemCallback = CallbackUtility.GetUIElementCallback(wndWindow.SystemCallback) ?? DefaultSystem;

            InputCallback = CallbackUtility.GetUIElementCallback(wndWindow.InputCallback);
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

            TooltipCallback = CallbackUtility.GetUIElementCallback(wndWindow.TooltipCallback);

            DrawCallback = CallbackUtility.GetDrawCallback(wndWindow.DrawCallback) ?? DefaultDraw;

            Material = new SpriteMaterial(contentManager.EffectLibrary.Sprite);

            _materialConstantsBuffer = AddDisposable(new ConstantBuffer<SpriteMaterial.MaterialConstants>(contentManager.GraphicsDevice));
            _materialConstantsBuffer.Value.Opacity = 1;
            _materialConstantsBuffer.Update();

            Material.SetMaterialConstants(_materialConstantsBuffer.Buffer);

            // TODO

            _initialized = true;
        }

        public void CreateSizeDependentResources(ContentManager contentManager, in Size windowSize)
        {
            Frame = CalculateFrame(_wndWindow.ScreenRect, windowSize, out _scale);

            RemoveAndDispose(ref _vertexBuffer);
            RemoveAndDispose(ref _texture);

            _texture = AddDisposable(Texture.CreateTexture2D(
                contentManager.GraphicsDevice,
                PixelFormat.Rgba8UNorm,
                Frame.Width,
                Frame.Height,
                TextureBindFlags.ShaderResource | TextureBindFlags.RenderTarget));

            Material.SetTexture(_texture);

            var left = (Frame.X / (float) windowSize.Width) * 2 - 1;
            var top = (Frame.Y / (float) windowSize.Height) * 2 - 1;
            var right = ((Frame.X + Frame.Width) / (float) windowSize.Width) * 2 - 1;
            var bottom = ((Frame.Y + Frame.Height) / (float) windowSize.Height) * 2 - 1;

            var vertices = new[]
            {
                new SpriteVertex(new Vector2(left, top * -1), new Vector2(0, 0)),
                new SpriteVertex(new Vector2(right, top * -1), new Vector2(1, 0)),
                new SpriteVertex(new Vector2(left, bottom * -1), new Vector2(0, 1)),
                new SpriteVertex(new Vector2(right, top * -1), new Vector2(1, 0)),
                new SpriteVertex(new Vector2(right, bottom * -1), new Vector2(1, 1)),
                new SpriteVertex(new Vector2(left, bottom * -1), new Vector2(0, 1))
            };
            _vertexBuffer = AddDisposable(Buffer<SpriteVertex>.CreateStatic(
                contentManager.GraphicsDevice,
                vertices,
                BufferBindFlags.VertexBuffer));

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

        protected void Invalidate()
        {
            _needsRender = true;
        }

        private static void DefaultInput(UIElement element, GuiWindowMessage message, UIElementCallbackContext context)
        {

        }

        private static void DefaultSystem(UIElement element, GuiWindowMessage message, UIElementCallbackContext context)
        {

        }

        private static void DefaultDraw(UIElement element, GraphicsDevice graphicsDevice)
        {
            element.RenderImpl(graphicsDevice);
        }

        private void RenderImpl(GraphicsDevice graphicsDevice)
        {
            if (!_needsRender)
            {
                return;
            }

            var activeState = _stateConfigurations[_currentState];

            var clearColour = _backgroundColorOverride?.ToColorRgbaF()
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
                    var overlayColor = _overlayColorOverride.Value.ToColorRgbaF();
                    overlayColor.A *= Opacity;

                    drawingContext.FillRectangle(
                        new RawRectangleF(0, 0, Frame.Width, Frame.Height),
                        overlayColor);
                }

                if (!string.IsNullOrEmpty(_text))
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
                    
                    using (var textFormat = new TextFormat(HostPlatform.GraphicsDevice2D, fontName, fontSize * _scale, fontBold ? FontWeight.Bold : FontWeight.Normal, _textAlignment))
                    {
                        var textColor = activeState.TextColor.ToColorRgbaF();
                        textColor.A *= TextOpacity;

                        var rect = new RawRectangleF { X = 0, Y = 0, Width = Frame.Width, Height = Frame.Height };
                        drawingContext.DrawText(_text, textFormat, textColor, rect);
                    }
                }

                OnRender(drawingContext);

                drawingContext.End();
            }

            _needsRender = false;
        }

        protected virtual void OnRender(DrawingContext drawingContext) { }
    }

    internal delegate void UIElementCallback(UIElement element, GuiWindowMessage message, UIElementCallbackContext context);

    internal sealed class UIElementCallbackContext
    {
        public WndSystem GuiSystem { get; }

        public UIElementCallbackContext(WndSystem guiSystem)
        {
            GuiSystem = guiSystem;
        }
    }
}
