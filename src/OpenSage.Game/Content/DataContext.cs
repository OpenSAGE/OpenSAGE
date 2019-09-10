using System;
using System.Collections.Generic;
using OpenSage.Graphics.Animation;
using Veldrid;

namespace OpenSage.Content
{
    /// <summary>
    /// Eventually all game data will live in this hierarchy of objects,
    /// with global or map-specific scope.
    /// </summary>
    public sealed class DataContext
    {
        public Dictionary<string, Animation> Animations { get; } = new Dictionary<string, Animation>(StringComparer.OrdinalIgnoreCase);
    }

    internal sealed class ContentScope : DisposableBase
    {
        public ResourceCollection<Texture> Textures { get; } = new ResourceCollection<Texture>();
    }

    internal sealed class ResourceCollection<T> : DisposableBase
        where T : IDisposable
    {
        private readonly Dictionary<string, T> _resources = new Dictionary<string, T>();

        public bool TryGetResource(string path, out T resource)
        {
            return _resources.TryGetValue(path, out resource);
        }

        public void AddResource(string path, T resource, bool disposable)
        {
            if (disposable)
            {
                AddDisposable(resource);
            }
            _resources.Add(path, resource);
        }
    }
}
