using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;

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
}
