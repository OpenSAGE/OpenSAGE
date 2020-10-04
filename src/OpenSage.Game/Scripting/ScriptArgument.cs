using System.IO;
using System.Numerics;
using OpenSage.FileFormats;

namespace OpenSage.Scripting
{
    public sealed class ScriptArgument
    {
        public ScriptArgumentType ArgumentType { get; set; }

        // Either...
        public int? IntValue { get; set; }
        public float? FloatValue { get; set; }
        public string StringValue { get; set; }

        // Or...
        public Vector3? PositionValue { get; set; }

        public bool IntValueAsBool => IntValue.Value == 1;

        public ScriptArgument()
        {

        }

        public ScriptArgument(ScriptArgumentType argumentType, float floatValue)
        {
            ArgumentType = argumentType;
            FloatValue = floatValue;
        }

        public ScriptArgument(ScriptArgumentType argumentType, string stringValue)
        {
            ArgumentType = argumentType;
            StringValue = stringValue;
        }

        public ScriptArgument(ScriptArgumentType argumentType, int intValue)
        {
            ArgumentType = argumentType;
            IntValue = intValue;
        }

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

        public ScriptArgument Copy(string appendix)
        {
            return new ScriptArgument()
            {
                ArgumentType = ArgumentType,
                FloatValue = FloatValue,
                PositionValue = PositionValue,
                IntValue = IntValue,
                StringValue = ArgumentType switch
                {
                    ScriptArgumentType.CounterName => StringValue + appendix,
                    ScriptArgumentType.FlagName => StringValue + appendix,
                    ScriptArgumentType.ScriptName => StringValue + appendix,
                    ScriptArgumentType.SubroutineName => StringValue + appendix,
                    ScriptArgumentType.TeamName => StringValue + appendix,
                    _ => StringValue
                }
            };
        }
    }
}
