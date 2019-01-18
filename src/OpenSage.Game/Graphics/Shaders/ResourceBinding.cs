using Veldrid;

namespace OpenSage.Graphics.Shaders
{
    internal sealed class ResourceBinding
    {
        public readonly uint Binding;
        public readonly ResourceLayoutElementDescription Description;
        public readonly ResourceType Type;

        public ResourceBinding(
            uint binding,
            in ResourceLayoutElementDescription description,
            ResourceType type)
        {
            Binding = binding;
            Description = description;
            Type = type;
        }
    }
}
