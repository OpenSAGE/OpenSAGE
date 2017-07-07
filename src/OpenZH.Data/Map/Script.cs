using System.IO;
using OpenZH.Data.Utilities.Extensions;

namespace OpenZH.Data.Map
{
    public sealed class Script : Asset
    {
        public string Name { get; private set; }

        public string Comment { get; private set; }

        /// <summary>
        /// How often the script should be evaluated, in seconds.
        /// If zero, script should be evaluated every frame.
        /// </summary>
        public uint ScriptEvaluationInterval { get; private set; }

        public static Script Parse(BinaryReader reader, MapParseContext context)
        {
            return ParseAsset(reader, context, version =>
            {
                if (version != 2)
                {
                    throw new InvalidDataException();
                }

                var name = reader.ReadUInt16PrefixedAsciiString();

                var comment = reader.ReadUInt16PrefixedAsciiString();

                var unknownBytes1 = reader.ReadBytes(4);

                var isActive = reader.ReadBoolean();
                var deactivateUponSuccess = reader.ReadBoolean();

                var activeInEasy = reader.ReadBoolean();
                var activeInMedium = reader.ReadBoolean();
                var activeInHard = reader.ReadBoolean();

                var isSubroutine = reader.ReadBoolean();

                var scriptEvaluationInterval = reader.ReadUInt32();

                ParseAssets(reader, context, assetName =>
                {
                    switch (assetName)
                    {
                        case "Condition":
                            var unknown3 = reader.ReadUInt16();
                            var unknown4 = reader.ReadUInt32();
                            var unknown5 = reader.ReadUInt32();
                            var unknown6 = reader.ReadByte();
                            break;

                        case "OrCondition":
                            var unknown1 = reader.ReadUInt16();
                            var unknown2 = reader.ReadUInt32();
                            break;

                        case "ScriptAction":
                            var unknown3_ = reader.ReadUInt16();
                            var unknown4_ = reader.ReadUInt32();
                            var unknown5_ = reader.ReadUInt32();
                            var unknown6_ = reader.ReadByte();
                            break;

                        case "CONDITION_TRUE":
                            var unknown7 = reader.ReadBytes(3);
                            break;

                        case "CONDITION_FALSE":
                            var unknown9 = reader.ReadBytes(3);
                            break;

                        case "NO_OP":
                            var unknown8 = reader.ReadBytes(3);
                            break;

                        default:
                            throw new InvalidDataException($"Unexpected asset: {assetName}");
                    }
                });
                
                return new Script
                {
                    Name = name
                };
            });
        }
    }
}
