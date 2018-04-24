using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace OpenSage.Graphics.Shaders
{
    internal static class ShaderDefinitions
    {
        private static readonly Dictionary<string, ShaderDefinition> Cache = new Dictionary<string, ShaderDefinition>();

        public static ShaderDefinition GetShaderDefinition(string name)
        {
            if (!Cache.TryGetValue(name, out var result))
            {
                using (var jsonStream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"OpenSage.Graphics.Shaders.Config.{name}.json"))
                using (var jsonStreamReader = new StreamReader(jsonStream))
                {
                    result = ShaderDefinition.FromJson(jsonStreamReader.ReadToEnd());
                }
                Cache.Add(name, result);
            }
            return result;
        }
    }
}
