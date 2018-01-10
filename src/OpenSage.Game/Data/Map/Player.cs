using System.IO;

namespace OpenSage.Data.Map
{
    public sealed class Player
    {
        public AssetPropertyCollection Properties { get; private set; }
        public BuildListItem[] BuildList { get; private set; }

        internal static Player Parse(BinaryReader reader, MapParseContext context, ushort version)
        {
            var result = new Player
            {
                Properties = AssetPropertyCollection.Parse(reader, context)
            };

            var numBuildListItems = reader.ReadUInt32();
            var buildListItems = new BuildListItem[numBuildListItems];

            for (var i = 0; i < numBuildListItems; i++)
            {
                buildListItems[i] = BuildListItem.Parse(reader, version);
            }

            result.BuildList = buildListItems;

            return result;
        }

        internal void WriteTo(BinaryWriter writer, AssetNameCollection assetNames, ushort version)
        {
            Properties.WriteTo(writer, assetNames);

            writer.Write((uint) BuildList.Length);

            foreach (var buildListItem in BuildList)
            {
                buildListItem.WriteTo(writer, version);
            }
        }
    }
}
