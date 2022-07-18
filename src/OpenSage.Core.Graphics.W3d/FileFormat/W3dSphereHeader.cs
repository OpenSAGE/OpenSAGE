using System.IO;
using System.Numerics;
using OpenSage.Mathematics;

namespace OpenSage.FileFormats.W3d
{
    public sealed class W3dSphereHeader
    {
        public uint ChunkType { get; private set; }

        public uint ChunkSize { get; private set; }

        public uint Version { get; private set; }

        public uint Flags { get; private set; }

        public bool AlignedCamera { get; private set; }

        public bool Looping { get; private set; }

        public string Name { get; private set; }

        public Vector3 Center { get; private set; }

        public Vector3 Size { get; private set; }

        public float Duration { get; private set; }

        public ColorRgbF InitialColor { get; private set; }

        public float InitialOpacity { get; private set; }

        public Vector3 InitialScale { get; private set; }

        public Vector3 InitialAlphaVector { get; private set; }

        public Vector2 InitialAlphaVectorMagnitude { get; private set; }

        public string TextureFileName { get; private set; }

        internal static W3dSphereHeader Parse(BinaryReader reader)
        {
            var result = new W3dSphereHeader
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
            result.Size = reader.ReadVector3();
            result.Duration = reader.ReadSingle();
            result.InitialColor = reader.ReadColorRgbF();
            result.InitialOpacity = reader.ReadSingle();
            result.InitialScale = reader.ReadVector3();
            result.InitialAlphaVector = reader.ReadVector3();
            result.InitialAlphaVectorMagnitude = reader.ReadVector2();
            result.TextureFileName = reader.ReadFixedLengthString(W3dConstants.NameLength * 2);

            return result;
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(Version);
            writer.Write(Flags);
            writer.WriteFixedLengthString(Name, W3dConstants.NameLength * 2);
            writer.Write(Center);
            writer.Write(Size);
            writer.Write(Duration);
            writer.Write(InitialColor);
            writer.Write(InitialOpacity);
            writer.Write(InitialScale);
            writer.Write(InitialAlphaVector);
            writer.Write(InitialAlphaVectorMagnitude);
            writer.WriteFixedLengthString(TextureFileName, W3dConstants.NameLength * 2);
        }
    }
}
