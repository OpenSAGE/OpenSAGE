using OpenSage.Input;
using OpenSage.Mathematics;
using Veldrid;
using Rectangle = OpenSage.Mathematics.Rectangle;

namespace OpenSage.Logic
{
    public class UnitSelectionInputHandler : InputMessageHandler
    {
        private readonly UnitSelectionSystem _system;

        private Point2D _mousePos;
        private Point2D _startPos;
        private Point2D _endPos;

        private bool _leftButtonDown;
        private bool _altDown;

        private Rectangle SelectionRect
        {
            get
            {
                var topLeft = Point2D.Min(_startPos, _endPos);
                var bottomRight = Point2D.Max(_startPos, _endPos);

                return new Rectangle(topLeft,
                    new Size(bottomRight.X - topLeft.X, bottomRight.Y - topLeft.Y));
            }
        }

        public UnitSelectionInputHandler(UnitSelectionSystem system)
        {
            _system = system;
        }

        public override InputMessageResult HandleMessage(InputMessage message)
        {
            switch (message.MessageType)
            {
                case InputMessageType.MouseMove:
                    _mousePos = message.Value.MousePosition;

                    if (_leftButtonDown)
                    {
                        _endPos = _mousePos;
                        _system.UpdateSelectionUi(SelectionRect);
                    }

                    break;
                case InputMessageType.MouseLeftButtonDown:
                    // HACK?: If the camera is being rotated using ALT+Mouse1, don't handle the event.
                    // This check shouldn't really be here.
                    if (_altDown)
                    {
                        break;
                    }

                    _leftButtonDown = true;
                    _startPos = _mousePos;
                    _endPos = _startPos;
                    _system.UpdateSelectionUi(SelectionRect);
                    _system.Selecting = true;
                    return InputMessageResult.Handled;
                case InputMessageType.MouseLeftButtonUp:
                    _leftButtonDown = false;

                    if (_system.Selecting)
                    {
                        _system.SelectObjectsInRectangle(SelectionRect);
                        _system.Selecting = false;
                    }

                    break;
                case InputMessageType.KeyDown:
                    if (message.Value.Key == Key.LAlt)
                    {
                        _altDown = true;
                    }

                    break;
                case InputMessageType.KeyUp:
                    if (message.Value.Key == Key.LAlt)
                    {
                        _altDown = false;
                    }

                    break;
            }

            return InputMessageResult.NotHandled;
        }
    }
}

