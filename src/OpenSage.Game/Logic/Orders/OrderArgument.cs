using System;
using System.Text;

namespace OpenSage.Logic.Orders
{
    /// <remarks/>
    public class OrderArgument
    {
        public OrderArgumentType ArgumentType { get; set; }
        public OrderArgumentValue Value { get; set; }

        public OrderArgument() { }

        public OrderArgument(
            OrderArgumentType argumentType,
            in OrderArgumentValue value)
        {
            ArgumentType = argumentType;
            Value = value;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append(ArgumentType);
            sb.Append(":");

            switch (ArgumentType)
            {
                case OrderArgumentType.Integer:
                    sb.Append(Value.Integer.ToString());
                    break;

                case OrderArgumentType.Float:
                    sb.Append(Value.Float.ToString());
                    break;

                case OrderArgumentType.Boolean:
                    sb.Append(Value.Boolean.ToString());
                    break;

                case OrderArgumentType.ObjectId:
                    sb.Append(Value.ObjectId.ToString());
                    break;

                case OrderArgumentType.Position:
                    sb.Append(Value.Position.ToString());
                    break;

                case OrderArgumentType.ScreenPosition:
                    sb.Append(Value.ScreenPosition.ToString());
                    break;

                case OrderArgumentType.ScreenRectangle:
                    sb.Append(Value.ScreenRectangle.ToString());
                    break;

                default:
                    throw new InvalidOperationException();
            }

            return sb.ToString();
        }
    }
}
