using System.Collections.Generic;
using System.Numerics;
using System.Text;
using OpenSage.Mathematics;

namespace OpenSage
{
    public sealed class GameMessage
    {
        private readonly List<GameMessageArgument> _arguments;

        public readonly GameMessageType MessageType;

        public IReadOnlyList<GameMessageArgument> Arguments => _arguments;

        public GameMessage(GameMessageType messageType)
        {
            MessageType = messageType;

            _arguments = new List<GameMessageArgument>();
        }

        public void AddIntegerArgument(int value)
        {
            _arguments.Add(new GameMessageArgument(
                GameMessageArgumentType.Integer,
                new GameMessageArgumentValue { Integer = value }));
        }

        public void AddFloatArgument(float value)
        {
            _arguments.Add(new GameMessageArgument(
                GameMessageArgumentType.Float,
                new GameMessageArgumentValue { Float = value }));
        }

        public void AddBooleanArgument(bool value)
        {
            _arguments.Add(new GameMessageArgument(
                GameMessageArgumentType.Boolean,
                new GameMessageArgumentValue { Boolean = value }));
        }

        public void AddObjectIdArgument(uint value)
        {
            _arguments.Add(new GameMessageArgument(
                GameMessageArgumentType.ObjectId,
                new GameMessageArgumentValue { ObjectId = value }));
        }

        public void AddPositionArgument(in Vector3 value)
        {
            _arguments.Add(new GameMessageArgument(
                GameMessageArgumentType.Position,
                new GameMessageArgumentValue { Position = value }));
        }

        public void AddScreenPositionArgument(in Point2D value)
        {
            _arguments.Add(new GameMessageArgument(
                GameMessageArgumentType.ScreenPosition,
                new GameMessageArgumentValue { ScreenPosition = value }));
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.Append(MessageType);
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
    }
}
