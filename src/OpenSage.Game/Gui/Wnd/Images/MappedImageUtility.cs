﻿using System;
using OpenSage.Content;
using OpenSage.Graphics;
using OpenSage.Mathematics;
using Veldrid;

namespace OpenSage.Gui.Wnd.Images
{
    internal static class MappedImageUtility
    {
        /// <summary>
        /// Creates a texture from a mapped image.
        /// This version does a simple copy of the original image part
        /// </summary>
        public static Texture CreateTexture(
            GraphicsLoadContext loadContext,
            MappedImage mappedImage)
        {
            var graphicsDevice = loadContext.GraphicsDevice;

            var src = mappedImage.Coords;
            var width = (uint) src.Width;
            var height = (uint) src.Height;

            var imageTexture = graphicsDevice.ResourceFactory.CreateTexture(
                TextureDescription.Texture2D(
                    width,
                    height,
                    1,
                    1,
                    PixelFormat.R8_G8_B8_A8_UNorm,
                    TextureUsage.Sampled));

            imageTexture.Name = "WndImage";

            var commandList = graphicsDevice.ResourceFactory.CreateCommandList();

            commandList.Begin();

            commandList.CopyTexture(
                mappedImage.Texture.Value, (uint) src.Left, (uint) src.Top, 0, 0, 0,    // Source
                imageTexture, 0, 0, 0, 0, 0, width, height, 1, 1);                      // Destination

            commandList.End();

            graphicsDevice.SubmitCommands(commandList);
            graphicsDevice.DisposeWhenIdle(commandList);

            graphicsDevice.WaitForIdle();

            return imageTexture;
        }

        /// <summary>
        /// Creates a texture from a mapped image. We need to do this to avoid alpha bleeding artifacts
        /// when blitting a portion of a texture.
        /// </summary>
        public static Texture CreateTexture(
            GraphicsLoadContext loadContext,
            Size size,
            Action<SpriteBatch> drawCallback)
        {
            var graphicsDevice = loadContext.GraphicsDevice;

            var imageTexture = graphicsDevice.ResourceFactory.CreateTexture(
                TextureDescription.Texture2D(
                    (uint) size.Width,
                    (uint) size.Height,
                    1,
                    1,
                    PixelFormat.R8_G8_B8_A8_UNorm,
                    TextureUsage.Sampled | TextureUsage.RenderTarget));

            imageTexture.Name = "WndImage";

            var framebuffer = graphicsDevice.ResourceFactory.CreateFramebuffer(new FramebufferDescription(null, imageTexture));
            var spriteBatch = new SpriteBatch(
                loadContext,
                BlendStateDescription.SingleDisabled,
                framebuffer.OutputDescription);
            var commandList = graphicsDevice.ResourceFactory.CreateCommandList();

            commandList.Begin();

            commandList.SetFramebuffer(framebuffer);

            spriteBatch.Begin(
                commandList,
                loadContext.StandardGraphicsResources.PointClampSampler,
                new SizeF(imageTexture.Width, imageTexture.Height));

            spriteBatch.DrawImage(
                loadContext.StandardGraphicsResources.SolidWhiteTexture,
                null,
                new RectangleF(0, 0, imageTexture.Width, imageTexture.Height),
                ColorRgbaF.Transparent);

            drawCallback(spriteBatch);

            spriteBatch.End();

            commandList.End();

            graphicsDevice.SubmitCommands(commandList);

            graphicsDevice.DisposeWhenIdle(commandList);
            graphicsDevice.DisposeWhenIdle(spriteBatch);
            graphicsDevice.DisposeWhenIdle(framebuffer);

            graphicsDevice.WaitForIdle();

            return imageTexture;
        }
    }
}
