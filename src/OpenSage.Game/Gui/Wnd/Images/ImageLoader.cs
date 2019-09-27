using OpenSage.Content;
using OpenSage.Data.Wnd;
using OpenSage.Gui.Wnd.Controls;
using Veldrid;

namespace OpenSage.Gui.Wnd.Images
{
    public sealed class ImageLoader
    {
        private readonly ImageTextureCache _textureCache;

        internal ImageLoader(ImageTextureCache textureCache)
        {
            _textureCache = textureCache;
        }

        public Image CreateFromTexture(Texture texture)
        {
            return new Image(texture.Name, new TextureSource(texture));
        }

        public Image CreateFromMappedImageReference(LazyAssetReference<MappedImage> mappedImageReference)
        {
            var mappedImage = mappedImageReference?.Value;
            if (mappedImage != null)
            {
                return new Image(mappedImage.Name, new MappedImageSource(mappedImage, _textureCache));
            }
            else
            {
                return null;
            }
        }

        public Image CreateFromWndDrawData(WndDrawData wndDrawData, int index)
        {
            return CreateFromMappedImageReference(wndDrawData.Items[index].Image);
        }

        public Image CreateFromStretchableWndDrawData(WndDrawData wndDrawData, int leftIndex, int middleIndex, int rightIndex)
        {
            var leftImage = wndDrawData.Items[leftIndex].Image?.Value;
            var middleImage = wndDrawData.Items[middleIndex].Image?.Value;
            var rightImage = wndDrawData.Items[rightIndex].Image?.Value;

            if (leftImage != null &&
                middleImage != null &&
                rightImage != null)
            {
                return new Image(
                    $"{leftImage.Name}:{middleImage.Name}:{rightImage.Name}",
                    new StretchableMappedImageSource(leftImage, middleImage, rightImage, _textureCache));
            }
            else
            {
                return null;
            }
        }
    }
}
