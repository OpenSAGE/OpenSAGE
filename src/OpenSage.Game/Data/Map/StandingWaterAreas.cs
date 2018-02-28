using System.IO;

namespace OpenSage.Data.Map
{
    [AddedIn(SageGame.Bfme2)]
    public sealed class StandingWaterAreas : Asset
    {
        public const string AssetName = "StandingWaterAreas";

        public StandingWaterArea[] Areas { get; private set; }

        internal static StandingWaterAreas Parse(BinaryReader reader, MapParseContext context)
        {
            return ParseAsset(reader, context, version =>
            {
                var numAreas = reader.ReadUInt32();
                var areas = new StandingWaterArea[numAreas];

                for (var i = 0; i < numAreas; i++)
                {
                    areas[i] = StandingWaterArea.Parse(reader);
                }

                return new StandingWaterAreas
                {
                    Areas = areas
                };
            });
        }

        internal void WriteTo(BinaryWriter writer)
        {
            WriteAssetTo(writer, () =>
            {
                writer.Write((uint) Areas.Length);

                foreach (var area in Areas)
                {
                    area.WriteTo(writer);
                }
            });
        }
    }
}
