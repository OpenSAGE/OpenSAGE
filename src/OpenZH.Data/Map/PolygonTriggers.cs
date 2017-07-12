using System.IO;

namespace OpenZH.Data.Map
{
    public sealed class PolygonTriggers : Asset
    {
        public const string AssetName = "PolygonTriggers";

        public PolygonTrigger[] Triggers { get; private set; }

        public static PolygonTriggers Parse(BinaryReader reader, MapParseContext context)
        {
            return ParseAsset(reader, context, version =>
            {
                var numTriggers = reader.ReadUInt32();
                var triggers = new PolygonTrigger[numTriggers];

                for (var i = 0; i < numTriggers; i++)
                {
                    triggers[i] = PolygonTrigger.Parse(reader, version);
                }

                return new PolygonTriggers
                {
                    Triggers = triggers
                };
            });
        }

        public void WriteTo(BinaryWriter writer)
        {
            WriteAssetTo(writer, () =>
            {
                writer.Write((uint) Triggers.Length);

                foreach (var trigger in Triggers)
                {
                    trigger.WriteTo(writer, Version);
                }
            });
        }
    }
}
