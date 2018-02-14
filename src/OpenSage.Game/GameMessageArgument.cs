using System;
using System.Text;

namespace OpenSage
{
    public sealed class GameMessageArgument
    {
        public readonly GameMessageArgumentType ArgumentType;
        public readonly GameMessageArgumentValue Value;

        internal GameMessageArgument(
            GameMessageArgumentType argumentType,
            in GameMessageArgumentValue value)
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
                case GameMessageArgumentType.Integer:
                    sb.Append(Value.Integer.ToString());
                    break;

                case GameMessageArgumentType.Float:
                    sb.Append(Value.Float.ToString());
                    break;

                case GameMessageArgumentType.Boolean:
                    sb.Append(Value.Boolean.ToString());
                    break;

                case GameMessageArgumentType.ObjectId:
                    sb.Append(Value.ObjectId.ToString());
                    break;

                case GameMessageArgumentType.Position:
                    sb.Append(Value.Position.ToString());
                    break;

                case GameMessageArgumentType.ScreenPosition:
                    sb.Append(Value.ScreenPosition.ToString());
                    break;

                default:
                    throw new InvalidOperationException();
            }

            return sb.ToString();
        }
    }
}
