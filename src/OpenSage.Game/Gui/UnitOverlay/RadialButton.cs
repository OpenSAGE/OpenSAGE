using System.Numerics;
using OpenSage.Gui.ControlBar;
using OpenSage.Gui.Wnd.Images;
using OpenSage.Input;
using OpenSage.Logic.Object;
using OpenSage.Mathematics;
using SixLabors.Fonts;

namespace OpenSage.Gui.UnitOverlay
{
    public class RadialButton
    {
        private readonly GameObject _owner;
        private readonly Game _game;
        private readonly int _width;

        private readonly MappedImage _background;
        private readonly MappedImage _border;
        private readonly MappedImage _hover;
        private readonly MappedImage _down;

        private Vector2 _center;

        private bool _isHovered = false;
        private bool _isPushed = false;

        private readonly ObjectDefinition _objectDefinition;

        private readonly Font _font;
        private readonly int _fontSize = 11;
        private readonly ColorRgbaF _fontColor;

        private float _progress;
        private int _count;
        private bool _enabled;

        private readonly Veldrid.Texture _alphaMask;
        //private ControlBarScheme _scheme;

        public readonly CommandButton CommandButton;
        public bool IsVisible { get; set; }
        public bool IsRecruitHeroButton { get; }

        public RadialButton(Game game, GameObject owner, CommandButton commandButton, bool isHeroButton = false)
        {
            _game = game;
            _owner = owner;
            CommandButton = commandButton;
            _objectDefinition = commandButton.Object?.Value ?? null;

            IsRecruitHeroButton = isHeroButton;

            _background = commandButton.ButtonImage.Value;
            _border = _game.GetMappedImage("RadialBorder");
            _hover = _game.GetMappedImage("RadialOver");
            _down = _game.GetMappedImage("RadialPush");

            _width = _border.Coords.Width;

            _fontColor = new ColorRgbaF(0, 0, 0, 1); // _game.AssetStore.InGameUI.Current.DrawableCaptionColor.ToColorRgbaF(); -> this is white -> conflicts with the progress clock
            _fontSize = _game.AssetStore.InGameUI.Current.DrawableCaptionPointSize;
            var fontWeight = _game.AssetStore.InGameUI.Current.DrawableCaptionBold ? FontWeight.Bold : FontWeight.Normal;
            _font = _game.ContentManager.FontManager.GetOrCreateFont(_game.AssetStore.InGameUI.Current.DrawableCaptionFont, _fontSize, fontWeight);

            _alphaMask = MappedImageUtility.CreateTexture(_game.GraphicsLoadContext, _game.GetMappedImage("RadialClockOverlay1"));

            //_scheme = game.AssetStore.ControlBarSchemes.FindBySide(game.Scene3D.LocalPlayer.Side);
        }

        public void Update(float progress, int count, bool enabled)
        {
            _isPushed = false;
            _progress = progress;
            _count = count;
            _enabled = enabled;
        }

        public void Render(DrawingContext2D drawingContext, Vector2 center)
        {
            _center = center;
            var rect = new RectangleF(center.X - _width / 2, center.Y - _width / 2, _width, _width);
            drawingContext.DrawMappedImage(_background, rect.Scale(0.9f), grayscaled: !_enabled);

            if (_count > 0)
            {
                //drawingContext.SetAlphaMask(_alphaMask);
                drawingContext.FillRectangleRadial360(
                                        new Rectangle(rect),
                                        new ColorRgbaF(1.0f, 1.0f, 1.0f, 0.6f), //_scheme.BuildUpClockColor.ToColorRgbaF(),
                                        _progress);

                if (_count > 1)
                {
                    var textRect = new Rectangle(RectangleF.Transform(rect, Matrix3x2.CreateTranslation(new Vector2(0, rect.Width / 3))));
                    drawingContext.DrawText(_count.ToString(), _font, TextAlignment.Center, _fontColor, textRect);
                }
                //drawingContext.SetAlphaMask(null);
            }

            if (_isHovered && _enabled)
            {
                drawingContext.DrawMappedImage(_hover, rect);
            }
            else if (_isPushed && _enabled)
            {
                drawingContext.DrawMappedImage(_down, rect);
            }
            else
            {
                drawingContext.DrawMappedImage(_border, rect);
            }
        }

        public bool HandleMouseCursor(InputMessage message)
        {
            if (!IsVisible)
            {
                return false;
            }

            switch (message.MessageType)
            {
                case InputMessageType.MouseMove:
                    var distance = (message.Value.MousePosition.ToVector2() - _center).Length();
                    if (distance <= _width / 2)
                    {
                        _isHovered = true;
                        return true;
                    }
                    _isHovered = false;
                    break;
                case InputMessageType.MouseLeftButtonUp:
                    if (_isHovered)
                    {
                        if (_enabled)
                        {
                            CommandButtonCallback.HandleCommand(_game, CommandButton, _objectDefinition);
                            _game.Audio.PlayAudioEvent("Gui_PalantirCommandButtonClick");
                        }
                        return true;
                    }
                    break;
                case InputMessageType.MouseRightButtonUp:
                    if (_isHovered)
                    {
                        if (_count > 0)
                        {
                            var index = 0;
                            // upgrades first!!
                            if (CommandButton.Upgrade != null && CommandButton.Upgrade.Value != null)
                            {
                                index = CommandButton.Upgrade.Value.InternalId;
                            }
                            else if (CommandButton.Object != null && CommandButton.Object.Value != null)
                            {
                                for (var i = 0; i < _owner.ProductionUpdate.ProductionQueue.Count; i++)
                                {
                                    var job = _owner.ProductionUpdate.ProductionQueue[i];
                                    if (job.ObjectDefinition != null && job.ObjectDefinition.Name == CommandButton.Object.Value.Name)
                                    {
                                        index = i;
                                    }
                                }
                            }

                            CommandButtonCallback.HandleCommand(_game, CommandButton, _objectDefinition, true, index);
                        }
                        return true;
                    }
                    break;
            }
            return false;
        }
    }
}
