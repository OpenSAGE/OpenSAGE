﻿using System;
using System.Diagnostics;
using System.IO;
using OpenSage.Data.Utilities.Extensions;
using OpenSage.Logic.Orders;

namespace OpenSage.Data.Rep
{
    [DebuggerDisplay("[{Header.Timecode}]: {Order.OrderType} ({Order.Arguments.Count})")]
    public sealed class ReplayChunk
    {
        public ReplayChunkHeader Header { get; private set; }
        public Order Order { get; private set; }

        internal static ReplayChunk Parse(BinaryReader reader)
        {
            var result = new ReplayChunk
            {
                Header = ReplayChunkHeader.Parse(reader)
            };

            var numUniqueArgumentTypes = reader.ReadByte();

            // Pairs of {argument type, count}.
            var argumentCounts = new (OrderArgumentType argumentType, byte count)[numUniqueArgumentTypes];
            for (var i = 0; i < numUniqueArgumentTypes; i++)
            {
                argumentCounts[i] = (reader.ReadByteAsEnum<OrderArgumentType>(), reader.ReadByte());
            }

            var order = new Order((int) result.Header.Number, result.Header.OrderType);
            result.Order = order;

            for (var i = 0; i < numUniqueArgumentTypes; i++)
            {
                ref var argumentCount = ref argumentCounts[i];
                var argumentType = argumentCount.argumentType;

                for (var j = 0; j < argumentCount.count; j++)
                {
                    switch (argumentType)
                    {
                        case OrderArgumentType.Integer:
                            order.AddIntegerArgument(reader.ReadInt32());
                            break;

                        case OrderArgumentType.Float:
                            order.AddFloatArgument(reader.ReadSingle());
                            break;

                        case OrderArgumentType.Boolean:
                            order.AddBooleanArgument(reader.ReadBooleanChecked());
                            break;

                        case OrderArgumentType.ObjectId:
                            order.AddObjectIdArgument(reader.ReadUInt32());
                            break;

                        case OrderArgumentType.Position:
                            order.AddPositionArgument(reader.ReadVector3());
                            break;

                        case OrderArgumentType.ScreenPosition:
                            order.AddScreenPositionArgument(reader.ReadPoint2D());
                            break;

                        case OrderArgumentType.ScreenRectangle:
                            order.AddScreenRectangleArgument(reader.ReadRectangle());
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
