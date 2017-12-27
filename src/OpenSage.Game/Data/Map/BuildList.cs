using System.IO;

namespace OpenSage.Data.Map
{
    public sealed class BuildList
    {
        public AssetPropertyKey FactionNameProperty { get; private set; }
        public BuildListItem2[] Items { get; private set; }

        internal static BuildList Parse(BinaryReader reader, MapParseContext context)
        {
            var factionNameProperty = AssetPropertyKey.Parse(reader, context);

            var numBuildListItems = reader.ReadUInt32();
            var buildListItems = new BuildListItem2[numBuildListItems];

            for (var i = 0; i < numBuildListItems; i++)
            {
                buildListItems[i] = BuildListItem2.Parse(reader);
            }

            return new BuildList
            {
                FactionNameProperty = factionNameProperty,
                Items = buildListItems
            };
        }

        internal void WriteTo(BinaryWriter writer, AssetNameCollection assetNames)
        {
            FactionNameProperty.WriteTo(writer, assetNames);

            writer.Write((uint) Items.Length);

            foreach (var buildListItem in Items)
            {
                buildListItem.WriteTo(writer);
            }
        }
    }
}
