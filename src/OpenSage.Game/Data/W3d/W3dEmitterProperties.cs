using System.IO;
using OpenSage.Data.Utilities.Extensions;

namespace OpenSage.Data.W3d
{
    public sealed class W3dEmitterProperties
    {
        public uint ColorKeyframesCount { get; private set; }
        public uint OpacityKeyframesCount { get; private set; }
        public uint SizeKeyframesCount { get; private set; }
        public W3dRgba ColorRandom { get; private set; }
        public float OpacityRandom { get; private set; }
        public float SizeRandom { get; private set; }

        public W3dEmitterColorKeyframe[] ColorKeyframes { get; private set; }
        public W3dEmitterOpacityKeyframe[] OpacityKeyframes { get; private set; }
        public W3dEmitterSizeKeyframe[] SizeKeyframes { get; private set; }

        internal static W3dEmitterProperties Parse(BinaryReader reader)
        {
            var result = new W3dEmitterProperties
            {
                ColorKeyframesCount = reader.ReadUInt32(),
                OpacityKeyframesCount = reader.ReadUInt32(),
                SizeKeyframesCount = reader.ReadUInt32(),
                ColorRandom = W3dRgba.Parse(reader),
                OpacityRandom = reader.ReadSingle(),
                SizeRandom = reader.ReadSingle()
            };

            reader.ReadBytes(4 * sizeof(uint)); // Pad

            result.ColorKeyframes = new W3dEmitterColorKeyframe[result.ColorKeyframesCount];
            for (var i = 0; i < result.ColorKeyframesCount; i++)
            {
                result.ColorKeyframes[i] = W3dEmitterColorKeyframe.Parse(reader);
            }

            result.OpacityKeyframes = new W3dEmitterOpacityKeyframe[result.OpacityKeyframesCount];
            for (var i = 0; i < result.OpacityKeyframesCount; i++)
            {
                result.OpacityKeyframes[i] = W3dEmitterOpacityKeyframe.Parse(reader);
            }

            result.SizeKeyframes = new W3dEmitterSizeKeyframe[result.SizeKeyframesCount];
            for (var i = 0; i < result.SizeKeyframesCount; i++)
            {
                result.SizeKeyframes[i] = W3dEmitterSizeKeyframe.Parse(reader);
            }

            return result;
        }

        internal void WriteTo(BinaryWriter writer)
        {
            writer.Write(ColorKeyframesCount);
            writer.Write(OpacityKeyframesCount);
            writer.Write(SizeKeyframesCount);
            writer.Write(ColorRandom);
            writer.Write(OpacityRandom);
            writer.Write(SizeRandom);

            for (var i = 0; i < 4; i++) // Pad
            {
                writer.Write((uint) 0);
            }

            foreach (var keyframe in ColorKeyframes)
            {
                keyframe.WriteTo(writer);
            }

            foreach (var keyframe in OpacityKeyframes)
            {
                keyframe.WriteTo(writer);
            }

            foreach (var keyframe in SizeKeyframes)
            {
                keyframe.WriteTo(writer);
            }
        }
    }
}
