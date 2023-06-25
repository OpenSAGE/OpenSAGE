using OpenSage.Input;
using OpenSage.Mathematics;

namespace OpenSage.Logic
{
    public class OrderGeneratorInputHandler : InputMessageHandler
    {
        private readonly OrderGeneratorSystem _orderGeneratorSystem;

        private bool _isDragging;
        private Point2D _mousePosition;
        private Point2D _dragEndPosition;

        private KeyModifiers _keyModifiers;

        public override HandlingPriority Priority => _priority;
        private HandlingPriority _priority = HandlingPriority.Disabled;

        internal KeyModifiers KeyModifiers => _keyModifiers;

        internal Point2D LastMousePosition { get => _mousePosition; }

        public OrderGeneratorInputHandler(OrderGeneratorSystem orderGeneratorSystem)
        {
            _orderGeneratorSystem = orderGeneratorSystem;
        }

        public void Update()
        {
            _priority = HandlingPriority.OrderGeneratorPriority;
        }

        public override InputMessageResult HandleMessage(InputMessage message)
        {
            switch (message.MessageType)
            {
                case InputMessageType.MouseMove:
                    if (_isDragging)
                    {
                        _dragEndPosition = message.Value.MousePosition;
                        _orderGeneratorSystem.UpdateDrag(_dragEndPosition.ToVector2());
                    }
                    else
                    {
                        _mousePosition = message.Value.MousePosition;
                        _orderGeneratorSystem.UpdatePosition(_mousePosition.ToVector2());
                    }
                    break;

                case InputMessageType.MouseLeftButtonDown:
                    if (!_orderGeneratorSystem.ActiveGenerator.CanDrag)
                    {
                        if (_orderGeneratorSystem.TryActivate(_keyModifiers))
                        {
                            return InputMessageResult.Handled;
                        }
                        break;
                    }
                    _isDragging = true;
                    // Copy initial position to drag end position so that delta is 0 if the drag ends immediately
                    _dragEndPosition = _mousePosition;
                    return InputMessageResult.Handled;

                case InputMessageType.MouseLeftButtonUp:
                    if (_isDragging)
                    {
                        _orderGeneratorSystem.TryActivate(_keyModifiers);
                        _isDragging = false;
                        return InputMessageResult.Handled;
                    }
                    break;

                case InputMessageType.MouseRightButtonUp:
                    // TODO: is this desirable if we don't actually deselect the unit, but simply pan the camera?
                    _orderGeneratorSystem.CancelOrderGenerator();
                    break;

                case InputMessageType.KeyDown:
                    if (message.Value.Key == Veldrid.Key.ControlLeft ||
                        message.Value.Key == Veldrid.Key.ControlRight)
                    {
                        _keyModifiers |= KeyModifiers.Ctrl;
                        return InputMessageResult.Handled;
                    }
                    break;

                case InputMessageType.KeyUp:
                    if (message.Value.Key == Veldrid.Key.ControlLeft ||
                        message.Value.Key == Veldrid.Key.ControlRight)
                    {
                        _keyModifiers &= ~KeyModifiers.Ctrl;
                        return InputMessageResult.Handled;
                    }
                    break;
            }

            return InputMessageResult.NotHandled;
        }
    }
}
