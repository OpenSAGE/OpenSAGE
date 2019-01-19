using System.IO;
using System.Linq;
using OpenSage.Content;
using OpenSage.Content.Util;
using Veldrid;

namespace OpenSage.Gui
{
    public sealed class MappedImageLoader
    {
        private readonly ContentManager _contentManager;

        public MappedImageLoader(ContentManager contentManager)
        {
            _contentManager = contentManager;
        }

        public MappedImageTexture GetMappedImage(string mappedImageName)
        {
            var mappedImage = _contentManager.IniDataContext.MappedImages.FirstOrDefault(x => x.Name == mappedImageName);
            if (mappedImage == null)
            {
                return null;
            }


            var texture = _contentManager.Load<Texture>(
                new[]
                {
                    Path.Combine("Data", _contentManager.Language, "Art", "Textures", mappedImage.Texture),
                    Path.Combine("Lang", _contentManager.Language, "Art", "Textures", mappedImage.Texture),
                    Path.Combine("Art", "Textures", mappedImage.Texture),
                    Path.Combine("Art", "Compiledtextures",  mappedImage.Texture.Substring(0,2) , mappedImage.Texture)
                },
                new TextureLoadOptions { GenerateMipMaps = false });

            var textureRect = mappedImage.Coords.ToRectangle();

            return new MappedImageTexture(texture, textureRect);
        }
    }
}
