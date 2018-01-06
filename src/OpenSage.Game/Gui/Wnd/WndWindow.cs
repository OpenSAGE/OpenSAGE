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
    public sealed partial class WndWindow : DisposableBase
    {
        private readonly WndWindowDefinition _wndWindow;

        private Texture _texture;
        private Buffer<SpriteVertex> _vertexBuffer;

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

        public Buffer<SpriteVertex> VertexBuffer => _vertexBuffer;

        private ConstantBuffer<SpriteMaterial.MaterialConstants> _materialConstantsBuffer;

        public SpriteMaterial Material { get; private set; }

        public bool Visible { get; private set; }

        public WndWindow Parent { get; internal set; }

        public WndTopLevelWindow Window { get; internal set; }

        public WndWindowCollection Children { get; } = new WndWindowCollection();

        internal UIElementCallback SystemCallback { get; private set; }
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

        internal WndWindowStateConfiguration ActiveState => _stateConfigurations[_currentState];

        public WndWindow(WndWindowDefinition wndWindow, ContentManager contentManager)
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
                    contentManager,
                    contentManager.GraphicsDevice,
                    HostPlatform.GraphicsDevice2D));
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

            Invalidate();
        }

        private void Invalidate()
        {
            IsInvalidated = true;
        }

        private static void DefaultInput(WndWindow element, GuiWindowMessage message, UIElementCallbackContext context)
        {

        }

        private static void DefaultSystem(WndWindow element, GuiWindowMessage message, UIElementCallbackContext context)
        {

        }

        private static void DefaultDraw(WndWindow window, Game game)
        {
            var activeState = window.ActiveState;

            var clearColour = window.BackgroundColorOverride?.ToColorRgbaF()
                ?? activeState.BackgroundColor
                ?? new ColorRgbaF(0, 0, 0, 0);

            using (var drawingContext = new DrawingContext(game.ContentManager.GraphicsDevice2D, window._texture))
            {
                drawingContext.Begin();

                drawingContext.Clear(clearColour);

                if (activeState.ImageTexture != null)
                {
                    drawingContext.DrawImage(
                        activeState.ImageTexture,
                        new RawRectangleF(0, 0, activeState.ImageTexture.Width, activeState.ImageTexture.Height),
                        window.Bounds.ToRawRectangleF(),
                        true);
                }

                if (activeState.BorderColor != null || window.Highlighted)
                {
                    var borderColor = window.Highlighted
                        ? new ColorRgbaF(1f, 0.41f, 0.71f, 1)
                        : activeState.BorderColor.Value;

                    var borderWidth = window.Highlighted ? 8 : 2;

                    drawingContext.DrawRectangle(
                        window.Bounds.ToRawRectangleF(),
                        borderColor,
                        borderWidth);
                }

                if (window.OverlayColorOverride != null)
                {
                    var overlayColor = window.OverlayColorOverride.Value.ToColorRgbaF();
                    overlayColor.A *= window.Opacity;

                    drawingContext.FillRectangle(
                        window.Bounds.ToRawRectangleF(),
                        overlayColor);
                }

                DrawText(window, game, drawingContext);

                drawingContext.End();
            }
        }

        private static void DrawText(
            WndWindow window,
            Game game,
            DrawingContext drawingContext)
        {
            if (!string.IsNullOrEmpty(window.Text))
            {
                var textFormat = game.ContentManager.GetOrCreateTextFormat(
                    window.TextFont.Name,
                    window.TextFont.Size * window._scale,
                    window.TextFont.Bold ? FontWeight.Bold : FontWeight.Normal,
                    window.TextAlignment);

                var textColor = window.ActiveState.TextColor.ToColorRgbaF();
                textColor.A *= window.TextOpacity;

                drawingContext.DrawText(
                    window.Text,
                    textFormat,
                    textColor,
                    window.Bounds.ToRawRectangleF());
            }
        }
    }

    internal delegate void UIElementCallback(WndWindow element, GuiWindowMessage message, UIElementCallbackContext context);

    internal sealed class UIElementCallbackContext
    {
        public WndWindowManager WindowManager { get; }

        public UIElementCallbackContext(WndWindowManager windowManager)
        {
            WindowManager = windowManager;
        }
    }
}
