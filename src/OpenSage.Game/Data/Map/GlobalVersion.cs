using System.IO;

namespace OpenSage.Data.Map
{
    [AddedIn(SageGame.Ra3)]
    public sealed class GlobalVersion : Asset
    {
        public const string AssetName = "GlobalVersion";

        internal static GlobalVersion Parse(BinaryReader reader, MapParseContext context)
        {
            return ParseAsset(reader, context, version => new GlobalVersion());
        }

        internal void WriteTo(BinaryWriter writer)
        {
            WriteAssetTo(writer, () => { });
        }
    }
}
