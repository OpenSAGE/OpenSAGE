using System.IO;

namespace OpenZH.Data.Map
{
    public sealed class Team
    {
        public AssetProperty[] Properties { get; private set; }

        public static Team Parse(BinaryReader reader, MapParseContext context)
        {
            return new Team
            {
                Properties = Asset.ParseProperties(reader, context)
            };
        }
    }
}
