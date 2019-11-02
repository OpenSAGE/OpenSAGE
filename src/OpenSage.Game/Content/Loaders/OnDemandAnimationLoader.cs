using System;
using OpenSage.Data.IO;
using OpenSage.FileFormats.W3d;
using OpenSage.Graphics.Animation;

namespace OpenSage.Content.Loaders
{
    internal sealed class OnDemandAnimationLoader : IOnDemandAssetLoader<W3DAnimation>
    {
        private readonly IPathResolver _pathResolver;
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public OnDemandAnimationLoader(IPathResolver pathResolver)
        {
            _pathResolver = pathResolver;
        }

        public W3DAnimation Load(string key, AssetLoadContext context)
        {
            var splitName = key.Split('.');

            if (splitName.Length <= 1)
            {
                return null;
            }

            // Find it in the file system.
            string url = null;
            foreach (var path in _pathResolver.GetPaths(splitName[1], context.Language))
            {
                if (FileSystem.FileExists(path))
                {
                    url = path;
                    break;
                }
            }

            if (url is null)
            {
                logger.Warn("Failed to load animation: " + key);
                return null;
            }

            // Load animation.
            W3dFile w3dFile;
            using (var entryStream = FileSystem.OpenStream(url, FileMode.Open))
            {
                w3dFile = W3dFile.FromStream(entryStream, url);
            }
            var animation = W3DAnimation.FromW3dFile(w3dFile);
            if (!string.Equals(animation.Name, key, StringComparison.OrdinalIgnoreCase))
            {
                throw new NotSupportedException();
            }

            return animation;
        }
    }
}
