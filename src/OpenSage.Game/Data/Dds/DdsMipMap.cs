namespace OpenSage.Data.Dds
{
    public sealed class DdsMipMap
    {
        public byte[] Data { get; }
        public uint RowPitch { get; }

        internal DdsMipMap(byte[] data, uint rowPitch)
        {
            Data = data;
            RowPitch = rowPitch;
        }
    }
}
