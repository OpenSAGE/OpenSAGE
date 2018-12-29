using OpenSage.Input;
using OpenSage.Mathematics;
using OpenSage.Network;

namespace OpenSage.Logic
{
    public class OrderGeneratorInputHandler : InputMessageHandler
    {
        private readonly OrderGeneratorSystem _orderGeneratorSystem;
        private Point2D _mousePosition;

        public override HandlingPriority Priority => HandlingPriority.OrderGeneratorPriority;

        public OrderGeneratorInputHandler(OrderGeneratorSystem orderGeneratorSystem)
        {
            _orderGeneratorSystem = orderGeneratorSystem;
        }

        public override InputMessageResult HandleMessage(InputMessage message)
        {
            switch (message.MessageType)
            {
                case InputMessageType.MouseMove:
                    _mousePosition = message.Value.MousePosition;
                    break;
                case InputMessageType.MouseLeftButtonDown:
                    if (_orderGeneratorSystem.OnClick(_mousePosition.ToVector2()))
                    {
                        return InputMessageResult.Handled;
                    }

                    return InputMessageResult.NotHandled;
            }

            return InputMessageResult.NotHandled;
        }
    }
}
