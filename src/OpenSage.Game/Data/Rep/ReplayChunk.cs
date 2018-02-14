using System;
using System.IO;
using OpenSage.Data.Utilities.Extensions;

namespace OpenSage.Data.Rep
{
    public sealed class ReplayChunk
    {
        public ReplayChunkHeader Header { get; private set; }
        public GameMessage Message { get; private set; }

        internal static ReplayChunk Parse(BinaryReader reader)
        {
            var result = new ReplayChunk
            {
                Header = ReplayChunkHeader.Parse(reader)
            };

            var numUniqueArgumentTypes = reader.ReadByte();

            // Pairs of {argument type, count}.
            var argumentCounts = new (GameMessageArgumentType argumentType, byte count)[numUniqueArgumentTypes];
            for (var i = 0; i < numUniqueArgumentTypes; i++)
            {
                argumentCounts[i] = (reader.ReadByteAsEnum<GameMessageArgumentType>(), reader.ReadByte());
            }

            var message = new GameMessage(result.Header.MessageType);
            result.Message = message;

            for (var i = 0; i < numUniqueArgumentTypes; i++)
            {
                ref var argumentCount = ref argumentCounts[i];
                var argumentType = argumentCount.argumentType;

                for (var j = 0; j < argumentCount.count; j++)
                {
                    switch (argumentType)
                    {
                        case GameMessageArgumentType.Integer:
                            message.AddIntegerArgument(reader.ReadInt32());
                            break;

                        case GameMessageArgumentType.Float:
                            message.AddFloatArgument(reader.ReadSingle());
                            break;

                        case GameMessageArgumentType.Boolean:
                            message.AddBooleanArgument(reader.ReadBooleanChecked());
                            break;

                        case GameMessageArgumentType.ObjectId:
                            message.AddObjectIdArgument(reader.ReadUInt32());
                            break;

                        case GameMessageArgumentType.Position:
                            message.AddPositionArgument(reader.ReadVector3());
                            break;

                        case GameMessageArgumentType.ScreenPosition:
                            message.AddScreenPositionArgument(reader.ReadPoint2D());
                            break;

                        default:
                            throw new InvalidOperationException();
                    }
                }
            }

            return result;
        }
    }
}
