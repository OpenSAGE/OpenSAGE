using System.IO;
using OpenZH.Data.Utilities.Extensions;

namespace OpenZH.Data.Map
{
    public sealed class ScriptArgument
    {
        public ScriptArgumentType ArgumentType { get; private set; }

        // Either...
        public uint? UintValue { get; private set; }
        public float? FloatValue { get; private set; }
        public string StringValue { get; private set; }

        // Or...
        public MapVector3? PositionValue { get; private set; }

        public static ScriptArgument Parse(BinaryReader reader)
        {
            var argumentType = reader.ReadUInt32AsEnum<ScriptArgumentType>();
            
            uint? uintValue = null;
            float? floatValue = null;
            string stringValue = null;
            MapVector3? positionValue = null;

            if (argumentType == ScriptArgumentType.PositionCoordinate)
            {
                positionValue = MapVector3.Parse(reader);
            }
            else
            {
                uintValue = reader.ReadUInt32();
                floatValue = reader.ReadSingle();
                stringValue = reader.ReadUInt16PrefixedAsciiString();
            }

            return new ScriptArgument
            {
                ArgumentType = argumentType,

                UintValue = uintValue,
                FloatValue = floatValue,
                StringValue = stringValue,

                PositionValue = positionValue
            };
        }

        public void WriteTo(BinaryWriter writer)
        {
            writer.Write((uint) ArgumentType);

            if (ArgumentType == ScriptArgumentType.PositionCoordinate)
            {
                PositionValue.Value.WriteTo(writer);
            }
            else
            {
                writer.Write(UintValue.Value);
                writer.Write(FloatValue.Value);
                writer.WriteUInt16PrefixedAsciiString(StringValue);
            }
        }
    }
}
