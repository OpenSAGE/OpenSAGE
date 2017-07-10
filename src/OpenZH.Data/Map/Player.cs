using System.IO;

namespace OpenZH.Data.Map
{
    public sealed class Player
    {
        public AssetProperty[] Properties { get; private set; }
        public BuildListItem[] BuildList { get; private set; }

        public static Player Parse(BinaryReader reader, MapParseContext context)
        {
            var result = new Player
            {
                Properties = Asset.ParseProperties(reader, context)
            };

            var numBuildListItems = reader.ReadUInt32();
            var buildListItems = new BuildListItem[numBuildListItems];

            for (var i = 0; i < numBuildListItems; i++)
            {
                buildListItems[i] = BuildListItem.Parse(reader);
            }

            return result;
        }
    }
}
