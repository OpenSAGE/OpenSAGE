using System.IO;
using OpenSage.Data.Utilities.Extensions;

namespace OpenSage.Data.W3d
{
    public sealed class W3dAnimationHeader
    {
        public uint Version { get; private set; }

        public string Name { get; private set; }

        public string HierarchyName { get; private set; }

        public uint NumFrames { get; private set; }

        public uint FrameRate { get; private set; }

        internal static W3dAnimationHeader Parse(BinaryReader reader)
        {
            return new W3dAnimationHeader
            {
                Version = reader.ReadUInt32(),
                Name = reader.ReadFixedLengthString(W3dConstants.NameLength),
                HierarchyName = reader.ReadFixedLengthString(W3dConstants.NameLength),
                NumFrames = reader.ReadUInt32(),
                FrameRate = reader.ReadUInt32()
            };
        }

        internal void WriteTo(BinaryWriter writer)
        {
            writer.Write(Version);
            writer.WriteFixedLengthString(Name, W3dConstants.NameLength);
            writer.WriteFixedLengthString(HierarchyName, W3dConstants.NameLength);
            writer.Write(NumFrames);
            writer.Write(FrameRate);
        }
    }
}
