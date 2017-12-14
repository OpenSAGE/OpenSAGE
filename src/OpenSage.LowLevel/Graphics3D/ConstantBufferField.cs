namespace OpenSage.LowLevel.Graphics3D
{
    public sealed class ConstantBufferField
    {
        public string Name { get; }
        public int Offset { get; }
        public int Size { get; }

        public ConstantBufferField(string name, int offset, int size)
        {
            Name = name;
            Offset = offset;
            Size = size;
        }
    }
}
