namespace OpenSage.Utilities
{
    public readonly struct TextureMipMapData
    {
        public readonly byte[] Data;

        public readonly uint RowPitch;
        public readonly uint SlicePitch;

        public readonly uint Width;
        public readonly uint Height;

        internal TextureMipMapData(
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
    }
}
