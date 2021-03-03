using OpenSage.Input;
using OpenSage.Mathematics;

namespace OpenSage.Logic
{
    public class SelectionMessageHandler : InputMessageHandler
    {
        private readonly SelectionSystem _system;

        private Point2D _mousePos;

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
                    else if (!_system.Panning)
                    {
                        _system.OnHoverSelection(_mousePos);
                    }

                    break;

                case InputMessageType.MouseLeftButtonDown:
                    _system.OnStartDragSelection(_mousePos);
                    return InputMessageResult.Handled;

                case InputMessageType.MouseLeftButtonUp:
                    if (_system.Selecting)
                    {
                        _system.OnEndDragSelection();
                    }
                    break;

                case InputMessageType.MouseRightButtonDown:
                    _system.OnStartRightClickDrag(_mousePos);
                    break;

                case InputMessageType.MouseRightButtonUp:
                    if (_system.Panning)
                    {
                        // we need to pass in the position directly instead of relying on MouseMove as in my experience
                        // when moving quickly MouseMove wouldn't be called before MouseRightButtonUp
                        _system.OnEndRightClickDrag(message.Value.MousePosition);
                    }
                    break;
            }

            return InputMessageResult.NotHandled;
        }
    }
}

