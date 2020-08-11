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
                    else
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

                case InputMessageType.MouseRightButtonUp:
                    _system.ClearSelectedObjectsForLocalPlayer();
                    break;
            }

            return InputMessageResult.NotHandled;
        }
    }
}

