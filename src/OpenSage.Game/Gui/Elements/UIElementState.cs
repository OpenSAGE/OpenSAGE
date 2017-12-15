using System.IO;
using System.Linq;
using OpenSage.LowLevel.Graphics3D;
using OpenSage.Content;
using OpenSage.Content.Util;
using OpenSage.Data.Wnd;
using OpenSage.Graphics;

namespace OpenSage.Gui.Elements
{
    internal sealed class UIElementState : DisposableBase
    {
        private ColorRgbaF? _borderColor;

        private StretchableImage _image;

        public ColorRgbaF? BackgroundColor { get; }
        public Texture ImageTexture { get; }

        public UIElementState(
            WndWindow wndWindow,
            WndDrawData wndDrawData,
            ContentManager contentManager,
            SpriteBatch spriteBatch)
        {
            if (!wndWindow.Status.HasFlag(WndWindowStatusFlags.Image))
            {
                BackgroundColor = wndDrawData.Items[0].Color.ToColorRgbaF();
            }

            if (wndWindow.Status.HasFlag(WndWindowStatusFlags.Border))
            {
                _borderColor = wndDrawData.Items[0].BorderColor.ToColorRgbaF();
            }

            if (wndWindow.Status.HasFlag(WndWindowStatusFlags.Image))
            {
                _image = LoadImage(wndWindow, wndDrawData, contentManager);

                if (_image != null)
                {
                    ImageTexture = AddDisposable(_image.RenderToTexture(
                        contentManager.GraphicsDevice,
                        spriteBatch));
                }
            }
        }

        private StretchableImage LoadImage(WndWindow wndWindow, WndDrawData wndDrawData, ContentManager contentManager)
        {
            switch (wndWindow.WindowType)
            {
                case WndWindowType.GenericWindow:
                    {
                        var image = LoadImage(wndDrawData, 0, contentManager);
                        return image != null
                            ? StretchableImage.CreateNormal(wndWindow.ScreenRect.ToRectangle().Width, image)
                            : null;
                    }

                case WndWindowType.PushButton:
                    {
                        var imageLeft = LoadImage(wndDrawData, 0, contentManager);
                        var imageMiddle = LoadImage(wndDrawData, 5, contentManager);
                        var imageRight = LoadImage(wndDrawData, 6, contentManager);

                        if (imageLeft != null && imageMiddle != null && imageRight != null)
                            return StretchableImage.CreateStretchable(wndWindow.ScreenRect.ToRectangle().Width, imageLeft, imageMiddle, imageRight);

                        if (imageLeft != null)
                            return StretchableImage.CreateNormal(wndWindow.ScreenRect.ToRectangle().Width, imageLeft);

                        return null;
                    }

                default:
                    // TODO
                    return null;
            }
        }

        private CroppedBitmap LoadImage(WndDrawData wndDrawData, int drawDataIndex, ContentManager contentManager)
        {
            var image = wndDrawData.Items[drawDataIndex].Image;

            if (string.IsNullOrEmpty(image) || image == "NoImage")
            {
                return null;
            }

            var mappedImage = contentManager.IniDataContext.MappedImages.FirstOrDefault(x => x.Name == image);
            if (mappedImage == null)
            {
                return null;
            }

            var texture = contentManager.Load<Texture>(
                new[]
                {
                    Path.Combine(@"Data\English\Art\Textures", mappedImage.Texture),
                    Path.Combine(@"Art\Textures", mappedImage.Texture)
                },
                new TextureLoadOptions { GenerateMipMaps = false });

            var textureRect = mappedImage.Coords.ToRectangle();

            return new CroppedBitmap(texture, textureRect);
        }
    }
}
