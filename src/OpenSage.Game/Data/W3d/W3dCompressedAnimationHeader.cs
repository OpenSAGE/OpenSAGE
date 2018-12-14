using System.IO;
using OpenSage.Data.Utilities.Extensions;

namespace OpenSage.Data.W3d
{
    public sealed class W3dCompressedAnimationHeader
    {
        public uint Version { get; private set; }

        public string Name { get; private set; }

        public string HierarchyName { get; private set; }

        public uint NumFrames { get; private set; }

        public ushort FrameRate { get; private set; }

        public W3dCompressedAnimationFlavor Flavor { get; private set; }

        internal static W3dCompressedAnimationHeader Parse(BinaryReader reader)
        {
            return new W3dCompressedAnimationHeader
            {
                Version = reader.ReadUInt32(),
                Name = reader.ReadFixedLengthString(W3dConstants.NameLength),
                HierarchyName = reader.ReadFixedLengthString(W3dConstants.NameLength),
                NumFrames = reader.ReadUInt32(),
                FrameRate = reader.ReadUInt16(),
                Flavor = reader.ReadUInt16AsEnum<W3dCompressedAnimationFlavor>()
            };
        }

        internal void WriteTo(BinaryWriter writer)
        {
            writer.Write(Version);
            writer.WriteFixedLengthString(Name, W3dConstants.NameLength);
            writer.WriteFixedLengthString(HierarchyName, W3dConstants.NameLength);
            writer.Write(NumFrames);
            writer.Write(FrameRate);
            writer.Write((ushort) Flavor);
        }
    }
}
