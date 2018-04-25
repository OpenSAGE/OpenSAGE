using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Veldrid;

namespace OpenSage.Graphics.Shaders
{
    public class ShaderDefinition
    {
        public static ShaderDefinition FromJson(string json)
        {
            return JsonConvert.DeserializeObject<ShaderDefinition>(json);
        }

        public List<ResourceBinding> ResourceBindings { get; set; } = new List<ResourceBinding>();

        public string ToJson()
        {
            return JsonConvert.SerializeObject(
                this,
                new JsonSerializerSettings
                {
                    Formatting = Formatting.Indented,
                    NullValueHandling = NullValueHandling.Ignore
                });
        }
    }

    public class ResourceBinding
    {
        public string Name { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public ResourceKind Type { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public ShaderStages Stages { get; set; }

        public uint Size { get; set; }

        public List<UniformBufferField> Fields { get; set; }

        public UniformBufferField GetField(string name)
        {
            return Fields.First(x => x.Name == name);
        }
    }

    public class UniformBufferField
    {
        public string Name { get; set; }
        public int Offset { get; set; }
        public int Size { get; set; }
    }
}
