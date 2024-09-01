using System.IO;
using OpenSage.Logic;

namespace OpenSage.Data.Map
{
    public sealed class Player
    {
        public AssetPropertyCollection Properties { get; internal set; }
        public BuildListItem[] BuildList { get; internal set; }

        internal static Player CreateNeutralPlayer() => CreatePlayer("Neutral");

        internal static Player CreateCivilianPlayer() => CreatePlayer("Civilian");

        private static Player CreatePlayer(string side)
        {
            var result = new Player
            {
                Properties = new AssetPropertyCollection(),
                BuildList = []
            };

            result.Properties.AddAsciiString("playerFaction", $"Faction{side}");
            result.Properties.AddAsciiString("playerName", $"plyr{side}");
            result.Properties.AddAsciiString("playerDisplayName", side);
            result.Properties.AddBoolean("playerIsHuman", false);

            return result;
        }

        internal static Player Parse(BinaryReader reader, MapParseContext context, ushort version, bool mapHasAssetList)
        {
            var result = new Player
            {
                Properties = AssetPropertyCollection.Parse(reader, context)
            };

            var numBuildListItems = reader.ReadUInt32();
            var buildListItems = new BuildListItem[numBuildListItems];

            for (var i = 0; i < numBuildListItems; i++)
            {
                buildListItems[i] = BuildListItem.Parse(reader, version, 6, mapHasAssetList);
            }

            result.BuildList = buildListItems;

            return result;
        }

        internal void WriteTo(BinaryWriter writer, AssetNameCollection assetNames, ushort version, bool mapHasAssetList)
        {
            Properties.WriteTo(writer, assetNames);

            writer.Write((uint) BuildList.Length);

            foreach (var buildListItem in BuildList)
            {
                buildListItem.WriteTo(writer, version, 6, mapHasAssetList);
            }
        }
    }
}
