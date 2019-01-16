using Veldrid;

namespace OpenSage.Graphics.Shaders
{
    internal sealed class ResourceBinding
    {
        public uint Set;
        public uint Binding;
        public ResourceLayoutElementDescription Description;
        public ResourceType Type;
    }
}
