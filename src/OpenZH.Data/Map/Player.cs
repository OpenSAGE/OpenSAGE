using System.IO;
using OpenZH.Data.Utilities.Extensions;

namespace OpenZH.Data.Map
{
    public sealed class Player
    {
        public string Name { get; private set; }
        public bool IsHuman { get; private set; }
        public string DisplayName { get; private set; }
        public string Faction { get; private set; }
        public string Allies { get; private set; }
        public string Enemies { get; private set; }
        public ColorArgb? Color { get; private set; }

        public static Player Parse(BinaryReader reader, string[] assetStrings)
        {
            var numProperties = reader.ReadUInt16();

            var result = new Player();

            for (var i = 0; i < numProperties; i++)
            {
                var propertyType = reader.ReadUInt32();
                var propertyName = assetStrings[(propertyType >> 8) - 1];

                switch (propertyName)
                {
                    case "playerName":
                        result.Name = reader.ReadUInt16PrefixedAsciiString();
                        break;

                    case "playerIsHuman":
                        result.IsHuman = reader.ReadBoolean();
                        break;

                    case "playerDisplayName":
                        result.DisplayName = reader.ReadUInt16PrefixedUnicodeString();
                        break;

                    case "playerFaction":
                        result.Faction = reader.ReadUInt16PrefixedAsciiString();
                        break;

                    case "playerAllies":
                        result.Allies = reader.ReadUInt16PrefixedAsciiString();
                        break;

                    case "playerEnemies":
                        result.Enemies = reader.ReadUInt16PrefixedAsciiString();
                        break;

                    case "playerColor":
                        result.Color = reader.ReadColorArgb();
                        break;

                    default:
                        throw new InvalidDataException($"Unexpected property name: {propertyName}");
                }
            }

            var unknown = reader.ReadUInt32();
            if (unknown != 0)
            {
                throw new InvalidDataException();
            }

            return result;
        }
    }
}
