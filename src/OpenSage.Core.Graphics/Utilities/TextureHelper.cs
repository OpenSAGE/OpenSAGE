using System;
using Veldrid;

namespace OpenSage.Core.Graphics;

public static class TextureHelper
{
    private static Texture GetDefaultTexture(
        GraphicsDeviceManager graphicsManager,
        string key,
        byte[] color)
    {
        if (!graphicsManager.Data.TryGetValue(key, out var result))
        {
            var texture = graphicsManager.GraphicsDevice.CreateStaticTexture2D(
                1, 1, 1,
                new TextureMipMapData(
                    color,
                    4, 4,
                    1, 1),
                PixelFormat.R8_G8_B8_A8_UNorm);
            graphicsManager.Data.Add(key, texture);

            texture.Name = key;

            result = texture;
        }

        return (Texture)result;
    }

    public static Texture GetDefaultTextureWhite(this GraphicsDeviceManager graphicsManager)
    {
        return GetDefaultTexture(graphicsManager, "White Texture",new byte[] { 255, 255, 255, 255 });
    }

    public static Texture GetDefaultTextureBlack(this GraphicsDeviceManager graphicsManager)
    {
        return GetDefaultTexture(graphicsManager, "White Texture", new byte[] { 0, 0, 0, 255 });
    }

    public static Texture GetDefaultTexturePlaceholder(this GraphicsDeviceManager graphicsManager)
    {
        return GetDefaultTexture(graphicsManager, "White Texture", new byte[] { 255, 105, 180, 255 });
    }

    public static Texture CreateStaticTexture2D(
        this GraphicsDevice graphicsDevice,
        uint width, uint height, uint arrayLayers,
        in TextureMipMapData mipMapData,
        PixelFormat pixelFormat)
    {
        return graphicsDevice.CreateStaticTexture2D(
            width, height, arrayLayers,
            new[]
            {
                mipMapData
            },
            pixelFormat,
            false);
    }

    public static Texture CreateStaticTexture2D(
        this GraphicsDevice graphicsDevice,
        uint width, uint height, uint arrayLayers,
        TextureMipMapData[] mipMapData,
        PixelFormat pixelFormat,
        bool isCubemap)
    {
        var mipMapLevels = (uint)mipMapData.Length / arrayLayers;

        var staging = graphicsDevice.ResourceFactory.CreateTexture(
            TextureDescription.Texture2D(
                width,
                height,
                mipMapLevels,
                1,
                pixelFormat,
                TextureUsage.Staging));

        var usage = TextureUsage.Sampled;
        if (isCubemap)
        {
            usage |= TextureUsage.Cubemap;
        }

        var result = graphicsDevice.ResourceFactory.CreateTexture(
            TextureDescription.Texture2D(
                width,
                height,
                mipMapLevels,
                arrayLayers,
                pixelFormat,
                usage));

        var commandList = graphicsDevice.ResourceFactory.CreateCommandList();
        commandList.Begin();

        for (var arrayLayer = 0u; arrayLayer < arrayLayers; arrayLayer++)
        {
            var calculatedMipWidth = width;
            var calculatedMipHeight = height;

            for (var level = 0u; level < mipMapLevels; level++)
            {
                var mipMap = mipMapData[(arrayLayer * mipMapLevels) + level];

                var mipMapWidth = Math.Min(calculatedMipWidth, mipMap.Width);
                var mipMapHeight = Math.Min(calculatedMipHeight, mipMap.Height);

                graphicsDevice.UpdateTexture(
                    staging,
                    mipMap.Data,
                    0, 0, 0,
                    mipMapWidth,
                    mipMapHeight,
                    1,
                    level,
                    0);

                commandList.CopyTexture(
                    staging, 0, 0, 0, level, 0,
                    result, 0, 0, 0, level, arrayLayer,
                    mipMapWidth,
                    mipMapHeight,
                    1, 1);

                calculatedMipWidth = Math.Max(calculatedMipWidth / 2, 1);
                calculatedMipHeight = Math.Max(calculatedMipHeight / 2, 1);
            }
        }

        commandList.End();

        graphicsDevice.SubmitCommands(commandList);

        graphicsDevice.DisposeWhenIdle(commandList);
        graphicsDevice.DisposeWhenIdle(staging);

        return result;
    }
}
