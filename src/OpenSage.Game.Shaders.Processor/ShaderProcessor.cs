using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ShaderGen;
using Veldrid;

namespace OpenSage.Graphics.Shaders.Processor
{
    /// <summary>
    /// Creates a .json file that specifies the shader's resource bindings,
    /// which is used within the engine to create a pipeline object.
    /// </summary>
    public class ShaderProcessor : IShaderSetProcessor
    {
        public string UserArgs { get; set; }

        public void ProcessShaderSet(ShaderSetProcessorInput input)
        {
            var shaderDefinition = new ShaderDefinition();

            foreach (var resource in input.Model.AllResources)
            {
                try
                {
                    var resourceBinding = CreateResourceBinding(input.Model, resource);
                    shaderDefinition.ResourceBindings.Add(resourceBinding);
                }
                catch (Exception ex)
                {
                    throw new Exception($"Could not create resource binding for set {input.SetName}, resource {resource.Name}, value type {resource.ValueType.Name}", ex);
                }
            }

            var path = Path.Combine(UserArgs, input.SetName + ".json");
            var json = shaderDefinition.ToJson();
            File.WriteAllText(path, json);
        }

        private static ResourceBinding CreateResourceBinding(
            ShaderModel model,
            ResourceDefinition resource)
        {
            var resourceBinding = new ResourceBinding
            {
                Name = resource.Name,
                Type = GetResourceKind(resource.ResourceKind)
            };

            var shaderStages = ShaderStages.None;
            if (model.VertexResources.Contains(resource))
            {
                shaderStages |= ShaderStages.Vertex;
            }
            if (model.FragmentResources.Contains(resource))
            {
                shaderStages |= ShaderStages.Fragment;
            }
            resourceBinding.Stages = shaderStages;

            switch (resourceBinding.Type)
            {
                case ResourceKind.StructuredBufferReadOnly:
                    resourceBinding.Size = (uint) model.GetTypeSize(resource.ValueType);
                    break;

                case ResourceKind.UniformBuffer:
                    resourceBinding.Fields = new List<UniformBufferField>();
                    resourceBinding.Size = (uint) model.GetTypeSize(resource.ValueType);
                    var structureDefinition = model.GetStructureDefinition(resource.ValueType);
                    if (structureDefinition != null) // Uniform buffers can be of built-in type
                    {
                        var offset = 0;
                        foreach (var field in structureDefinition.Fields)
                        {
                            var uniformBufferField = new UniformBufferField();
                            resourceBinding.Fields.Add(uniformBufferField);

                            uniformBufferField.Name = field.Name;
                            uniformBufferField.Offset = offset;

                            var fieldSize = model.GetTypeSize(field.Type);
                            if (field.IsArray)
                            {
                                fieldSize *= field.ArrayElementCount;
                            }
                            uniformBufferField.Size = fieldSize;

                            offset += uniformBufferField.Size;
                        }
                    }
                    break;
            }

            return resourceBinding;
        }

        private static ResourceKind GetResourceKind(ShaderResourceKind resourceKind)
        {
            switch (resourceKind)
            {
                case ShaderResourceKind.Uniform:
                    return ResourceKind.UniformBuffer;

                case ShaderResourceKind.Texture2D:
                case ShaderResourceKind.Texture2DArray:
                case ShaderResourceKind.TextureCube:
                case ShaderResourceKind.Texture2DMS:
                case ShaderResourceKind.DepthTexture2D:
                case ShaderResourceKind.DepthTexture2DArray:
                    return ResourceKind.TextureReadOnly;

                case ShaderResourceKind.Sampler:
                case ShaderResourceKind.SamplerComparison:
                    return ResourceKind.Sampler;
                    
                case ShaderResourceKind.StructuredBuffer:
                    return ResourceKind.StructuredBufferReadOnly;
                    
                case ShaderResourceKind.RWStructuredBuffer:
                    return ResourceKind.StructuredBufferReadWrite;
                    
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
