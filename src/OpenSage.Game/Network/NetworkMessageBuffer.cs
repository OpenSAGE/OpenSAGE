using System;
using System.Collections.Generic;
using OpenSage.Logic.Orders;

namespace OpenSage.Network
{
    public sealed class NetworkMessageBuffer : DisposableBase
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

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

        public void AddLocalOrder(Order order)
        {
            _localOrders.Add(order);
        }

        internal void Tick()
        {
            _connection.Send(_netFrameNumber, _localOrders);

            _localOrders.Clear();

            _connection.Receive(
                _netFrameNumber,
                (frame, order) =>
                {
                    if (frame < _netFrameNumber)
                    {
                        throw new InvalidOperationException("This should not be possible, Receive should block until all orders are available.");
                    }

                    Logger.Trace($"Storing order {order.OrderType} for frame {frame}");

                    if (!_frameOrders.TryGetValue(frame, out var orders))
                    {
                        _frameOrders.Add(frame, orders = new List<Order>());
                    }
                    orders.Add(order);
                });

            if (_frameOrders.TryGetValue(_netFrameNumber, out var frameOrders))
            {
                _orderProcessor.Process(frameOrders);
                _frameOrders.Remove(_netFrameNumber);
            }

            _netFrameNumber++;
        }
    }
}
