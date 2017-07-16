using System.IO;

namespace OpenZH.Data.Map
{
    public sealed class Player
    {
        public AssetPropertyCollection Properties { get; private set; }
        public BuildListItem[] BuildList { get; private set; }

        internal static Player Parse(BinaryReader reader, MapParseContext context)
        {
            var result = new Player
            {
                Properties = AssetPropertyCollection.Parse(reader, context)
            };

            var numBuildListItems = reader.ReadUInt32();
            var buildListItems = new BuildListItem[numBuildListItems];

            for (var i = 0; i < numBuildListItems; i++)
            {
                buildListItems[i] = BuildListItem.Parse(reader);
            }

            result.BuildList = buildListItems;

            return result;
        }

        internal void WriteTo(BinaryWriter writer, AssetNameCollection assetNames)
        {
            Properties.WriteTo(writer, assetNames);

            writer.Write((uint) BuildList.Length);

            foreach (var buildListItem in BuildList)
            {
                buildListItem.WriteTo(writer);
            }
        }
    }
}
