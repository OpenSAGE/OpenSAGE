using System.Collections.Generic;

namespace OpenSage.Graphics.Shaders
{
    // Classes used to deserialise JSON produced by `spirv-cross.exe --reflect`

    internal sealed class ShaderReflection
    {
        public ShaderReflectionEntryPoint[] EntryPoints { get; set; }
        public Dictionary<string, ShaderReflectionType> Types { get; set; }
        public ShaderReflectionInputOutput[] Inputs { get; set; }
        public ShaderReflectionInputOutput[] Outputs { get; set; }

        [Newtonsoft.Json.JsonProperty(PropertyName = "separate_images")]
        public ShaderReflectionResource[] SeparateImages { get; set; }

        [Newtonsoft.Json.JsonProperty(PropertyName = "separate_samplers")]
        public ShaderReflectionResource[] SeparateSamplers { get; set; }

        public ShaderReflectionResource[] Ssbos { get; set; }
        public ShaderReflectionResource[] Ubos { get; set; }
    }

    internal sealed class ShaderReflectionEntryPoint
    {
        public string Name { get; set; }
        public string Mode { get; set; }
    }

    internal sealed class ShaderReflectionType
    {
        public string Name { get; set; }
        public ShaderReflectionTypeMember[] Members { get; set; }
    }

    internal sealed class ShaderReflectionTypeMember
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public uint Offset { get; set; }
    }

    internal sealed class ShaderReflectionInputOutput
    {
        public string Type { get; set; }
        public string Name { get; set; }
        public uint Location { get; set; }
    }

    internal sealed class ShaderReflectionResource
    {
        public string Type { get; set; }
        public string Name { get; set; }
        public bool ReadOnly { get; set; }
        public uint BlockSize { get; set; }
        public uint Set { get; set; }
        public uint Binding { get; set; }
    }
}
