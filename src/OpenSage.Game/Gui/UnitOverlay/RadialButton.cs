using System.Numerics;
using OpenSage.Audio;
using OpenSage.Gui.ControlBar;
using OpenSage.Input;
using OpenSage.Logic.Object;
using OpenSage.Mathematics;

namespace OpenSage.Gui.UnitOverlay
{
    public class RadialButton
    {
        private Game _game;
        private int _width;

        private MappedImage _background;
        private MappedImage _border;
        private MappedImage _hover;
        private MappedImage _down;

        private Vector2 _center;

        private bool _isHovered = false;
        private bool _isPushed = false;

        private CommandButton _commandButton;
        private ObjectDefinition _objectDefinition;

        private float _progress;
        //private ControlBarScheme _scheme;

        public RadialButton(Game game, CommandButton commandButton, ObjectDefinition objectDefintion)
        {
            _game = game;
            _commandButton = commandButton;
            _objectDefinition = objectDefintion;

            _background = commandButton.ButtonImage.Value;
            _border = _game.GetMappedImage("RadialBorder");
            _hover = _game.GetMappedImage("RadialOver");
            _down = _game.GetMappedImage("RadialPush");

            _width = _border.Coords.Width;

            //_scheme = game.AssetStore.ControlBarSchemes.FindBySide(game.Scene3D.LocalPlayer.Side);
        }

        public void Update(float progress)
        {
            _isPushed = false;
            _progress = progress;
        }

        public void Render(DrawingContext2D drawingContext, Vector2 center)
        {
            _center = center;
            var rect = new RectangleF(center.X - _width / 2, center.Y - _width / 2, _width, _width);
            drawingContext.DrawMappedImage(_background, rect.Scale(0.9f));

            if (_isHovered)
            {
                drawingContext.DrawMappedImage(_hover, rect);
            }
            else if (_isPushed)
            {
                drawingContext.DrawMappedImage(_down, rect);
            }
            else
            {
                drawingContext.DrawMappedImage(_border, rect);
            }

            if (_progress > 0.0f)
            {
                drawingContext.FillRectangleRadial360(
                                        new Rectangle(rect),
                                        new ColorRgbaF(1.0f, 1.0f, 1.0f, 0.75f), //_scheme.BuildUpClockColor.ToColorRgbaF(),
                                        _progress);
            }
        }

        public bool HandleMouseCursor(InputMessage message)
        {
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
                        //_isPushed = true;
                        CommandButtonCallback.HandleCommand(_game, _commandButton, _commandButton.Object?.Value);
                        _game.Audio.PlayAudioEvent("Gui_PalantirCommandButtonClick");
                        return true;
                    }
                    break;
            }
            return false;
        }
    }
}
