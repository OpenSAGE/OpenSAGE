using System.IO;

namespace OpenSage.Data.Map
{
    [AddedIn(SageGame.BattleForMiddleEarthII)]
    public sealed class StandingWaveAreas : Asset
    {
        public const string AssetName = "StandingWaveAreas";

        public StandingWaveArea[] Areas { get; private set; }

        internal static StandingWaveAreas Parse(BinaryReader reader, MapParseContext context)
        {
            return ParseAsset(reader, context, version =>
            {
                var numAreas = reader.ReadUInt32();
                var areas = new StandingWaveArea[numAreas];

                for (var i = 0; i < numAreas; i++)
                {
                    areas[i] = StandingWaveArea.Parse(reader);
                }

                return new StandingWaveAreas
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
