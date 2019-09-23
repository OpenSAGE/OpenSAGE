using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OpenSage.Data;
using OpenSage.FileFormats.W3d;
using OpenSage.Graphics;

namespace OpenSage.Content.Loaders
{
    public sealed class OnDemandModelBoneHierarchyLoader : IOnDemandAssetLoader<string, ModelBoneHierarchy>
    {
        private readonly IPathResolver _pathResolver;

        public OnDemandModelBoneHierarchyLoader(IPathResolver pathResolver)
        {
            _pathResolver = pathResolver;
        }

        ModelBoneHierarchy IOnDemandAssetLoader<string, ModelBoneHierarchy>.Load(string name, AssetLoadContext context)
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

            // Load hierarchy.
            W3dFile hierarchyFile;
            using (var entryStream = entry.Open())
            {
                hierarchyFile = W3dFile.FromStream(entryStream, entry.FilePath);
            }
            var w3dHierarchy = hierarchyFile.GetHierarchy();
            return w3dHierarchy != null
                ? new ModelBoneHierarchy(w3dHierarchy)
                : ModelBoneHierarchy.CreateDefault();
        }
    }

    public sealed class OnDemandModelLoader : IOnDemandAssetLoader<string, Model>
    {
        private readonly IPathResolver _pathResolver;

        public OnDemandModelLoader(IPathResolver pathResolver)
        {
            _pathResolver = pathResolver;
        }

        Model IOnDemandAssetLoader<string, Model>.Load(string name, AssetLoadContext context)
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

            // Load model.
            W3dFile w3dFile;
            using (var entryStream = entry.Open())
            {
                w3dFile = W3dFile.FromStream(entryStream, entry.FilePath);
            }

            var w3dHLod = w3dFile.GetHLod();
            var w3dHierarchy = w3dFile.GetHierarchy();
            ModelBoneHierarchy boneHierarchy;
            if (w3dHierarchy != null)
            {
                boneHierarchy = new ModelBoneHierarchy(w3dHierarchy);
            }
            else if (w3dHLod != null && w3dHierarchy == null)
            {
                // Load referenced hierarchy.
                boneHierarchy = context.AssetStore.ModelBoneHierarchies.GetByKey(w3dHLod.Header.HierarchyName);
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

            var w3dMeshes = w3dFile.GetMeshes();
            var w3dHLod = w3dFile.GetHLod();

            var subObjects = new List<ModelSubObject>();

            if (w3dHLod != null)
            {
                foreach (var w3dSubObject in w3dHLod.Lods[0].SubObjects)
                {
                    // TODO: Collision boxes
                    var w3dMesh = w3dMeshes.FirstOrDefault(x => x.Header.ContainerName + "." + x.Header.MeshName == w3dSubObject.Name);
                    if (w3dMesh == null)
                    {
                        continue;
                    }

                    var bone = boneHierarchy.Bones[(int) w3dSubObject.BoneIndex];

                    var mesh = new ModelMesh(w3dMesh, context);

                    //var meshBoundingSphere = mesh.BoundingSphere.Transform(bone.Transform);

                    //boundingSphere = (i == 0)
                    //    ? meshBoundingSphere
                    //    : BoundingSphere.CreateMerged(boundingSphere, meshBoundingSphere);

                    subObjects.Add(new ModelSubObject(w3dSubObject.Name, bone, mesh));
                }
            }
            else if (w3dMeshes.Count > 0)
            {
                // Simple models can have only one mesh with no HLod chunk.
                if (w3dMeshes.Count != 1)
                {
                    throw new InvalidOperationException();
                }

                var w3dMesh = w3dMeshes[0];

                var mesh = new ModelMesh(w3dMesh, context);

                subObjects.Add(new ModelSubObject(
                    w3dMesh.Header.MeshName,
                    boneHierarchy.Bones[0],
                    mesh));
            }
            else
            {
                // TODO: Some .w3d files contain a single W3D_BOX.
            }

            return new Model(
                Path.GetFileNameWithoutExtension(w3dFile.FilePath),
                boneHierarchy,
                subObjects.ToArray());
        }
    }
}
