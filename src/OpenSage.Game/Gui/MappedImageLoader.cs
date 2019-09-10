using System.Linq;
using OpenSage.Content;
using OpenSage.Content.Util;

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

            var texture = _contentManager.GetGuiTexture(mappedImage.Texture);

            var textureRect = mappedImage.Coords.ToRectangle();

            return new MappedImageTexture(texture, textureRect);
        }
    }
}
