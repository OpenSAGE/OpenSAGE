using System;
using System.Collections.Generic;
using Veldrid;

namespace OpenSage.Rendering;

public sealed class ShaderSetStore : DisposableBase
{
    private readonly Dictionary<Type, ShaderSet> _cache = new();

    private byte _nextId;

    public readonly GraphicsDevice GraphicsDevice;
    public readonly OutputDescription OutputDescription;

    public ShaderSetStore(GraphicsDevice graphicsDevice, OutputDescription outputDescription)
    {
        GraphicsDevice = graphicsDevice;
        OutputDescription = outputDescription;
    }

    public T GetShaderSet<T>(Func<T> createShaderSet)
        where T : ShaderSet
    {
        var key = typeof(T);

        if (!_cache.TryGetValue(key, out var result))
        {
            result = AddDisposable(createShaderSet());
            _cache.Add(key, result);
        }

        return (T)result;
    }

    internal byte GetNextId()
    {
        return checked(_nextId++);
    }
}
