using System.IO;

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

        internal void WriteTo(BinaryWriter writer)
        {
            WriteAssetTo(writer, () =>
            {
                writer.Write((uint) 0);

                // TODO
            });
        }
    }
}
