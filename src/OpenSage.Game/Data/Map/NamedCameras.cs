using System.IO;
using OpenSage.Data.Utilities.Extensions;

namespace OpenSage.Data.Map
{
    public sealed class NamedCameras : Asset
    {
        public const string AssetName = "NamedCameras";

        internal static NamedCameras Parse(BinaryReader reader, MapParseContext context)
        {
            return ParseAsset(reader, context, version =>
            {
                var numNamedCameras = reader.ReadUInt32();
                if (numNamedCameras != 0)
                {
                    throw new System.NotImplementedException();
                }

                // TODO

                return new NamedCameras
                {
                    
                };
            });
        }

        internal void WriteTo(BinaryWriter writer, AssetNameCollection assetNames)
        {
            WriteAssetTo(writer, () =>
            {
                writer.Write((byte) 0);

                // TODO
            });
        }
    }
}
