using OpenSage.Data.IO;
using OpenSage.FileFormats.W3d;
using OpenSage.Graphics;

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
            string url = null;
            foreach (var path in _pathResolver.GetPaths(name, context.Language))
            {
                if (FileSystem.FileExists(path))
                {
                    url = path;
                    break;
                }
            }

            // Load hierarchy.
            W3dFile hierarchyFile;
            using (var entryStream = FileSystem.OpenStream(url, FileMode.Open))
            {
                hierarchyFile = W3dFile.FromStream(entryStream, url);
            }
            var w3dHierarchy = hierarchyFile.GetHierarchy();
            return w3dHierarchy != null
                ? new ModelBoneHierarchy(w3dHierarchy)
                : ModelBoneHierarchy.CreateDefault();
        }
    }
}
