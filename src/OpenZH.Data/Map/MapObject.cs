using System.IO;
using OpenZH.Data.Utilities.Extensions;

namespace OpenZH.Data.Map
{
    public sealed class MapObject : Asset
    {
        public MapVector3 Position { get; private set; }

        /// <summary>
        /// Angle of the object in radians.
        /// </summary>
        public float Angle { get; private set; }

        public string TypeName { get; private set; }
        public uint InitialHealth { get; private set; }
        public bool Enabled { get; private set; }
        public bool Indestructible { get; private set; }
        public bool Unsellable { get; private set; }
        public bool Powered { get; private set; }
        public bool AiRecruitable { get; private set; }
        public bool Targetable { get; private set; }
        public string OriginalOwner { get; private set; }
        public string UniqueId { get; private set; }
        public string ObjectLayer { get; private set; }

        public static MapObject Parse(BinaryReader reader, MapParseContext context)
        {
            return ParseAsset(reader, context, version =>
            {
                var position = MapVector3.Parse(reader);
                var angle = reader.ReadSingle();
                
                var unknown = reader.ReadUInt32(); // Road type?

                var typeName = reader.ReadUInt16PrefixedAsciiString();

                var result = new MapObject
                {
                    Position = position,
                    Angle = angle,
                    TypeName = typeName
                };

                ParseProperties(reader, context, propertyName =>
                {
                    switch (propertyName)
                    {
                        case "objectInitialHealth":
                            result.InitialHealth = reader.ReadUInt32();
                            break;

                        case "objectEnabled":
                            result.Enabled = reader.ReadBoolean();
                            break;

                        case "objectIndestructible":
                            result.Indestructible = reader.ReadBoolean();
                            break;

                        case "objectUnsellable":
                            result.Unsellable = reader.ReadBoolean();
                            break;

                        case "objectPowered":
                            result.Powered = reader.ReadBoolean();
                            break;

                        case "objectRecruitableAI":
                            result.AiRecruitable = reader.ReadBoolean();
                            break;

                        case "objectTargetable":
                            result.Targetable = reader.ReadBoolean();
                            break;

                        case "originalOwner":
                            result.OriginalOwner = reader.ReadUInt16PrefixedAsciiString();
                            break;

                        case "uniqueID":
                            result.UniqueId = reader.ReadUInt16PrefixedAsciiString();
                            break;

                        case "objectLayer":
                            result.ObjectLayer = reader.ReadUInt16PrefixedAsciiString();
                            break;

                        default:
                            throw new InvalidDataException($"Unexpected property name: {propertyName}");
                    }
                });

                return result;
            });
        }
    }
}
