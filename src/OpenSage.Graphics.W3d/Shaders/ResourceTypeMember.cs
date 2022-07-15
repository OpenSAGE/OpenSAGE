namespace OpenSage.Graphics.Shaders
{
    public sealed class ResourceTypeMember
    {
        public readonly string Name;
        public readonly ResourceType Type;
        public readonly uint Offset;
        public readonly uint Size;

        public ResourceTypeMember(string name, ResourceType type, uint offset)
        {
            Name = name;
            Type = type;
            Offset = offset;
            Size = type.Size;
        }
    }
}
