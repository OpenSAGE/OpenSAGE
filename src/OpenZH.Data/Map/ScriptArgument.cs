using System.IO;
using OpenZH.Data.Utilities.Extensions;

namespace OpenZH.Data.Map
{
    public sealed class ScriptArgument
    {
        public ScriptArgumentType ArgumentType { get; private set; }
        public uint UintValue { get; private set; }
        public float FloatValue { get; private set; }
        public string StringValue { get; private set; }

        public static ScriptArgument Parse(BinaryReader reader)
        {
            var argumentType = reader.ReadUInt32AsEnum<ScriptArgumentType>();

            var uintValue = reader.ReadUInt32();
            var floatValue = reader.ReadSingle();
            var stringValue = reader.ReadUInt16PrefixedAsciiString();

            return new ScriptArgument
            {
                ArgumentType = argumentType,

                UintValue = uintValue,
                FloatValue = floatValue,
                StringValue = stringValue
            };
        }
    }
}
