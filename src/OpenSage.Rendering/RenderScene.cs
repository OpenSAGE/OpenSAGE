using System;
using System.Collections.Generic;
using Veldrid;

namespace OpenSage.Rendering;

public sealed class RenderScene
{
    private readonly List<RenderableObject> _renderables = new();

    private readonly RenderList _forwardPassList = new();
    private readonly RenderList _shadowPassList = new();

    public void AddObject(RenderableObject renderable)
    {
        _renderables.Add(renderable);

        // TODO: Extract child objects and store in appropriate render buckets (opaque / transparent / etc.)
    }
}

public abstract class RenderableObject
{
    public readonly List<RenderableObject> ChildObjects = new();

    public abstract SurfaceType SurfaceType { get; }

    public abstract ulong RenderKey { get; } // ShaderSet, Pipeline, Material, SortPriority

    //public abstract int SortPriority { get; } // Needs to be lower for Road than for ParticleSystem

    public abstract bool CastsShadow { get; }
}

public readonly struct RenderKey
{
    private readonly ulong _key;

    // TODO: Also material constants?

    public RenderKey(
        SurfaceType surfaceType,
        ShaderSet shaderSet,
        Pipeline pipeline)
        // TODO: Also material constants?
    {
        // TODO
        _key = 0;
    }
}

public sealed class RenderContext
{
}

public sealed class MaterialDefinitionStore : DisposableBase
{
    private readonly Dictionary<Type, MaterialDefinition> _definitions = new();

    private byte _nextMaterialDefinitionId;

    public readonly GraphicsDevice GraphicsDevice;
    public readonly OutputDescription OutputDescription;

    public MaterialDefinitionStore(GraphicsDevice graphicsDevice, OutputDescription outputDescription)
    {
        GraphicsDevice = graphicsDevice;
        OutputDescription = outputDescription;
    }

    public T GetMaterialDefinition<T>(Func<T> createMaterialDefinition)
        where T : MaterialDefinition
    {
        var key = typeof(T);

        if (!_definitions.TryGetValue(key, out var result))
        {
            result = AddDisposable(createMaterialDefinition());
            _definitions.Add(key, result);
        }

        return (T)result;
    }

    internal byte GetNextMaterialDefinitionId()
    {
        return checked(_nextMaterialDefinitionId++);
    }
}

public abstract class MaterialDefinition : DisposableBase
{
    private byte _nextMaterialId;

    public readonly byte Id;
    public readonly GraphicsDevice GraphicsDevice;
    public readonly ShaderSet ShaderSet;

    protected MaterialDefinition(
        MaterialDefinitionStore store,
        string shaderName,
        GlobalResourceSetIndices globalResourceSetIndices,
        params VertexLayoutDescription[] vertexDescriptors)
    {
        Id = store.GetNextMaterialDefinitionId();
        GraphicsDevice = store.GraphicsDevice;
        ShaderSet = AddDisposable(new ShaderSet(store.GraphicsDevice.ResourceFactory, shaderName, globalResourceSetIndices, vertexDescriptors));
    }

    internal byte GetNextMaterialId()
    {
        return checked(_nextMaterialId++);
    }
}

public abstract class Material : DisposableBase
{
    public readonly byte Id;

    public readonly MaterialDefinition Definition;
    public readonly GraphicsDevice GraphicsDevice;
    public readonly Pipeline Pipeline;

    protected Material(MaterialDefinition definition, Pipeline pipeline)
    {
        Id = definition.GetNextMaterialId();

        Definition = definition;
        GraphicsDevice = definition.GraphicsDevice;
        Pipeline = pipeline;
    }

    public ulong RenderKey { get; } // TODO

    public void Apply(CommandList commandList, RenderContext context)
    {
        commandList.SetPipeline(Pipeline);

        ApplyCore(commandList, context);
    }

    protected abstract void ApplyCore(CommandList commandList, RenderContext context);
}

public enum SurfaceType
{
    Opaque,
    Transparent,
}

public enum RenderBucketType
{
    Terrain,
    Road,
    Opaque,
    Transparent,
    Water,
}

internal sealed class RenderList
{
    public readonly List<RenderableObject> Opaque = new();
    public readonly List<RenderableObject> Transparent = new();
}
