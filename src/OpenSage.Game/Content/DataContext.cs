using System;
using System.Collections.Generic;
using OpenSage.Graphics;
using OpenSage.Graphics.Animation;
using Veldrid;

namespace OpenSage.Content
{
    internal sealed class ContentScope : DisposableBase
    {
        public Dictionary<string, Animation> Animations { get; } = new Dictionary<string, Animation>();
        public Dictionary<string, Data.Ini.MappedImage> MappedImages { get; } = new Dictionary<string, Data.Ini.MappedImage>();
        public ResourceCollection<Model> Models { get; } = new ResourceCollection<Model>();
        //public ResourceCollection<ModelMesh> ModelMeshes { get; } = new ResourceCollection<ModelMesh>();
        public Dictionary<string, ModelBoneHierarchy> ModelBoneHierarchies { get; } = new Dictionary<string, ModelBoneHierarchy>();
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

        public void AddResource(string path, T resource, bool disposable = true)
        {
            if (disposable)
            {
                AddDisposable(resource);
            }
            _resources.Add(path, resource);
        }
    }
}
