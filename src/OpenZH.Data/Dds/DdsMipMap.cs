namespace OpenZH.Data.Dds
{
    public sealed class DdsMipMap
    {
        public byte[] Data { get; }
        public uint RowPitch { get; }

        public DdsMipMap(byte[] data, uint rowPitch)
        {
            Data = data;
            RowPitch = rowPitch;
        }
    }
}
