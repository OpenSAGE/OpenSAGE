using System.IO;
using System.Linq;
using OpenSage.LowLevel.Graphics3D;
using OpenSage.Content;
using OpenSage.Content.Util;
using OpenSage.Data.Wnd;
using OpenSage.Graphics;
using OpenSage.Mathematics;
using OpenSage.LowLevel.Graphics2D;

namespace OpenSage.Gui.Elements
{
    internal sealed class UIElementState : DisposableBase
    {
        public ColorRgba TextColor { get; }
        public ColorRgba TextBorderColor { get; }

        public ColorRgbaF? BackgroundColor { get; }
        public ColorRgbaF? BorderColor { get; }

        public Texture ImageTexture { get; }

        public UIElementState(
            WndWindow wndWindow,
            ColorRgba textColor,
            ColorRgba textBorderColor,
            WndDrawData wndDrawData,
            ContentManager contentManager,
            GraphicsDevice graphicsDevice,
            GraphicsDevice2D graphicsDevice2D)
        {
            TextColor = textColor;
            TextBorderColor = textBorderColor;

            if (!wndWindow.Status.HasFlag(WndWindowStatusFlags.Image))
            {
                BackgroundColor = wndDrawData.Items[0].Color.ToColorRgbaF();
            }

            if (BackgroundColor != null || wndWindow.Status.HasFlag(WndWindowStatusFlags.Border))
            {
                BorderColor = wndDrawData.Items[0].BorderColor.ToColorRgbaF();
            }

            if (wndWindow.Status.HasFlag(WndWindowStatusFlags.Image))
            {
                var image = LoadImage(wndWindow, wndDrawData, contentManager);
                if (image != null)
                {
                    ImageTexture = AddDisposable(image.RenderToTexture(graphicsDevice, graphicsDevice2D));
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
