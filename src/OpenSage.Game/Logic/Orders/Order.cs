using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using Newtonsoft.Json;
using OpenSage.Mathematics;

namespace OpenSage.Logic.Orders
{
    /// <remarks/>
    public sealed class Order
    {
        public int PlayerIndex { get; set; }

        [JsonProperty("OrderType")]
        public OrderType OrderType { get; set; }

        public int DelayMSec {get; set;}

        [JsonProperty("Arguments")]
        public IList<OrderArgument> Arguments { get; set; }

        public Order()
        {
            Arguments = new List<OrderArgument>();
        }

        public Order(int playerIndex, OrderType orderType)
        {
            OrderType = orderType;
            PlayerIndex = playerIndex;

            Arguments = new List<OrderArgument>();
        }

        public void AddIntegerArgument(int value)
        {
            Arguments.Add(new OrderArgument(
                OrderArgumentType.Integer,
                new OrderArgumentValue { Integer = value }));
        }

        public void AddFloatArgument(float value)
        {
            Arguments.Add(new OrderArgument(
                OrderArgumentType.Float,
                new OrderArgumentValue { Float = value }));
        }

        public void AddBooleanArgument(bool value)
        {
            Arguments.Add(new OrderArgument(
                OrderArgumentType.Boolean,
                new OrderArgumentValue { Boolean = value }));
        }

        public void AddObjectIdArgument(uint value)
        {
            Arguments.Add(new OrderArgument(
                OrderArgumentType.ObjectId,
                new OrderArgumentValue { ObjectId = value }));
        }

        public void AddPositionArgument(in Vector3 value)
        {
            Arguments.Add(new OrderArgument(
                OrderArgumentType.Position,
                new OrderArgumentValue { Position = new Vector3Wrapper(value) }));
        }

        public void AddScreenPositionArgument(in Point2D value)
        {
            Arguments.Add(new OrderArgument(
                OrderArgumentType.ScreenPosition,
                new OrderArgumentValue { ScreenPosition = value }));
        }

        public void AddScreenRectangleArgument(in Rectangle value)
        {
            Arguments.Add(new OrderArgument(
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

        public static Order CreateMoveOrder(int playerId, in Vector3 targetPosition)
        {
            var order = new Order(playerId, OrderType.MoveTo);

            order.AddPositionArgument(targetPosition);

            return order;
        }

        public static Order CreateSetRallyPointOrder(int playerId, List<uint> objectIds, in Vector3 targetPosition)
        {
            var order = new Order(playerId, OrderType.SetRallyPoint);

            if (objectIds.Count > 1)
            {
                order.AddPositionArgument(targetPosition);
                foreach (var objId in objectIds)
                {
                    order.AddObjectIdArgument(objId);
                }
            }
            else if (objectIds.Count > 0)
            {
                order.AddObjectIdArgument(objectIds.ElementAt(0));
                order.AddPositionArgument(targetPosition);
            }

            return order;
        }

        public static Order CreateBuildObject(int playerId, int objectDefinitionId, in Vector3 position, float angle)
        {
            var order = new Order(playerId, OrderType.BuildObject);

            order.AddIntegerArgument(objectDefinitionId);
            order.AddPositionArgument(position);
            order.AddFloatArgument(angle);

            return order;
        }

        public static Order CreateAttackGround(int playerId, in Vector3 position)
        {
            var order = new Order(playerId, OrderType.ForceAttackGround);

            order.AddPositionArgument(position);

            return order;
        }

        public static Order CreateAttackObject(int playerId, uint objectId, bool force)
        {
            var order = new Order(playerId, force ? OrderType.ForceAttackObject : OrderType.AttackObject);

            order.AddObjectIdArgument(objectId);

            return order;
        }

        public static Order CreateSpecialPowerAtObject(int playerId, int specialPowerId)
        {
            var order = new Order(playerId, OrderType.SpecialPowerAtObject);

            //TODO: figure out arguments

            return order;
        }

        public static Order CreateSpecialPowerAtLocation(int playerId, int specialPowerId, in Vector3 position)
        {
            var order = new Order(playerId, OrderType.SpecialPowerAtLocation);

            order.AddIntegerArgument(specialPowerId);
            order.AddPositionArgument(position);

            // Figure those out
            order.AddObjectIdArgument(0);
            order.AddIntegerArgument(0);
            order.AddObjectIdArgument(0);

            return order;
        }

        public static Order CreateEnter(int playerId, uint objectId)
        {
            var order = new Order(playerId, OrderType.Enter);

            // TODO: Figure this out.
            order.AddObjectIdArgument(0);

            order.AddObjectIdArgument(objectId);

            return order;
        }
    }
}
