using System.IO;
using System.Numerics;
using OpenSage.Data.Utilities.Extensions;

namespace OpenSage.Data.Map
{
    public sealed class ScriptArgument
    {
        public ScriptArgumentType ArgumentType { get; private set; }

        // Either...
        public int? IntValue { get; private set; }
        public float? FloatValue { get; private set; }
        public string StringValue { get; private set; }

        // Or...
        public Vector3? PositionValue { get; private set; }

        public bool IntValueAsBool => IntValue.Value == 1;

        internal static ScriptArgument Parse(BinaryReader reader)
        {
            // TODO: Need to make game-specific ScriptArgumentType enums.
            var argumentType = (ScriptArgumentType) reader.ReadUInt32();
            //var argumentType = reader.ReadUInt32AsEnum<ScriptArgumentType>();
            
            int? uintValue = null;
            float? floatValue = null;
            string stringValue = null;
            Vector3? positionValue = null;

            if (argumentType == ScriptArgumentType.PositionCoordinate)
            {
                positionValue = reader.ReadVector3();
            }
            else
            {
                uintValue = reader.ReadInt32();
                floatValue = reader.ReadSingle();
                stringValue = reader.ReadUInt16PrefixedAsciiString();
            }

            return new ScriptArgument
            {
                ArgumentType = argumentType,

                IntValue = uintValue,
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
                writer.Write(IntValue.Value);
                writer.Write(FloatValue.Value);
                writer.WriteUInt16PrefixedAsciiString(StringValue);
            }
        }

        public override string ToString()
        {
            var value = string.Empty;
            if (IntValue != null)
            {
                value = $"Int: {IntValue.Value.ToString()}";
            }
            else if (FloatValue != null)
            {
                value = $"Float: {FloatValue.Value.ToString()}";
            }
            else if (StringValue != null)
            {
                value = $"String: {StringValue}";
            }
            else if (PositionValue != null)
            {
                value = $"Position: {PositionValue.Value.ToString()}";
            }

            return $"({ArgumentType}) {value}";
        }
    }
}
