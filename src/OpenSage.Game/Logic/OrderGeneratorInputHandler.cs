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

        public override HandlingPriority Priority => _priority;
        private HandlingPriority _priority = HandlingPriority.Disabled;

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
                        var mouseDelta = (_dragEndPosition - _mousePosition).ToVector2();
                        _orderGeneratorSystem.ActiveGenerator.UpdateDrag(mouseDelta);
                    }
                    else
                    {
                        _mousePosition = message.Value.MousePosition;
                        _orderGeneratorSystem.UpdatePosition(_mousePosition.ToVector2());
                    }
                    break;
                case InputMessageType.MouseLeftButtonDown:
                    if (_orderGeneratorSystem.ActiveGenerator == null)
                    {
                        break;
                    }

                    if (!_orderGeneratorSystem.ActiveGenerator.CanDrag)
                    {
                        _orderGeneratorSystem.OnActivate();
                        break;
                    }

                    _isDragging = true;
                    // Copy initial position to drag end position so that delta is 0 if the drag ends immediately
                    _dragEndPosition = _mousePosition;

                    return InputMessageResult.Handled;
                case InputMessageType.MouseLeftButtonUp:
                    if (_orderGeneratorSystem.ActiveGenerator == null)
                    {
                        break;
                    }

                    if (_isDragging)
                    {
                        _orderGeneratorSystem.OnActivate();
                    }

                    _isDragging = false;

                    return InputMessageResult.Handled;
                case InputMessageType.MouseRightButtonDown:
                    _orderGeneratorSystem.OnRightClick();

                    return InputMessageResult.Handled;
            }

            return InputMessageResult.NotHandled;
        }
    }
}
