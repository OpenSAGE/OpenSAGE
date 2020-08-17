﻿using System.Collections.Generic;
using System.IO;

namespace OpenSage.Content
{
    public static class PathResolvers
    {
        public static readonly IPathResolver W3d = new StandardW3dPathResolver();
        public static readonly IPathResolver Bfme2W3d = new Bfme2W3dPathResolver();
        public static readonly IPathResolver GeneralsTexture = new StandardTexturePathResolver();
        public static readonly IPathResolver BfmeTexture = new BfmeTexturePathResolver();
        public static readonly IPathResolver Bfme2Texture = new Bfme2TexturePathResolver();

        private sealed class StandardW3dPathResolver : IPathResolver
        {
            public IEnumerable<string> GetPaths(string name, string language)
            {
                yield return Path.Combine("art", "w3d", name + ".w3d");
            }
        }

        private sealed class Bfme2W3dPathResolver : IPathResolver
        {
            public IEnumerable<string> GetPaths(string name, string language)
            {
                yield return Path.Combine("art", "w3d", name.Substring(0, 2), name + ".w3d");
            }
        }

        private sealed class StandardTexturePathResolver : IPathResolver
        {
            public IEnumerable<string> GetPaths(string name, string language)
            {
                yield return Path.Combine("data", language.ToLowerInvariant(), "art", "textures", name);

                yield return Path.Combine("art", "textures", name); // Normal and APT

                // TODO: This is only here to load map preview images, see if there's a better way.
                yield return name;
            }
        }

        private sealed class BfmeTexturePathResolver : IPathResolver
        {
            public IEnumerable<string> GetPaths(string name, string language)
            {
                yield return Path.Combine("lang", language.ToLowerInvariant(), "art", "textures", name);

                yield return Path.Combine("art", "textures", name); // Normal and APT

                // TODO: This is only here to load map preview images, see if there's a better way.
                yield return name;
            }
        }

        private sealed class Bfme2TexturePathResolver : IPathResolver
        {
            public IEnumerable<string> GetPaths(string name, string language)
            {
                yield return Path.Combine("art", "compiledtextures", name.Substring(0, 2), name);
                yield return Path.Combine("art", "textures", name); // APT only

                // TODO: This is only here to load map preview images, see if there's a better way.
                yield return name;
            }
        }
    }

    public interface IPathResolver
    {
        IEnumerable<string> GetPaths(string name, string language);
    }
}
