using System;

namespace OpenSage.Core.Graphics
{
    public readonly struct TextureMipMapData
    {
        public readonly byte[] Data;

        public readonly uint RowPitch;
        public readonly uint SlicePitch;

        public readonly uint Width;
        public readonly uint Height;

        public TextureMipMapData(
            byte[] data,
            uint rowPitch,
            uint slicePitch,
            uint width,
            uint height)
        {
            Data = data;

            RowPitch = rowPitch;
            SlicePitch = slicePitch;

            Width = width;
            Height = height;
        }

        public static uint CalculateMipMapCount(uint width, uint height)
        {
            return 1u + (uint) MathF.Floor(MathF.Log(Math.Max(width, height), 2));
        }

        public static uint CalculateMipSize(uint mipLevel, uint baseSize)
        {
            baseSize >>= (int) mipLevel;
            return baseSize > 0 ? baseSize : 1;
        }
    }
}
