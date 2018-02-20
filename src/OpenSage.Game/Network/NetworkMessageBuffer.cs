using System.Collections.Generic;
using OpenSage.Logic.Orders;

namespace OpenSage.Network
{
    public sealed class NetworkMessageBuffer : DisposableBase
    {
        private readonly Dictionary<uint, List<Order>> _frameOrders;
        private readonly List<Order> _localOrders;
        private readonly IConnection _connection;
        private readonly OrderProcessor _orderProcessor;

        private uint _netFrameNumber;

        public NetworkMessageBuffer(Game game, IConnection connection)
        {
            _frameOrders = new Dictionary<uint, List<Order>>();
            _localOrders = new List<Order>();
            _connection = connection;
            _orderProcessor = new OrderProcessor(game);
        }

        public void AddLocalMessage(Order message)
        {
            _localOrders.Add(message);
        }

        internal void Tick()
        {
            _connection.Receive(
                _netFrameNumber,
                (frame, message) =>
                {
                    if (!_frameOrders.TryGetValue(frame, out var messages))
                    {
                        _frameOrders.Add(frame, messages = new List<Order>());
                    }
                    messages.Add(message);
                });

            _connection.Send(_netFrameNumber, _localOrders);
            _localOrders.Clear();

            _orderProcessor.Process(_frameOrders[_netFrameNumber]);

            _netFrameNumber++;
        }
    }
}
