using Veldrid;

namespace OpenSage.Graphics.Shaders
{
    internal class ResourceBinding
    {
        public string Name { get; set; }
        public ResourceKind Kind { get; set; }
        public ShaderStages Stages { get; set; }
        public ResourceType Type { get; set; }
    }
}
