using System;
using OpenSage.Data;
using OpenSage.FileFormats.W3d;
using OpenSage.Graphics.Animation;

namespace OpenSage.Content.Loaders
{
    public sealed class OnDemandAnimationLoader : IOnDemandAssetLoader<string, Animation>
    {
        private readonly IPathResolver _pathResolver;

        public OnDemandAnimationLoader(IPathResolver pathResolver)
        {
            _pathResolver = pathResolver;
        }

        Animation IOnDemandAssetLoader<string, Animation>.Load(string key, AssetLoadContext context)
        {
            var splitName = key.Split('.');

            if (splitName.Length <= 1)
            {
                return null;
            }

            // Find it in the file system.
            FileSystemEntry entry = null;
            foreach (var path in _pathResolver.GetPaths(splitName[1], context.Language))
            {
                entry = context.FileSystem.GetFile(path);
                if (entry != null)
                {
                    break;
                }
            }

            // Load animation.
            W3dFile w3dFile;
            using (var entryStream = entry.Open())
            {
                w3dFile = W3dFile.FromStream(entryStream, entry.FilePath);
            }
            var animation = Animation.FromW3dFile(w3dFile);
            if (!string.Equals(animation.Name, key, StringComparison.OrdinalIgnoreCase))
            {
                throw new NotSupportedException();
            }

            return animation;
        }
    }
}
