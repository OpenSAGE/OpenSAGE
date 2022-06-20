using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OpenSage.FileFormats.W3d;
using OpenSage.Graphics;
using OpenSage.IO;

namespace OpenSage.Content.Loaders
{

    internal sealed class OnDemandModelLoader : IOnDemandAssetLoader<Model>
    {
        private readonly IPathResolver _pathResolver;

        public OnDemandModelLoader(IPathResolver pathResolver)
        {
            _pathResolver = pathResolver;
        }

        public Model Load(string name, AssetLoadContext context)
        {
            // Find it in the file system.
            FileSystemEntry entry = null;
            foreach (var path in _pathResolver.GetPaths(name, context.Language))
            {
                entry = context.FileSystem.GetFile(path);
                if (entry != null)
                {
                    break;
                }
            }

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

            var w3dHLod = w3dFile.HLod;
            var w3dHierarchy = w3dFile.Hierarchy;
            ModelBoneHierarchy boneHierarchy;
            if (w3dHierarchy != null)
            {
                boneHierarchy = new ModelBoneHierarchy(w3dHierarchy);
            }
            else if (w3dHLod != null)
            {
                // Load referenced hierarchy.
                boneHierarchy = context.AssetStore.ModelBoneHierarchies.GetByName(w3dHLod.Header.HierarchyName);
            }
            else
            {
                boneHierarchy = ModelBoneHierarchy.CreateDefault();
            }

            return CreateModel(
                context,
                w3dFile,
                boneHierarchy);
        }

        private static Model CreateModel(
            AssetLoadContext context,
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
                    
                    var bone = boneHierarchy.Bones[(int) w3dSubObject.BoneIndex];

                    //var meshBoundingSphere = mesh.BoundingSphere.Transform(bone.Transform);

                    //boundingSphere = (i == 0)
                    //    ? meshBoundingSphere
                    //    : BoundingSphere.CreateMerged(boundingSphere, meshBoundingSphere);

                    subObjects.Add(
                        CreateSubObject(
                            w3dSubObject.Name,
                            w3dRenderableObject,
                            bone,
                            context));
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
                        boneHierarchy.Bones[0],
                        context));
            }
            else
            {
                
            }

            return new Model(
                Path.GetFileNameWithoutExtension(w3dFile.FilePath),
                boneHierarchy,
                subObjects.ToArray());
        }

        private static ModelSubObject CreateSubObject(
            string fullName,
            W3dChunk w3dRenderableObject,
            ModelBone bone,
            AssetLoadContext context)
        {
            return w3dRenderableObject switch
            {
                W3dMesh w3dMesh => new ModelSubObject(fullName, w3dMesh.Header.MeshName, bone, new ModelMesh(w3dMesh, context)),

                W3dBox w3dBox => new ModelSubObject(fullName, bone, new ModelBox(w3dBox, context)),

                _ => throw new ArgumentOutOfRangeException(nameof(w3dRenderableObject)),
            };
        }
    }
}
