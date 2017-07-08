using System.IO;

namespace OpenZH.Data.Map
{
    public sealed class PolygonTriggers : Asset
    {
        public PolygonTrigger[] Triggers { get; private set; }

        public static PolygonTriggers Parse(BinaryReader reader, MapParseContext context)
        {
            return ParseAsset(reader, context, version =>
            {
                var numTriggers = reader.ReadUInt32();
                var triggers = new PolygonTrigger[numTriggers];

                for (var i = 0; i < numTriggers; i++)
                {
                    triggers[i] = PolygonTrigger.Parse(reader);
                }

                return new PolygonTriggers
                {
                    Triggers = triggers
                };
            });
        }
    }
}
