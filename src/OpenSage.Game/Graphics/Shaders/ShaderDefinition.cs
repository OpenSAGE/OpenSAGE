using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Veldrid;

namespace OpenSage.Graphics.Shaders
{
    internal static class ShaderDefinitions
    {
        private static readonly Dictionary<string, ShaderDefinition> Cache = new Dictionary<string, ShaderDefinition>();

        public static ShaderDefinition GetShaderDefinition(string name)
        {
            if (!Cache.TryGetValue(name, out var result))
            {
                using (var jsonStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(typeof(ShaderDefinitions), name + ".json"))
                using (var jsonStreamReader = new StreamReader(jsonStream))
                {
                    var shaderDefinitionJson = jsonStreamReader.ReadToEnd();
                    result = JsonConvert.DeserializeObject<ShaderDefinition>(shaderDefinitionJson);
                }
                Cache.Add(name, result);
            }
            return result;
        }
    }

    internal class ShaderDefinition
    {
        public ResourceBinding[] ResourceBindings { get; set; }
    }

    internal class ResourceBinding
    {
        public string Name { get; set; }
        public ResourceKind Type { get; set; }
        public uint Register { get; set; }
        public ShaderStages Stages { get; set; }
        public uint Size { get; set; }
        public UniformBufferField[] Fields { get; set; }

        public UniformBufferField GetField(string name)
        {
            return Fields.First(x => x.Name == name);
        }
    }

    internal enum ResourceBindingType
    {
        UniformBuffer,
        StructuredBuffer,
        Texture,
        Sampler
    }

    internal class UniformBufferField
    {
        public string Name { get; set; }
        public UniformBufferFieldType Type { get; set; }
        public int Offset { get; set; }
        public int Size { get; set; }
    }

    internal enum UniformBufferFieldType
    {
        Bool,
        Uint,
        Float,
        Float2,
        Float3,
        Float4,
        Float4x4,
        Struct
    }
}
