using System.IO;
using System.Numerics;
using OpenSage.Data.Utilities.Extensions;

namespace OpenSage.Data.Map
{
    public sealed class ScriptArgument
    {
        public ScriptArgumentType ArgumentType { get; private set; }

        // Either...
        public uint? UintValue { get; private set; }
        public float? FloatValue { get; private set; }
        public string StringValue { get; private set; }

        // Or...
        public Vector3? PositionValue { get; private set; }

        public bool UintValueAsBool => UintValue.Value == 1;

        internal static ScriptArgument Parse(BinaryReader reader)
        {
            var argumentType = reader.ReadUInt32AsEnum<ScriptArgumentType>();
            
            uint? uintValue = null;
            float? floatValue = null;
            string stringValue = null;
            Vector3? positionValue = null;

            if (argumentType == ScriptArgumentType.PositionCoordinate)
            {
                positionValue = reader.ReadVector3();
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

        internal void WriteTo(BinaryWriter writer)
        {
            writer.Write((uint) ArgumentType);

            if (ArgumentType == ScriptArgumentType.PositionCoordinate)
            {
                writer.Write(PositionValue.Value);
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
