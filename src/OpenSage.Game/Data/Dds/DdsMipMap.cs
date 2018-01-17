namespace OpenSage.Data.Dds
{
    public sealed class DdsMipMap
    {
        public byte[] Data { get; }
        public uint RowPitch { get; }
        public uint SlicePitch { get; }

        internal DdsMipMap(byte[] data, uint rowPitch, uint slicePitch)
        {
            Data = data;
            RowPitch = rowPitch;
            SlicePitch = slicePitch;
        }
    }
}
