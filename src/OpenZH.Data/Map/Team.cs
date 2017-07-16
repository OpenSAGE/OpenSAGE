using System.IO;

namespace OpenZH.Data.Map
{
    public sealed class Team
    {
        public AssetPropertyCollection Properties { get; private set; }

        internal static Team Parse(BinaryReader reader, MapParseContext context)
        {
            return new Team
            {
                Properties = AssetPropertyCollection.Parse(reader, context)
            };
        }

        internal void WriteTo(BinaryWriter writer, AssetNameCollection assetNames)
        {
            Properties.WriteTo(writer, assetNames);
        }
    }
}
