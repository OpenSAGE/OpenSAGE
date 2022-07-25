using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OpenSage.FileFormats.W3d;
using OpenSage.Graphics;
using OpenSage.Graphics.Animation;
using OpenSage.Graphics.Shaders;
using OpenSage.IO;
using OpenSage.Rendering;
using Veldrid;

namespace OpenSage.Core.Graphics.W3d;

public sealed class W3dAssetCache : DisposableBase
{
    private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

    private readonly GraphicsDeviceManager _graphicsDeviceManager;
    private readonly FileSystem _fileSystem;
    private readonly IW3dPathResolver _pathResolver;
    private readonly TextureAssetCache _textureAssetCache;

    private readonly MeshShaderResources _meshShaderResources;
    private readonly FixedFunctionShaderResources _fixedFunctionShaderResources;
    private readonly MeshDepthShaderResources _meshDepthShaderResources;
    private readonly Dictionary<string, ShaderMaterialShaderResources> _shaderMaterialResources;

    private readonly Dictionary<uint, Model> _modelCache;
    private readonly Dictionary<uint, ModelBoneHierarchy> _modelBoneHierarchyCache;
    private readonly Dictionary<uint, W3DAnimation> _animationCache;

    public W3dAssetCache(
        GraphicsDeviceManager graphicsDeviceManager,
        FileSystem fileSystem,
        IW3dPathResolver pathResolver,
        TextureAssetCache textureAssetCache,
        ShaderSetStore shaderSetStore)
    {
        _graphicsDeviceManager = graphicsDeviceManager;
        _fileSystem = fileSystem;
        _pathResolver = pathResolver;
        _textureAssetCache = textureAssetCache;

        _meshShaderResources = AddDisposable(new MeshShaderResources(graphicsDeviceManager.GraphicsDevice));
        _fixedFunctionShaderResources = AddDisposable(new FixedFunctionShaderResources(shaderSetStore));
        _meshDepthShaderResources = AddDisposable(new MeshDepthShaderResources(shaderSetStore));

        _shaderMaterialResources = new Dictionary<string, ShaderMaterialShaderResources>
        {
            { "NormalMapped", AddDisposable(new NormalMappedShaderResources(shaderSetStore)) },
            { "Simple", AddDisposable(new SimpleShaderResources(shaderSetStore)) }
        };

        _modelCache = new Dictionary<uint, Model>();
        _modelBoneHierarchyCache = new Dictionary<uint, ModelBoneHierarchy>();
        _animationCache = new Dictionary<uint, W3DAnimation>();
    }

    public Model GetModelByName(string name)
    {
        return GetOrCreateW3dAsset(name, _modelCache, w3dFile =>
        {
            var w3dHLod = w3dFile.HLod;
            var w3dHierarchy = w3dFile.Hierarchy;
            ModelBoneHierarchy boneHierarchy;
            if (w3dHierarchy != null)
            {
                boneHierarchy = new ModelBoneHierarchy(w3dHierarchy);
            }
            else if (w3dHLod != null && w3dHierarchy == null)
            {
                // Load referenced hierarchy.
                boneHierarchy = GetModelBoneHierarchyByName(w3dHLod.Header.HierarchyName);
            }
            else
            {
                boneHierarchy = ModelBoneHierarchy.CreateDefault();
            }

            return CreateModel(w3dFile, boneHierarchy);
        });
    }

    private Model CreateModel(
        W3dFile w3dFile,
        ModelBoneHierarchy boneHierarchy)
    {
        //BoundingSphere boundingSphere = default(BoundingSphere);

        var w3dHLod = w3dFile.HLod;

        var subObjects = new List<ModelSubObject>();

        if (w3dHLod != null)
        {
            foreach (var w3dSubObject in w3dHLod.Lods[0].SubObjects)
            {
                if (!w3dFile.RenderableObjectsByName.TryGetValue(w3dSubObject.Name, out var w3dRenderableObject))
                {
                    continue;
                }

                var bone = boneHierarchy.Bones[(int)w3dSubObject.BoneIndex];

                //var meshBoundingSphere = mesh.BoundingSphere.Transform(bone.Transform);

                //boundingSphere = (i == 0)
                //    ? meshBoundingSphere
                //    : BoundingSphere.CreateMerged(boundingSphere, meshBoundingSphere);

                subObjects.Add(
                    CreateSubObject(
                        w3dSubObject.Name,
                        w3dRenderableObject,
                        bone));
            }
        }
        else if (w3dFile.RenderableObjects.Count > 0)
        {
            // Simple models can have only one mesh with no HLod chunk.
            if (w3dFile.RenderableObjects.Count != 1)
            {
                throw new InvalidOperationException();
            }

            var w3dRenderableObjectPair = w3dFile.RenderableObjectsByName.First();

            subObjects.Add(
                CreateSubObject(
                    w3dRenderableObjectPair.Key,
                    w3dRenderableObjectPair.Value,
                    boneHierarchy.Bones[0]));
        }
        else
        {

        }

        return new Model(
            Path.GetFileNameWithoutExtension(w3dFile.FilePath),
            boneHierarchy,
            subObjects.ToArray(),
            _graphicsDeviceManager);
    }

    private ModelSubObject CreateSubObject(
        string fullName,
        W3dChunk w3dRenderableObject,
        ModelBone bone)
    {
        return w3dRenderableObject switch
        {
            W3dMesh w3dMesh => new ModelSubObject(
                fullName,
                w3dMesh.Header.MeshName,
                bone,
                new ModelMesh(
                    w3dMesh,
                    _graphicsDeviceManager,
                    _fixedFunctionShaderResources,
                    _meshShaderResources,
                    _meshDepthShaderResources,
                    x => (Texture)_textureAssetCache.GetByName(x),
                    x => _shaderMaterialResources[x])),

            W3dBox w3dBox => new ModelSubObject(fullName, bone, new ModelBox(w3dBox)),

            _ => throw new ArgumentOutOfRangeException(nameof(w3dRenderableObject)),
        };
    }

    private ModelBoneHierarchy GetModelBoneHierarchyByName(string name)
    {
        return GetOrCreateW3dAsset(name, _modelBoneHierarchyCache, static w3dFile =>
        {
            var w3dHierarchy = w3dFile.Hierarchy;

            return w3dHierarchy != null
                ? new ModelBoneHierarchy(w3dHierarchy)
                : ModelBoneHierarchy.CreateDefault();
        });
    }

    public W3DAnimation GetAnimationByName(string name)
    {
        var splitName = name.Split('.');

        if (splitName.Length <= 1)
        {
            return null;
        }

        return GetOrCreateW3dAsset(splitName[1], _animationCache, w3dFile =>
        {
            var animation = W3DAnimation.FromW3dFile(w3dFile);

            if (animation == null)
            {
                Logger.Warn($"Failed to load animation (was null): {name}");
                return null;
            }

            if (!string.Equals(animation.Name, name, StringComparison.OrdinalIgnoreCase))
            {
                Logger.Warn($"Animation name '{animation.Name}' does not match '{name}'");
            }

            return animation;
        });
    }

    private T GetOrCreateW3dAsset<T>(string name, Dictionary<uint, T> cache, Func<W3dFile, T> createAsset)
        where T : BaseAsset
    {
        var instanceId = AssetHash.GetHash(name);
        if (cache.TryGetValue(instanceId, out var result))
        {
            return result;
        }

        // Find it in the file system.
        var path = _pathResolver.GetPath(name);
        var entry = _fileSystem.GetFile(path);

        if (entry == null)
        {
            return null;
        }

        // Load model.
        W3dFile w3dFile;
        using (var entryStream = entry.Open())
        {
            w3dFile = W3dFile.FromStream(entryStream, entry.FilePath);
        }

        var w3dAsset = createAsset(w3dFile);

        cache.Add(instanceId, w3dAsset);

        return AddDisposable(w3dAsset);
    }

    public IEnumerable<BaseAsset> GetAssets()
    {
        foreach (var asset in _modelCache.Values)
        {
            yield return asset;
        }
        foreach (var asset in _modelBoneHierarchyCache.Values)
        {
            yield return asset;
        }
        foreach (var asset in _animationCache.Values)
        {
            yield return asset;
        }
    }
}

public interface IW3dPathResolver
{
    string GetPath(string name);
}

public static class W3dPathResolvers
{
    public static readonly IW3dPathResolver Standard = new StandardW3dPathResolver();
    public static readonly IW3dPathResolver Bfme2 = new Bfme2W3dPathResolver();

    private sealed class StandardW3dPathResolver : IW3dPathResolver
    {
        public string GetPath(string name)
        {
            return Path.Combine("art", "w3d", name + ".w3d");
        }
    }

    private sealed class Bfme2W3dPathResolver : IW3dPathResolver
    {
        public string GetPath(string name)
        {
            return Path.Combine("art", "w3d", name.Substring(0, 2), name + ".w3d");
        }
    }
}
