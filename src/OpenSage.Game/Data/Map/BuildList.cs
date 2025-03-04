using System.IO;
using OpenSage.FileFormats;

namespace OpenSage.Data.Map
{
    [AddedIn(SageGame.Bfme)]
    public sealed class BuildList
    {
        [AddedIn(SageGame.Bfme)]
        public AssetPropertyKey FactionNameProperty { get; private set; }

        [AddedIn(SageGame.Cnc3)]
        public string FactionName { get; private set; }

        public BuildListInfo[] Items { get; private set; }

        internal static BuildList Parse(BinaryReader reader, MapParseContext context, ushort version, bool mapHasAssetList)
        {
            var result = new BuildList();

            // BFME and C&C3 both used v1 for this chunk, but store the faction name differently :(
            // If the map file has an AssetList chunk, we assume it's C&C3.
            if (mapHasAssetList)
            {
                result.FactionName = reader.ReadUInt16PrefixedAsciiString();
            }
            else
            {
                result.FactionNameProperty = AssetPropertyKey.Parse(reader, context);
            }

            var numBuildListItems = reader.ReadUInt32();
            result.Items = new BuildListInfo[numBuildListItems];

            for (var i = 0; i < numBuildListItems; i++)
            {
                result.Items[i] = BuildListInfo.Parse(reader, version, 1, mapHasAssetList);
            }

            return result;
        }

        internal void WriteTo(BinaryWriter writer, AssetNameCollection assetNames, ushort version, bool mapHasAssetList)
        {
            if (mapHasAssetList)
            {
                writer.WriteUInt16PrefixedAsciiString(FactionName);
            }
            else
            {
                FactionNameProperty.WriteTo(writer, assetNames);
            }

            writer.Write((uint)Items.Length);

            foreach (var buildListItem in Items)
            {
                buildListItem.WriteTo(writer, version, 1, mapHasAssetList);
            }
        }
    }
}
