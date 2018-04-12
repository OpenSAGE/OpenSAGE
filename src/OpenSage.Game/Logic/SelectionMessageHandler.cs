using OpenSage.Input;
using OpenSage.Mathematics;
using Veldrid;

namespace OpenSage.Logic
{
    public class SelectionMessageHandler : InputMessageHandler
    {
        private readonly SelectionSystem _system;

        private Point2D _mousePos;
        private bool _altDown;

        public override HandlingPriority Priority => _system.Status == SelectionSystem.SelectionStatus.MultiSelecting
            ? HandlingPriority.BoxSelectionPriority
            : HandlingPriority.SelectionPriority;

        public SelectionMessageHandler(SelectionSystem system)
        {
            _system = system;
        }

        public override InputMessageResult HandleMessage(InputMessage message)
        {
            switch (message.MessageType)
            {
                case InputMessageType.MouseMove:
                    _mousePos = message.Value.MousePosition;

                    if (_system.Selecting)
                    {
                        _system.OnDragSelection(_mousePos);
                    }

                    break;
                case InputMessageType.MouseLeftButtonDown:
                    // HACK?: If the camera is being rotated using ALT+Mouse1, don't handle the event.
                    // This check shouldn't really be here.
                    if (_altDown)
                    {
                        break;
                    }

                    _system.OnStartDragSelection(_mousePos);
                    return InputMessageResult.Handled;
                case InputMessageType.MouseLeftButtonUp:
                    if (_system.Selecting)
                    {
                        _system.OnEndDragSelection();
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

