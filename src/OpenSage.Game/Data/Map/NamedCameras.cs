using System.IO;

namespace OpenSage.Data.Map
{
    public sealed class NamedCameras : Asset
    {
        public const string AssetName = "NamedCameras";

        public NamedCamera[] Cameras { get; private set; }

        internal static NamedCameras Parse(BinaryReader reader, MapParseContext context)
        {
            return ParseAsset(reader, context, version =>
            {
                var numNamedCameras = reader.ReadUInt32();
                var cameras = new NamedCamera[numNamedCameras];

                for (var i = 0; i < numNamedCameras; i++)
                {
                    cameras[i] = NamedCamera.Parse(reader);
                }

                return new NamedCameras
                {
                    Cameras = cameras
                };
            });
        }

        internal void WriteTo(BinaryWriter writer)
        {
            WriteAssetTo(writer, () =>
            {
                writer.Write((uint) Cameras.Length);

                foreach (var camera in Cameras)
                {
                    camera.WriteTo(writer);
                }
            });
        }
    }
}
