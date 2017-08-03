using System;
using System.IO;

namespace OpenZH.Data.Ani
{
    public sealed class AniHeaderChunkContent : RiffChunkContent
    {
        /// <summary>
        /// Data structure size (in bytes)
        /// </summary>
        public uint HeaderSize { get; private set; }

        /// <summary>
        /// Number of images (also known as frames) stored in the file
        /// </summary>
        public uint NumFrames { get; private set; }

        /// <summary>
        /// Number of frames to be displayed before the animation repeats
        /// </summary>
        public uint NumSteps { get; private set; }

        /// <summary>
        /// Width of frame (in pixels)
        /// </summary>
        public uint FrameWidth { get; private set; }

        /// <summary>
        /// Height of frame (in pixels)
        /// </summary>
        public uint FrameHeight { get; private set; }

        /// <summary>
        /// Number of bits per pixel
        /// </summary>
        public uint BitCount { get; private set; }

        /// <summary>
        /// Number of color planes
        /// </summary>
        public uint NumPlanes { get; private set; }

        /// <summary>
        /// Default frame display rate (measured in 1/60th-of-a-second units)
        /// </summary>
        public uint DefaultFrameDisplayRate { get; private set; }

        /// <summary>
        /// ANI attribute bit flags
        /// </summary>
        public AniFrameType FrameType { get; private set; }
        public bool FileContainsSequenceData { get; private set; }

        internal static AniHeaderChunkContent Parse(BinaryReader reader)
        {
            var result = new AniHeaderChunkContent
            {
                HeaderSize = reader.ReadUInt32(),
                NumFrames = reader.ReadUInt32(),
                NumSteps = reader.ReadUInt32(),
                FrameWidth = reader.ReadUInt32(),
                FrameHeight = reader.ReadUInt32(),
                BitCount = reader.ReadUInt32(),
                NumPlanes = reader.ReadUInt32(),
                DefaultFrameDisplayRate = reader.ReadUInt32(),
            };

            var attributes = reader.ReadUInt32();
            result.FrameType = (attributes & 0x1) == 1
                ? AniFrameType.IconOrCursorData
                : AniFrameType.RawData;

            result.FileContainsSequenceData = ((attributes >> 1) & 0x1) == 1;

            if (result.FrameType == AniFrameType.RawData)
            {
                throw new NotSupportedException();
            }

            return result;
        }
    }

    public enum AniFrameType
    {
        RawData,
        IconOrCursorData
    }
}
