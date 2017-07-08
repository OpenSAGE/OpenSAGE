using System.IO;

namespace OpenZH.Data.Map
{
    public sealed class PolygonTriggers : Asset
    {
        public static PolygonTriggers Parse(BinaryReader reader, MapParseContext context)
        {
            return ParseAsset(reader, context, version =>
            {
                return new PolygonTriggers
                {

                };
            });
        }
    }
}
