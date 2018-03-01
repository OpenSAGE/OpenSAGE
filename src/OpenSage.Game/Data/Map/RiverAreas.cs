using System.IO;

namespace OpenSage.Data.Map
{
    [AddedIn(SageGame.Bfme2)]
    public sealed class RiverAreas : Asset
    {
        public const string AssetName = "RiverAreas";

        public RiverArea[] Areas { get; private set; }

        internal static RiverAreas Parse(BinaryReader reader, MapParseContext context)
        {
            return ParseAsset(reader, context, version =>
            {
                var numAreas = reader.ReadUInt32();
                var areas = new RiverArea[numAreas];

                for (var i = 0; i < numAreas; i++)
                {
                    areas[i] = RiverArea.Parse(reader, version);
                }

                return new RiverAreas
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
                    area.WriteTo(writer, Version);
                }
            });
        }
    }
}
