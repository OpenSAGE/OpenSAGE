using System.IO;
using OpenSage.Data.Utilities.Extensions;

namespace OpenSage.Data.Map
{
    public sealed class BuildList
    {
        /// <summary>
        /// Used in v1
        /// </summary>
        public AssetPropertyKey FactionNameProperty { get; private set; }

        /// <summary>
        /// Used in > v1
        /// </summary>
        [AddedIn(SageGame.Cnc3)]
        public string FactionName { get; private set; }

        public BuildListItem[] Items { get; private set; }

        internal static BuildList Parse(BinaryReader reader, MapParseContext context, ushort version)
        {
            var result = new BuildList();

            if (version >= 1)
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
                result.Items[i] = BuildListItem.Parse(reader, version, 1);
            }

            return result;
        }

        internal void WriteTo(BinaryWriter writer, AssetNameCollection assetNames, ushort version)
        {
            if (version >= 1)
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
                buildListItem.WriteTo(writer, version, 1);
            }
        }
    }
}
