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
                var vertJson = ReadJson(name, "vert");
                var fragJson = ReadJson(name, "frag");

                result = ShaderDefinition.FromJson(vertJson, fragJson);

                Cache.Add(name, result);
            }
            return result;
        }

        private static string ReadJson(string name, string type)
        {
            var resourceName = $"OpenSage.Assets.Shaders.{name}.{type}.json";

            using (var jsonStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
            using (var jsonStreamReader = new StreamReader(jsonStream))
            {
                return jsonStreamReader.ReadToEnd();
            }
        }
    }
}
