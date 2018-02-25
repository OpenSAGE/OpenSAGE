using OpenSage.Content;
using OpenSage.Data.Wnd;

namespace OpenSage.Gui.Wnd.Images
{
    internal sealed class WndImageLoader : DisposableBase
    {
        private readonly ContentManager _contentManager;
        private readonly MappedImageLoader _mappedImageLoader;

        public WndImageLoader(ContentManager contentManager, MappedImageLoader mappedImageLoader)
        {
            _contentManager = contentManager;
            _mappedImageLoader = mappedImageLoader;
        }

        public Image GetNormalImage(
            WndDrawData wndDrawData,
            int index)
        {
            var result = GetMappedImage(wndDrawData.Items[index].Image);
            return result != null
                ? new Image(result)
                : null;
        }

        public StretchableImage GetStretchableImage(
            WndDrawData wndDrawData,
            int leftIndex,
            int middleIndex,
            int rightIndex)
        {
            var leftImage = wndDrawData.Items[leftIndex].Image;
            var middleImage = wndDrawData.Items[middleIndex].Image;
            var rightImage = wndDrawData.Items[rightIndex].Image;

            var leftMappedImageTexture = GetMappedImage(leftImage);
            var middleMappedImageTexture = GetMappedImage(middleImage);
            var rightMappedImageTexture = GetMappedImage(rightImage);

            if (leftMappedImageTexture != null &&
                middleMappedImageTexture != null &&
                rightMappedImageTexture != null)
            {
                return new StretchableImage(
                    leftMappedImageTexture,
                    middleMappedImageTexture,
                    rightMappedImageTexture);
            }
            else
            {
                return null;
            }
        }

        private MappedImageTexture GetMappedImage(string mappedImageName)
        {
            if (string.IsNullOrEmpty(mappedImageName) || mappedImageName == "NoImage")
            {
                return null;
            }

            return _mappedImageLoader.GetMappedImage(mappedImageName);
        }
    }
}
