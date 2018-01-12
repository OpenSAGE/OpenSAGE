using System.IO;

namespace OpenSage.Data.Map
{
    [AddedIn(SageGame.Cnc3)]
    public sealed class AssetList : Asset
    {
        public const string AssetName = "AssetList";

        public AssetListItem[] Items { get; private set; }

        internal static AssetList Parse(BinaryReader reader, MapParseContext context)
        {
            return ParseAsset(reader, context, version =>
            {
                var numAssets = reader.ReadUInt32();

                var items = new AssetListItem[numAssets];
                for (var i = 0; i < numAssets; i++)
                {
                    items[i] = AssetListItem.Parse(reader);
                }

                return new AssetList
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

    [AddedIn(SageGame.Cnc3)]
    public sealed class AssetListItem
    {
        public uint TypeId { get; private set; }
        public uint InstanceId { get; private set; }

        internal static AssetListItem Parse(BinaryReader reader)
        {
            return new AssetListItem
            {
                TypeId = reader.ReadUInt32(),
                InstanceId = reader.ReadUInt32()
            };
        }

        internal void WriteTo(BinaryWriter writer)
        {
            writer.Write(TypeId);
            writer.Write(InstanceId);
        }
    }
}
