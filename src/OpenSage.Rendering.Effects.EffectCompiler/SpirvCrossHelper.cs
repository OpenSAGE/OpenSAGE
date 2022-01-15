// This file originated as a C# port of https://github.com/mellinoe/veldrid-spirv/blob/master/src/libveldrid-spirv/libveldrid-spirv.cpp,
// which is licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Text;
using SPIRVCross;
using Veldrid;
using static SPIRVCross.SPIRV;

namespace OpenSage.Rendering.Effects.EffectCompiler;

public readonly struct SpirvCrossCompilationOptions
{
    public readonly bool FlipVertexY;
    public readonly bool FixupDepthConvention;
}

internal static unsafe class SpirvCrossHelper
{
    public static unsafe SpirvCrossCompilationResult RunSpirvCross(
        string vsEntryPoint, byte[] vsBytes,
        string fsEntryPoint, byte[] fsBytes,
        GraphicsBackend target,
        SpirvCrossCompilationOptions options = default)
    {
        fixed (byte* vsBytesPtr = vsBytes)
        fixed (byte* fsBytesPtr = fsBytes)
        {
            // Create context.
            spvc_context context;
            spvc_context_create(&context);

            // Parse the SPIR-V.
            spvc_parsed_ir vsIR, fsIR;
            spvc_context_parse_spirv(context, (SpvId*)vsBytesPtr, (nuint)(vsBytes.Length / 4), &vsIR);
            spvc_context_parse_spirv(context, (SpvId*)fsBytesPtr, (nuint)(fsBytes.Length / 4), &fsIR);

            // Create compilers.
            var vsCompiler = CreateCompiler(context, vsIR, target, options);
            var fsCompiler = CreateCompiler(context, fsIR, target, options);

            // Get shader resources.
            spvc_resources vsResources, fsResources;
            spvc_compiler_create_shader_resources(vsCompiler, &vsResources);
            spvc_compiler_create_shader_resources(fsCompiler, &fsResources);

            // Organise resources by bindings.
            var allResources = new Dictionary<BindingInfo, ResourceInfo>();

            AddResources(vsCompiler, vsResources, spvc_resource_type.UniformBuffer, allResources, 0, false, false);
            AddResources(vsCompiler, vsResources, spvc_resource_type.StorageBuffer, allResources, 0, false, true);
            AddResources(vsCompiler, vsResources, spvc_resource_type.SeparateImage, allResources, 0, true, false);
            AddResources(vsCompiler, vsResources, spvc_resource_type.StorageImage, allResources, 0, true, true);
            AddResources(vsCompiler, vsResources, spvc_resource_type.SeparateSamplers, allResources, 0, false, false);

            AddResources(fsCompiler, fsResources, spvc_resource_type.UniformBuffer, allResources, 1, false, false);
            AddResources(fsCompiler, fsResources, spvc_resource_type.StorageBuffer, allResources, 1, false, true);
            AddResources(fsCompiler, fsResources, spvc_resource_type.SeparateImage, allResources, 1, true, false);
            AddResources(fsCompiler, fsResources, spvc_resource_type.StorageImage, allResources, 1, true, true);
            AddResources(fsCompiler, fsResources, spvc_resource_type.SeparateSamplers, allResources, 1, false, false);

            if (target == GraphicsBackend.Direct3D11 || target == GraphicsBackend.Metal)
            {
                uint bufferIndex = 0;
                uint textureIndex = 0;
                uint uavIndex = 0;
                uint samplerIndex = 0;
                foreach (var resource in allResources)
                {
                    var index = GetResourceIndex(target, resource.Value.Kind, ref bufferIndex, ref textureIndex, ref uavIndex, ref samplerIndex);

                    var resourceInfo = resource.Value;

                    var vsId = resourceInfo.IDs[0];
                    if (vsId != 0)
                    {
                        spvc_compiler_set_decoration(vsCompiler, vsId, SpvDecoration.SpvDecorationBinding, index);
                    }
                    var fsId = resourceInfo.IDs[1];
                    if (fsId != 0)
                    {
                        spvc_compiler_set_decoration(fsCompiler, fsId, SpvDecoration.SpvDecorationBinding, index);
                    }
                }
            }

            if (target == GraphicsBackend.OpenGL)
            {
                HandleGlslSamplers(vsCompiler);
                HandleGlslSamplers(fsCompiler);
            }

            // Cross-compile shaders.

            byte* vsText;
            spvc_compiler_compile(vsCompiler, (byte*)&vsText);
            var vsCrossCompiledCode = GetBytes(vsText);

            byte* fsText;
            spvc_compiler_compile(fsCompiler, (byte*)&fsText);
            var fsCrossCompiledCode = GetBytes(fsText);

            var vertexElements = GetVertexElements(vsCompiler, vsResources);
            var resourceLayouts = CreateResourceLayouts(allResources);

            // TODO: Reflect structure members.

            spvc_context_destroy(context);

            //spvc_entry_point vsEntryPoints;
            //nuint numVsEntryPoints;
            //spvc_compiler_get_entry_points(vsCompiler, &vsEntryPoints, &numVsEntryPoints);

            return new SpirvCrossCompilationResult(
                vertexElements,
                resourceLayouts,
                vsCrossCompiledCode,
                fsCrossCompiledCode);
        }
    }

    private static void HandleGlslSamplers(spvc_compiler compiler)
    {
        uint id;
        spvc_compiler_build_dummy_sampler_for_combined_images(compiler, &id);
        spvc_compiler_build_combined_image_samplers(compiler);

        spvc_combined_image_sampler* vsCombinedImageSamplers;
        nuint numVsCombinedImageSamplers;
        spvc_compiler_get_combined_image_samplers(compiler, (spvc_combined_image_sampler*)&vsCombinedImageSamplers, &numVsCombinedImageSamplers);

        for (nuint i = 0; i < numVsCombinedImageSamplers; i++)
        {
            ref readonly var vsCombinedImageSampler = ref vsCombinedImageSamplers[i];
            spvc_compiler_set_name(compiler, (SpvId)vsCombinedImageSampler.combined_id, spvc_compiler_get_name(compiler, (SpvId)vsCombinedImageSampler.image_id));
        }
    }

    private static uint GetResourceIndex(
        GraphicsBackend target,
        ResourceKind resourceKind,
        ref uint bufferIndex,
        ref uint textureIndex,
        ref uint uavIndex,
        ref uint samplerIndex)
    {
        switch (resourceKind)
        {
            case ResourceKind.UniformBuffer:
                return bufferIndex++;

            case ResourceKind.StructuredBufferReadWrite:
                if (target == GraphicsBackend.Metal)
                {
                    return bufferIndex++;
                }
                else
                {
                    return uavIndex++;
                }

            case ResourceKind.TextureReadWrite:
                if (target == GraphicsBackend.Metal)
                {
                    return textureIndex++;
                }
                else
                {
                    return uavIndex++;
                }

            case ResourceKind.TextureReadOnly:
                return textureIndex++;

            case ResourceKind.StructuredBufferReadOnly:
                if (target == GraphicsBackend.Metal)
                {
                    return bufferIndex++;
                }
                else
                {
                    return textureIndex++;
                }

            case ResourceKind.Sampler:
                return samplerIndex++;

            default:
                throw new ArgumentOutOfRangeException(nameof(resourceKind));
        }
    }

    private static unsafe spvc_compiler CreateCompiler(
        spvc_context context,
        spvc_parsed_ir ir,
        GraphicsBackend target,
        SpirvCrossCompilationOptions options)
    {
        spvc_compiler compiler;

        var spvcBackend = target switch
        {
            GraphicsBackend.Direct3D11 => spvc_backend.Hlsl,
            GraphicsBackend.Metal => spvc_backend.Msl,
            GraphicsBackend.OpenGL => spvc_backend.Glsl,
            GraphicsBackend.OpenGLES => spvc_backend.Glsl,
            _ => throw new InvalidOperationException()
        };

        spvc_context_create_compiler(context, spvcBackend, ir, spvc_capture_mode.TakeOwnership, &compiler);

        spvc_compiler_options compilerOptions;
        spvc_compiler_create_compiler_options(compiler, &compilerOptions);

        switch (target)
        {
            case GraphicsBackend.Direct3D11:
                spvc_compiler_options_set_uint(compilerOptions, spvc_compiler_option.HlslShaderModel, 50);
                spvc_compiler_options_set_bool(compilerOptions, spvc_compiler_option.HlslPointSizeCompat, true);
                break;

            case GraphicsBackend.OpenGL:
            case GraphicsBackend.OpenGLES:
                spvc_compiler_options_set_bool(compilerOptions, spvc_compiler_option.GlslEs, target == GraphicsBackend.OpenGLES);
                spvc_compiler_options_set_bool(compilerOptions, spvc_compiler_option.GlslEnable420packExtension, false);
                spvc_compiler_options_set_uint(compilerOptions, spvc_compiler_option.GlslVersion, (uint)(target == GraphicsBackend.OpenGL ? 330 : 300));
                break;
        }

        spvc_compiler_options_set_bool(compilerOptions, spvc_compiler_option.FlipVertexY, options.FlipVertexY);
        spvc_compiler_options_set_bool(compilerOptions, spvc_compiler_option.FixupDepthConvention, options.FixupDepthConvention);

        spvc_compiler_install_compiler_options(compiler, compilerOptions);

        return compiler;
    }

    private static unsafe void ClassifyResource(
        spvc_compiler compiler,
        spvc_reflected_resource resource,
        bool image,
        bool storage,
        out ResourceKind resourceKind,
        out int size)
    {
        size = 0;

        var type = spvc_compiler_get_type_handle(compiler, resource.type_id);

        //var nonWritable = spvc_compiler_get_decoration(compiler, (SpvId)resource.id, SpvDecoration.SpvDecorationNonWritable);

        var baseType = spvc_type_get_basetype(type);
        switch (baseType)
        {
            case spvc_basetype.Struct:
                if (storage)
                {
                    SpvDecoration* decorations;
                    nuint numDecorations;
                    spvc_compiler_get_buffer_block_decorations(compiler, resource.id, (SpvDecoration*)&decorations, &numDecorations);
                    for (nuint i = 0; i < numDecorations; i++)
                    {
                        if (decorations[i] == SpvDecoration.SpvDecorationNonWritable)
                        {
                            resourceKind = ResourceKind.StructuredBufferReadOnly;
                            return;
                        }
                    }
                    resourceKind = ResourceKind.StructuredBufferReadWrite;
                    return;
                }
                else
                {
                    resourceKind = ResourceKind.UniformBuffer;
                    nuint structSize = 0;
                    spvc_compiler_get_declared_struct_size(compiler, type, &structSize);
                    size = (int)structSize;

                    var baseTypeId = spvc_type_get_base_type_id(type);
                    var baseTypeHandle = spvc_compiler_get_type_handle(compiler, baseTypeId);



                    var numMemberTypes = spvc_type_get_num_member_types(baseTypeHandle);
                    for (var i = 0u; i < numMemberTypes; i++)
                    {
                        var memberName = GetString(spvc_compiler_get_member_name(compiler, baseTypeId, i));
                        //spvc_type_get_member_type(baseTypeHandle, i);
                    }

                    return;
                }

            case spvc_basetype.Image:
                resourceKind = storage ? ResourceKind.TextureReadWrite : ResourceKind.TextureReadOnly;
                return;

            case spvc_basetype.Sampler:
                resourceKind = ResourceKind.Sampler;
                return;

            default:
                throw new InvalidOperationException();
        }
    }

    private static VertexElementDescription[] GetVertexElements(spvc_compiler vsCompiler, spvc_resources vsResources)
    {
        spvc_reflected_resource* resourceList;
        nuint resourceCount;
        spvc_resources_get_resource_list_for_type(vsResources, spvc_resource_type.StageInput, (spvc_reflected_resource*)&resourceList, &resourceCount);

        var elementCount = 0u;
        for (var i = 0u; i < resourceCount; i++)
        {
            var location = spvc_compiler_get_decoration(vsCompiler, (SpvId)resourceList[i].id, SpvDecoration.SpvDecorationLocation);
            elementCount = Math.Max(elementCount, location + 1);
        }

        var result = new VertexElementDescription[elementCount];

        for (var i = 0u; i < resourceCount; i++)
        {
            ref readonly var resource = ref resourceList[i];

            var location = spvc_compiler_get_decoration(vsCompiler, (SpvId)resource.id, SpvDecoration.SpvDecorationLocation);

            ref var vertexElement = ref result[i];

            vertexElement.Semantic = VertexElementSemantic.TextureCoordinate;
            vertexElement.Name = GetString(spvc_compiler_get_name(vsCompiler, (SpvId)resource.id));

            var baseTypeHandle = spvc_compiler_get_type_handle(vsCompiler, resource.base_type_id);
            var baseType = spvc_type_get_basetype(baseTypeHandle);

            var vectorSize = spvc_type_get_vector_size(baseTypeHandle);

            switch (baseType)
            {
                case spvc_basetype.Fp32:
                    vertexElement.Format = FloatFormats[vectorSize];
                    break;

                case spvc_basetype.Int32:
                    vertexElement.Format = IntFormats[vectorSize];
                    break;

                case spvc_basetype.Uint32:
                    vertexElement.Format = UIntFormats[vectorSize];
                    break;

                default:
                    throw new InvalidOperationException();
            }
        }

        return result;
    }

    private static readonly VertexElementFormat[] FloatFormats =
    {
        VertexElementFormat.Float1,
        VertexElementFormat.Float1,
        VertexElementFormat.Float2,
        VertexElementFormat.Float3,
        VertexElementFormat.Float4,
    };

    private static readonly VertexElementFormat[] IntFormats =
    {
        VertexElementFormat.Int1,
        VertexElementFormat.Int1,
        VertexElementFormat.Int2,
        VertexElementFormat.Int3,
        VertexElementFormat.Int4,
    };

    private static readonly VertexElementFormat[] UIntFormats =
    {
        VertexElementFormat.UInt1,
        VertexElementFormat.UInt1,
        VertexElementFormat.UInt2,
        VertexElementFormat.UInt3,
        VertexElementFormat.UInt4,
    };

    private static ResourceLayoutDescription[] CreateResourceLayouts(
        Dictionary<BindingInfo, ResourceInfo> resources)
    {
        //var currentSet = 0u;
        var setSizes = new List<uint>();
        foreach (var bindingInfo in resources.Keys)
        {
            var set = (int)bindingInfo.Set;
            while (setSizes.Count <= set)
            {
                setSizes.Add(0);
            }
            setSizes[set] = Math.Max(setSizes[set], bindingInfo.Binding + 1);
        }

        var result = new ResourceLayoutDescription[setSizes.Count];

        for (var i = 0; i < result.Length; i++)
        {
            var resourceLayout = new ResourceLayoutDescription
            {
                Elements = new ResourceLayoutElementDescription[setSizes[i]]
            };
            for (var j = 0; j < resourceLayout.Elements.Length; j++)
            {
                resourceLayout.Elements[j] = new ResourceLayoutElementDescription
                {
                    Name = "",
                    Kind = ResourceKind.UniformBuffer,
                    Stages = ShaderStages.None,
                    Options = (ResourceLayoutElementOptions)2, // Unused
                };
            }
            result[i] = resourceLayout;
        }

        foreach (var resourceBinding in resources)
        {
            var resourceInfo = resourceBinding.Value;

            var stages = ShaderStages.None;
            if (resourceInfo.IDs[0] != 0)
            {
                stages |= ShaderStages.Vertex;
            }
            if (resourceInfo.IDs[1] != 0)
            {
                stages |= ShaderStages.Fragment;
            }

            ref var resourceElement = ref result[resourceBinding.Key.Set].Elements[resourceBinding.Key.Binding];

            resourceElement.Name = resourceInfo.Name;
            resourceElement.Kind = resourceInfo.Kind;
            resourceElement.Stages = stages;
            resourceElement.Options = ResourceLayoutElementOptions.None;
        }

        return result;
    }

    private struct BindingInfo
    {
        public uint Set;
        public uint Binding;
    }

    public unsafe struct ResourceInfo
    {
        public string Name;
        public ResourceKind Kind;
        public fixed uint IDs[2]; // 0 == VS/CS, 1 == FS
        public int Size;
    }

    private static unsafe void AddResources(
        spvc_compiler compiler,
        spvc_resources resources,
        spvc_resource_type resourceType,
        Dictionary<BindingInfo, ResourceInfo> allResources,
        uint idIndex,
        bool image = false,
        bool storage = false)
    {
        spvc_reflected_resource* resourceList;
        nuint resourceCount;
        spvc_resources_get_resource_list_for_type(resources, resourceType, (spvc_reflected_resource*)&resourceList, &resourceCount);

        for (uint i = 0; i < resourceCount; i++)
        {
            var resource = resourceList[i];

            var bindingInfo = new BindingInfo
            {
                Set = spvc_compiler_get_decoration(compiler, (SpvId)resource.id, SpvDecoration.SpvDecorationDescriptorSet),
                Binding = spvc_compiler_get_decoration(compiler, (SpvId)resource.id, SpvDecoration.SpvDecorationBinding)
            };

            // Note that we don't use resource.name, because that returns the remapped block name,
            // which is not what we want when the source was HLSL.
            var resourceName = GetString(spvc_compiler_get_name(compiler, (SpvId)resource.id));

            ClassifyResource(compiler, resource, image, storage, out var resourceKind, out var resourceSize);

            var resourceInfo = new ResourceInfo
            {
                Name = resourceName,
                Kind = resourceKind,
                Size = resourceSize,
            };

            resourceInfo.IDs[idIndex] = resource.id;

            if (!allResources.TryAdd(bindingInfo, resourceInfo))
            {
                // Insertion failed; element already exists.
                var existingResourceInfo = allResources[bindingInfo];

                if (existingResourceInfo.IDs[idIndex] != 0)
                {
                    throw new InvalidOperationException($"The same binding slot ({bindingInfo.Set}, {bindingInfo.Binding}) was used by multiple distinct resources. First resource: {existingResourceInfo.Name}. Second resource: {resourceInfo.Name}");
                }

                existingResourceInfo.IDs[idIndex] = resource.id;
                if (existingResourceInfo.Kind != resourceInfo.Kind)
                {
                    throw new InvalidOperationException($"The same binding slot ({bindingInfo.Set}, {bindingInfo.Binding}) was used by multiple resources with incompatible types: {existingResourceInfo.Kind} and {resourceInfo.Kind}");
                }
            }
        }
    }

    private static unsafe string GetString(byte* ptr)
    {
        int length = 0;
        while (ptr[length] != 0)
        {
            length++;
        }
        return Encoding.UTF8.GetString(ptr, length);
    }

    private static byte[] GetBytes(string value)
    {
        return Encoding.UTF8.GetBytes(value);
    }

    private static unsafe byte[] GetBytes(byte* ptr)
    {
        int length = 0;
        while (ptr[length] != 0)
        {
            length++;
        }
        var source = new Span<byte>(ptr, length);
        var result = new byte[length];
        source.CopyTo(result);
        return result;
    }
}

public sealed class SpirvCrossCompilationResult
{
    public readonly VertexElementDescription[] VertexElements;
    public readonly ResourceLayoutDescription[] ResourceLayouts;

    public readonly byte[] VsCode;
    public readonly byte[] FsCode;

    public SpirvCrossCompilationResult(
        VertexElementDescription[] vertexElements,
        ResourceLayoutDescription[] resourceLayouts,
        byte[] vsCode,
        byte[] fsCode)
    {
        VertexElements = vertexElements;
        ResourceLayouts = resourceLayouts;
        VsCode = vsCode;
        FsCode = fsCode;
    }
}
