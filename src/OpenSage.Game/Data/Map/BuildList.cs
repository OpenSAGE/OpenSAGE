using System;
using System.IO;
using System.Linq;
using OpenSage.Data.Utilities.Extensions;

namespace OpenSage.Data.Map
{
    public sealed class BuildList
    {
        /// <summary>
        /// Used in BFME
        /// </summary>
        public AssetPropertyKey FactionNameProperty { get; private set; }

        /// <summary>
        /// Used in >= C&C3
        /// </summary>
        [AddedIn(SageGame.Cnc3)]
        public string FactionName { get; private set; }

        public BuildListItem[] Items { get; private set; }

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
            result.Items = new BuildListItem[numBuildListItems];

            for (var i = 0; i < numBuildListItems; i++)
            {
                result.Items[i] = BuildListItem.Parse(reader, version, 1, mapHasAssetList);
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

            writer.Write((uint) Items.Length);

            foreach (var buildListItem in Items)
            {
                buildListItem.WriteTo(writer, version, 1, mapHasAssetList);
            }
        }
    }
}
