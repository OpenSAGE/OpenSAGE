using System;
using System.Collections.Generic;
using OpenSage.Core.Graphics;
using Veldrid;

namespace OpenSage.Rendering;

public sealed class ShaderSetStore : DisposableBase
{
    private readonly Dictionary<Type, ShaderSet> _cache = new();

    private byte _nextId;

    public readonly GraphicsDeviceManager GraphicsDeviceManager;
    public readonly OutputDescription OutputDescription;

    public GraphicsDevice GraphicsDevice => GraphicsDeviceManager.GraphicsDevice;

    public ShaderSetStore(GraphicsDeviceManager graphicsDeviceManager, OutputDescription outputDescription)
    {
        GraphicsDeviceManager = graphicsDeviceManager;
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
