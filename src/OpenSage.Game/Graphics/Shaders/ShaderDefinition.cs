using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Veldrid;

namespace OpenSage.Graphics.Shaders
{
    internal sealed class ShaderDefinition
    {
        public static ShaderDefinition FromJson(string vertJson, string fragJson)
        {
            var shaderReflectionVert = JsonConvert.DeserializeObject<ShaderReflection>(vertJson);
            var shaderReflectionFrag = JsonConvert.DeserializeObject<ShaderReflection>(fragJson);

            var resourceBindingsVert = CreateResourceBinding(shaderReflectionVert, ShaderStages.Vertex);
            var resourceBindingsFrag = CreateResourceBinding(shaderReflectionFrag, ShaderStages.Fragment);

            var resourceBindingGroups =
                resourceBindingsVert
                .Union(resourceBindingsFrag)
                .GroupBy(x => new { x.Set, x.Binding })
                .OrderBy(x => x.Key.Set)
                .ThenBy(x => x.Key.Binding);

            var resourceBindings = new List<ResourceBinding>();

            foreach (var resourceBindingGroup in resourceBindingGroups)
            {
                var firstResource = resourceBindingGroup.First();
                var otherResources = resourceBindingGroup.Skip(1).ToList();

                if (otherResources.Any(x => x.Type.Name != firstResource.Type.Name))
                {
                    // TODO: Could also check that types have the same members.
                    throw new InvalidOperationException();
                }

                foreach (var otherResource in otherResources)
                {
                    firstResource.Description.Stages |= otherResource.Description.Stages;
                }

                resourceBindings.Add(firstResource);
            }

            return new ShaderDefinition(resourceBindings);
        }

        private static List<ResourceBinding> CreateResourceBinding(ShaderReflection shaderReflection, ShaderStages shaderStage)
        {
            var typeCache = new List<ResourceType>(ResourceType.DefaultTypes);

            var resourceBindings = new List<ResourceBinding>();

            void AddResources(ShaderReflectionResource[] resources, ResourceKind kind)
            {
                if (resources == null)
                {
                    return;
                }

                foreach (var resource in resources)
                {
                    resourceBindings.Add(CreateResourceBinding(
                        resource,
                        kind,
                        shaderStage,
                        shaderReflection.Types,
                        typeCache));
                }
            }

            AddResources(shaderReflection.SeparateImages, ResourceKind.TextureReadOnly);
            AddResources(shaderReflection.SeparateSamplers, ResourceKind.Sampler);
            AddResources(shaderReflection.Ssbos, ResourceKind.StructuredBufferReadOnly);
            AddResources(shaderReflection.Ubos, ResourceKind.UniformBuffer);

            return resourceBindings;
        }

        private static ResourceBinding CreateResourceBinding(
            ShaderReflectionResource input,
            ResourceKind kind,
            ShaderStages shaderStage,
            Dictionary<string, ShaderReflectionType> allTypes,
            List<ResourceType> typeCache)
        {
            return new ResourceBinding
            {
                Set = input.Set,
                Binding = input.Binding,
                Description = new ResourceLayoutElementDescription(
                    input.Name,
                    kind,
                    shaderStage),
                Type = ResourceType.FromReflectionType(input.Type, allTypes, typeCache)
            };
        }

        public IReadOnlyList<ResourceBinding> ResourceBindings { get; }

        private ShaderDefinition(IReadOnlyList<ResourceBinding> resourceBindings)
        {
            ResourceBindings = resourceBindings;
        }
    }
}
