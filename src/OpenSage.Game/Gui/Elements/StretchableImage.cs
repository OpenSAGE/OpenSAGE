using System;
using OpenSage.LowLevel.Graphics3D;
using OpenSage.Graphics;
using OpenSage.Mathematics;

namespace OpenSage.Gui.Elements
{
    public sealed class StretchableImage
    {
        private readonly StretchableImageMode _mode;

        private readonly CroppedBitmap _croppedBitmapLeft;
        private readonly CroppedBitmap _croppedBitmapMiddle;
        private readonly CroppedBitmap _croppedBitmapRight;

        private readonly int _totalWidth;
        private readonly int _height;

        private StretchableImage(
            int destinationWidth,
            StretchableImageMode mode,
            CroppedBitmap croppedBitmapLeft,
            CroppedBitmap croppedBitmapMiddle,
            CroppedBitmap croppedBitmapRight)
        {
            _totalWidth = destinationWidth;

            _mode = mode;

            _croppedBitmapLeft = croppedBitmapLeft;
            _croppedBitmapMiddle = croppedBitmapMiddle;
            _croppedBitmapRight = croppedBitmapRight;

            _height = croppedBitmapLeft.SourceRect.Height;
        }

        public static StretchableImage CreateNormal(int destinationWidth, CroppedBitmap croppedBitmap)
        {
            return new StretchableImage(destinationWidth, StretchableImageMode.Normal, croppedBitmap, null, null);
        }

        public static StretchableImage CreateStretchable(
            int destinationWidth,
            CroppedBitmap croppedBitmapLeft,
            CroppedBitmap croppedBitmapMiddle,
            CroppedBitmap croppedBitmapRight)
        {
            return new StretchableImage(
                destinationWidth,
                StretchableImageMode.Stretchable,
                croppedBitmapLeft,
                croppedBitmapMiddle,
                croppedBitmapRight);
        }

        internal Texture RenderToTexture(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch)
        {
            var drawingContext = new DrawingContext(spriteBatch);

            var imageTexture = Texture.CreateTexture2D(
                graphicsDevice,
                PixelFormat.Rgba8UNorm,
                _totalWidth,
                _height,
                TextureBindFlags.ShaderResource | TextureBindFlags.RenderTarget);

            using (var renderTarget = new RenderTarget(
                graphicsDevice,
                imageTexture))
            {
                var commandBuffer = graphicsDevice.CommandQueue.GetCommandBuffer();

                var renderPassDescriptor = new RenderPassDescriptor();

                var clearColour = new ColorRgbaF(0, 0, 0, 0);

                renderPassDescriptor.SetRenderTargetDescriptor(
                    renderTarget,
                    LoadAction.Clear,
                    clearColour);

                var commandEncoder = commandBuffer.GetCommandEncoder(renderPassDescriptor);

                var viewport = new Rectangle(0, 0, imageTexture.Width, imageTexture.Height);
                drawingContext.Begin(commandEncoder, viewport, SamplerStateDescription.PointClamp);

                commandEncoder.SetViewport(new Viewport(0, 0, imageTexture.Width, imageTexture.Height));

                switch (_mode)
                {
                    case StretchableImageMode.Normal:
                        drawingContext.DrawImage(
                           _croppedBitmapLeft.Bitmap,
                           new Rectangle(0, 0, _totalWidth, _height),
                           _croppedBitmapLeft.SourceRect);
                        break;

                    case StretchableImageMode.Stretchable:
                        var leftWidth = _croppedBitmapLeft.SourceRect.Width;
                        var rightWidth = _croppedBitmapRight.SourceRect.Width;
                        var leftRect = new Rectangle(0, 0, leftWidth, _height);
                        drawingContext.DrawImage(
                           _croppedBitmapLeft.Bitmap,
                           leftRect,
                           _croppedBitmapLeft.SourceRect);
                        var middleRect = new Rectangle(leftRect.Right, 0, _totalWidth - leftWidth - rightWidth, _height);
                        drawingContext.DrawImage(
                           _croppedBitmapMiddle.Bitmap,
                           middleRect,
                           _croppedBitmapMiddle.SourceRect);
                        var rightRect = new Rectangle(middleRect.Right, 0, rightWidth, _height);
                        drawingContext.DrawImage(
                           _croppedBitmapRight.Bitmap,
                           rightRect,
                           _croppedBitmapRight.SourceRect);
                        break;

                    default:
                        throw new InvalidOperationException();
                }

                drawingContext.End();

                commandEncoder.Close();

                commandBuffer.Commit();
            }

            return imageTexture;
        }
    }

    public enum StretchableImageMode
    {
        Normal,
        Stretchable
    }
}
