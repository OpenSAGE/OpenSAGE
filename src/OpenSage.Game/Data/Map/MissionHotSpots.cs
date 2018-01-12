using System.IO;

namespace OpenSage.Data.Map
{
    [AddedIn(SageGame.Ra3)]
    public sealed class MissionHotSpots : Asset
    {
        public const string AssetName = "MissionHotSpots";

        public MissionHotSpot[] Items { get; private set; }

        internal static MissionHotSpots Parse(BinaryReader reader, MapParseContext context)
        {
            return ParseAsset(reader, context, version =>
            {
                var numItems = reader.ReadUInt32();
                var items = new MissionHotSpot[numItems];

                for (var i = 0; i < numItems; i++)
                {
                    items[i] = MissionHotSpot.Parse(reader);
                }

                return new MissionHotSpots
                {
                    Items = items
                };
            });
        }

        internal void WriteTo(BinaryWriter writer)
        {
            WriteAssetTo(writer, () =>
            {
                writer.Write((uint) Items.Length);

                foreach (var item in Items)
                {
                    item.WriteTo(writer);
                }
            });
        }
    }
}
