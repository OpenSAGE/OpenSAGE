namespace OpenSage.Graphics.Shaders
{
    internal sealed class ResourceTypeMember
    {
        public string Name { get; }
        public ResourceType Type { get; }
        public uint Offset { get; }
        public uint Size { get; }

        internal ResourceTypeMember(string name, ResourceType type, uint offset)
        {
            Name = name;
            Type = type;
            Offset = offset;
            Size = type.Size;
        }
    }
}
