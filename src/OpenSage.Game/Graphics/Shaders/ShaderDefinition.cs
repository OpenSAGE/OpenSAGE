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

            var resourceBindingsVert = CreateResourceBindingEntries(shaderReflectionVert, ShaderStages.Vertex);
            var resourceBindingsFrag = CreateResourceBindingEntries(shaderReflectionFrag, ShaderStages.Fragment);

            var resourceBindingGroups =
                resourceBindingsVert
                .Union(resourceBindingsFrag)
                .GroupBy(x => x.Binding)
                .OrderBy(x => x.Key);

            var resourceBindings = new List<ResourceBinding>();

            foreach (var resourceBindingGroup in resourceBindingGroups)
            {
                var binding = resourceBindingGroup.Key;

                var firstResource = resourceBindingGroup.First().Resource;
                var otherResources = resourceBindingGroup.Skip(1).ToList();

                if (otherResources.Any(x => x.Resource.Type.Name != firstResource.Type.Name))
                {
                    // TODO: Could also check that types have the same members.
                    throw new InvalidOperationException();
                }

                foreach (var otherResource in otherResources)
                {
                    firstResource.Stages |= otherResource.Resource.Stages;
                }

                resourceBindings.Add(firstResource);
            }

            return new ShaderDefinition(resourceBindings);
        }

        private static List<ResourceBindingEntry> CreateResourceBindingEntries(ShaderReflection shaderReflection, ShaderStages shaderStage)
        {
            var typeCache = new List<ResourceType>(ResourceType.DefaultTypes);

            var resourceBindings = new List<ResourceBindingEntry>();

            void AddResources(ShaderReflectionResource[] resources, ResourceKind kind)
            {
                if (resources == null)
                {
                    return;
                }

                foreach (var resource in resources)
                {
                    resourceBindings.Add(CreateResourceBindingEntry(
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

        private static ResourceBindingEntry CreateResourceBindingEntry(
            ShaderReflectionResource input,
            ResourceKind kind,
            ShaderStages shaderStage,
            Dictionary<string, ShaderReflectionType> allTypes,
            List<ResourceType> typeCache)
        {
            return new ResourceBindingEntry
            {
                Binding = input.Binding,
                Resource = new ResourceBinding
                {
                    Name = input.Name,
                    Kind = kind,
                    Type = ResourceType.FromReflectionType(input.Type, allTypes, typeCache),
                    Stages = shaderStage
                }
            };
        }

        private sealed class ResourceBindingEntry
        {
            public uint Binding { get; set; }
            public ResourceBinding Resource { get; set; }
        }

        public IReadOnlyList<ResourceBinding> ResourceBindings { get; }

        private ShaderDefinition(IReadOnlyList<ResourceBinding> resourceBindings)
        {
            ResourceBindings = resourceBindings;
        }
    }
}
