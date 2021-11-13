using OpenSage.FileFormats.W3d;
using OpenSage.Graphics;
using OpenSage.IO;

namespace OpenSage.Content.Loaders
{
    internal sealed class OnDemandModelBoneHierarchyLoader : IOnDemandAssetLoader<ModelBoneHierarchy>
    {
        private readonly IPathResolver _pathResolver;

        public OnDemandModelBoneHierarchyLoader(IPathResolver pathResolver)
        {
            _pathResolver = pathResolver;
        }

        public ModelBoneHierarchy Load(string name, AssetLoadContext context)
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
}
