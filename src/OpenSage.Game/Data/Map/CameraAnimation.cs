using System.IO;
using OpenSage.Data.Utilities.Extensions;

namespace OpenSage.Data.Map
{
    public sealed class CameraAnimation
    {
        public string AnimationType { get; private set; }
        public string Name { get; private set; }
        public uint NumFrames { get; private set; }
        public uint StartOffset { get; private set; }
        public CameraAnimationFrameData FrameData { get; private set; }

        internal static CameraAnimation Parse(BinaryReader reader)
        {
            var result = new CameraAnimation();

            result.AnimationType = reader.ReadFourCc(bigEndian: true);
            if (result.AnimationType != "free" && result.AnimationType != "look")
            {
                throw new InvalidDataException();
            }

            result.Name = reader.ReadUInt16PrefixedAsciiString();
            result.NumFrames = reader.ReadUInt32();
            result.StartOffset = reader.ReadUInt32();

            switch (result.AnimationType)
            {
                case "free":
                    result.FrameData = FreeCameraAnimationFrameData.Parse(reader);
                    break;

                case "look":
                    result.FrameData = LookAtCameraAnimationFrameData.Parse(reader);
                    break;

                default:
                    throw new InvalidDataException();
            }

            return result;
        }

        internal void WriteTo(BinaryWriter writer)
        {
            writer.WriteFourCc(AnimationType, bigEndian: true);
            writer.WriteUInt16PrefixedAsciiString(Name);
            writer.Write(NumFrames);
            writer.Write(StartOffset);

            FrameData.WriteTo(writer);
        }
    }

    public abstract class CameraAnimationFrameData
    {
        internal abstract void WriteTo(BinaryWriter writer);
    }

    public sealed class FreeCameraAnimationFrameData : CameraAnimationFrameData
    {
        public FreeCameraAnimationCameraFrame[] Frames { get; private set; }

        internal static FreeCameraAnimationFrameData Parse(BinaryReader reader)
        {
            var result = new FreeCameraAnimationFrameData();

            var numCameraFrames = reader.ReadUInt32();
            result.Frames = new FreeCameraAnimationCameraFrame[numCameraFrames];
            for (var i = 0; i < numCameraFrames; i++)
            {
                result.Frames[i] = FreeCameraAnimationCameraFrame.Parse(reader);
            }

            return result;
        }

        internal override void WriteTo(BinaryWriter writer)
        {
            writer.Write((uint) Frames.Length);
            foreach (var frame in Frames)
            {
                frame.WriteTo(writer);
            }
        }
    }

    public sealed class LookAtCameraAnimationFrameData : CameraAnimationFrameData
    {
        public LookAtCameraAnimationCameraFrame[] CameraFrames { get; private set; }
        public LookAtCameraAnimationLookAtFrame[] LookAtFrames { get; private set; }

        internal static LookAtCameraAnimationFrameData Parse(BinaryReader reader)
        {
            var result = new LookAtCameraAnimationFrameData();

            var numCameraFrames = reader.ReadUInt32();
            result.CameraFrames = new LookAtCameraAnimationCameraFrame[numCameraFrames];
            for (var i = 0; i < numCameraFrames; i++)
            {
                result.CameraFrames[i] = LookAtCameraAnimationCameraFrame.Parse(reader);
            }

            var numLookAtFrames = reader.ReadUInt32();
            result.LookAtFrames = new LookAtCameraAnimationLookAtFrame[numLookAtFrames];
            for (var i = 0; i < numLookAtFrames; i++)
            {
                result.LookAtFrames[i] = LookAtCameraAnimationLookAtFrame.Parse(reader);
            }

            return result;
        }

        internal override void WriteTo(BinaryWriter writer)
        {
            writer.Write((uint) CameraFrames.Length);
            foreach (var frame in CameraFrames)
            {
                frame.WriteTo(writer);
            }

            writer.Write((uint) LookAtFrames.Length);
            foreach (var frame in LookAtFrames)
            {
                frame.WriteTo(writer);
            }
        }
    }
}
