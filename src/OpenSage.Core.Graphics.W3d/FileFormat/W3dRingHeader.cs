using System.IO;
using System.Numerics;
using OpenSage.Mathematics;

namespace OpenSage.FileFormats.W3d
{
    public sealed class W3dRingHeader
    {
        public uint ChunkType { get; private set; }

        public uint ChunkSize { get; private set; }

        public uint Version { get; private set; }

        public uint Flags { get; private set; }

        public bool AlignedCamera { get; private set; }

        public bool Looping { get; private set; }

        public string Name { get; private set; }

        public Vector3 Center { get; private set; }

        public Vector3 Extent { get; private set; }

        public float Duration { get; private set; }

        public ColorRgbF InitialColor { get; private set; }

        public float InitialOpacity { get; private set; }

        public Vector2 InnerScale { get; private set; }

        public Vector2 OuterScale { get; private set; }

        public Vector2 InnerRadii { get; private set; }

        public Vector2 OuterRadii { get; private set; }

        public string TextureFileName { get; private set; }

        internal static W3dRingHeader Parse(BinaryReader reader)
        {
            var result = new W3dRingHeader
            {
                ChunkType = reader.ReadUInt32(),
                ChunkSize = reader.ReadUInt32() & 0x7FFFFFFF,
                Version = reader.ReadUInt32(),
                Flags = reader.ReadUInt32()
            };

            result.AlignedCamera = ((result.Flags & 1) == 1);
            result.Looping = ((result.Flags & 2) == 2);
            result.Name = reader.ReadFixedLengthString(W3dConstants.NameLength * 2);
            result.Center = reader.ReadVector3();
            result.Extent = reader.ReadVector3();
            result.Duration = reader.ReadSingle();
            result.InitialColor = reader.ReadColorRgbF();
            result.InitialOpacity = reader.ReadSingle();
            result.InnerScale = reader.ReadVector2();
            result.OuterScale = reader.ReadVector2();
            result.InnerRadii = reader.ReadVector2();
            result.OuterRadii = reader.ReadVector2();
            result.TextureFileName = reader.ReadFixedLengthString(W3dConstants.NameLength * 2);


            return result;
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(ChunkType);
            writer.Write(ChunkSize);
            writer.Write(Version);
            writer.Write(Flags);
            writer.WriteFixedLengthString(Name, W3dConstants.NameLength * 2);
            writer.Write(Center);
            writer.Write(Extent);
            writer.Write(Duration);
            writer.Write(InitialColor);
            writer.Write(InitialOpacity);
            writer.Write(InnerScale);
            writer.Write(OuterScale);
            writer.Write(InnerRadii);
            writer.Write(OuterRadii);
            writer.WriteFixedLengthString(TextureFileName, W3dConstants.NameLength * 2);
        }
    }
}
