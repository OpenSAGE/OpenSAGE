using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using OpenSage.Mathematics;

namespace OpenSage.Logic.Orders
{
    public sealed class Order
    {
        private readonly List<OrderArgument> _arguments;

        public int PlayerIndex { get; }

        public OrderType OrderType { get; }

        public IReadOnlyList<OrderArgument> Arguments => _arguments;

        public Order(int playerIndex, OrderType orderType)
        {
            OrderType = orderType;
            PlayerIndex = playerIndex;

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
                new OrderArgumentValue { ScreenRectangle = value }));
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

        public static Order CreateClearSelection(int playerId)
        {
            return new Order(playerId, OrderType.ClearSelection);
        }

        public static Order CreateSetSelection(int playerId, uint objectId)
        {
            var order = new Order(playerId, OrderType.SetSelection);

            // TODO: Figure out what this parameter means.
            order.AddBooleanArgument(true);
            order.AddObjectIdArgument(objectId);

            return order;
        }

        public static Order CreateSetSelection(int playerId, IEnumerable<uint> objectIds)
        {
            var order = new Order(playerId, OrderType.SetSelection);

            // TODO: Figure out what this parameter means.
            order.AddBooleanArgument(true);

            foreach (var objectId in objectIds)
            {
                order.AddObjectIdArgument(objectId);
            }

            return order;
        }

        public static Order CreateMoveOrder(int playerId, Vector3 targetPosition)
        {
            var order = new Order(playerId, OrderType.MoveTo);

            order.AddPositionArgument(targetPosition);

            return order;
        }

        public static Order CreateSetRallyPointOrder(int playerId, List<int> objectIds, Vector3 targetPosition)
        {
            var order = new Order(playerId, OrderType.SetRallyPoint);

            if (objectIds.Count > 1)
            {
                order.AddPositionArgument(targetPosition);
                foreach (var objId in objectIds)
                {
                    order.AddObjectIdArgument((uint) objId);
                }
            }
            else
            {
                order.AddObjectIdArgument((uint) objectIds.ElementAt(0));
                order.AddPositionArgument(targetPosition);
            }

            return order;
        }

        public static Order CreateBuildObject(int playerId, int objectDefinitionId, Vector3 position, float angle)
        {
            var order = new Order(playerId, OrderType.BuildObject);

            order.AddIntegerArgument(objectDefinitionId);
            order.AddPositionArgument(position);
            order.AddFloatArgument(angle);

            return order;
        }
    }
}
