using System;
using System.IO;
using System.Numerics;
using OpenSage.Data.Utilities.Extensions;
using OpenSage.FileFormats;

namespace OpenSage.Data.Map
{
    public abstract class CameraAnimationFrame
    {
        public uint FrameIndex { get; private set; }
        public string InterpolationType { get; private set; }

        internal static TDerived Parse<TDerived>(
            BinaryReader reader,
            Action<TDerived> derivedParse)
            where TDerived : CameraAnimationFrame, new()
        {
            var result = new TDerived();

            result.FrameIndex = reader.ReadUInt32();

            result.InterpolationType = reader.ReadFourCc(bigEndian: true);
            if (result.InterpolationType != "catm" && result.InterpolationType != "line")
            {
                throw new InvalidDataException();
            }

            derivedParse(result);

            return result;
        }

        internal void WriteTo(BinaryWriter writer, Action derivedWriteTo)
        {
            writer.Write(FrameIndex);
            writer.WriteFourCc(InterpolationType, bigEndian: true);

            derivedWriteTo();
        }
    }

    public sealed class FreeCameraAnimationCameraFrame : CameraAnimationFrame
    {
        public Vector3 Position { get; private set; }
        public Quaternion Rotation { get; private set; }
        public float FieldOfView { get; private set; }

        internal static FreeCameraAnimationCameraFrame Parse(BinaryReader reader)
        {
            return Parse< FreeCameraAnimationCameraFrame>(reader, x =>
            {
                x.Position = reader.ReadVector3();
                x.Rotation = reader.ReadQuaternion();

                // TODO: Not sure if this is really the field of view, but it's inversely related to focal length.
                x.FieldOfView = reader.ReadSingle();
            });
        }

        internal void WriteTo(BinaryWriter writer)
        {
            WriteTo(writer, () =>
            {
                writer.Write(Position);
                writer.Write(Rotation);
                writer.Write(FieldOfView);
            });
        }
    }

    public sealed class LookAtCameraAnimationCameraFrame : CameraAnimationFrame
    {
        public Vector3 Position { get; private set; }
        public float Roll { get; private set; }
        public float FieldOfView { get; private set; }

        internal static LookAtCameraAnimationCameraFrame Parse(BinaryReader reader)
        {
            return Parse<LookAtCameraAnimationCameraFrame>(reader, x =>
            {
                x.Position = reader.ReadVector3();
                x.Roll = reader.ReadSingle();

                // TODO: Not sure if this is really the field of view, but it's inversely related to focal length.
                x.FieldOfView = reader.ReadSingle();
            });
        }

        internal void WriteTo(BinaryWriter writer)
        {
            WriteTo(writer, () =>
            {
                writer.Write(Position);
                writer.Write(Roll);
                writer.Write(FieldOfView);
            });
        }
    }

    public sealed class LookAtCameraAnimationLookAtFrame : CameraAnimationFrame
    {
        public Vector3 LookAt { get; private set; }

        internal static LookAtCameraAnimationLookAtFrame Parse(BinaryReader reader)
        {
            return Parse<LookAtCameraAnimationLookAtFrame>(reader, x =>
            {
                x.LookAt = reader.ReadVector3();
            });
        }

        internal void WriteTo(BinaryWriter writer)
        {
            WriteTo(writer, () =>
            {
                writer.Write(LookAt);
            });
        }
    }
}
