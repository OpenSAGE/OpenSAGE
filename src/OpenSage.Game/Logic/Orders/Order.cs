using System.Collections.Generic;
using System.Numerics;
using System.Text;
using OpenSage.Mathematics;

namespace OpenSage.Logic.Orders
{
    public sealed class Order
    {
        private readonly List<OrderArgument> _arguments;

        public uint PlayerIndex { get; }

        public OrderType OrderType { get; }

        public IReadOnlyList<OrderArgument> Arguments => _arguments;

        public Order(uint playerIndex, OrderType orderType)
        {
            OrderType = orderType;

            _arguments = new List<OrderArgument>();
        }

        public void AddIntegerArgument(int value)
        {
            _arguments.Add(new OrderArgument(
                OrderArgumentType.Integer,
                new OrderArgumentValue { Integer = value }));
        }

        public void AddFloatArgument(float value)
        {
            _arguments.Add(new OrderArgument(
                OrderArgumentType.Float,
                new OrderArgumentValue { Float = value }));
        }

        public void AddBooleanArgument(bool value)
        {
            _arguments.Add(new OrderArgument(
                OrderArgumentType.Boolean,
                new OrderArgumentValue { Boolean = value }));
        }

        public void AddObjectIdArgument(uint value)
        {
            _arguments.Add(new OrderArgument(
                OrderArgumentType.ObjectId,
                new OrderArgumentValue { ObjectId = value }));
        }

        public void AddPositionArgument(in Vector3 value)
        {
            _arguments.Add(new OrderArgument(
                OrderArgumentType.Position,
                new OrderArgumentValue { Position = value }));
        }

        public void AddScreenPositionArgument(in Point2D value)
        {
            _arguments.Add(new OrderArgument(
                OrderArgumentType.ScreenPosition,
                new OrderArgumentValue { ScreenPosition = value }));
        }

        public void AddScreenRectangleArgument(in Rectangle value)
        {
            _arguments.Add(new OrderArgument(
                OrderArgumentType.ScreenRectangle,
                new OrderArgumentValue {ScreenRectangle  = value}));
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.Append(OrderType);
            sb.Append("(");

            for (var i = 0; i < Arguments.Count; i++)
            {
                sb.Append(Arguments[i]);
                if (i < Arguments.Count - 1)
                {
                    sb.Append(", ");
                }
            }

            sb.Append(")");

            return sb.ToString();
        }

        public static Order CreateSetSelection(uint playedId, uint objectId)
        {
            var order = new Order(playedId, OrderType.SetSelection);
            order.AddObjectIdArgument(objectId);
            return order;
        }
    }
}
