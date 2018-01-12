using System.IO;

namespace OpenSage.Data.Map
{
    [AddedIn(SageGame.Ra3)]
    public sealed class MissionObjectives : Asset
    {
        public const string AssetName = "MissionObjectives";

        public MissionObjective[] Items { get; private set; }

        internal static MissionObjectives Parse(BinaryReader reader, MapParseContext context)
        {
            return ParseAsset(reader, context, version =>
            {
                var numItems = reader.ReadUInt32();
                var items = new MissionObjective[numItems];

                for (var i = 0; i < numItems; i++)
                {
                    items[i] = MissionObjective.Parse(reader);
                }

                return new MissionObjectives
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
