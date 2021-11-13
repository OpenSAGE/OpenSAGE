using System;
using Veldrid;
using Veldrid.StartupUtilities;

namespace OpenSage.Graphics
{
    internal static class GraphicsDeviceUtility
    {
        public static GraphicsDevice CreateGraphicsDevice(GraphicsBackend? preferredBackend, GameWindow window)
        {
#if DEBUG
            const bool debug = true;
#else
            const bool debug = false;
#endif

            var options = new GraphicsDeviceOptions
            {
                Debug = debug,
                SyncToVerticalBlank = true,
                ResourceBindingModel = ResourceBindingModel.Improved,
                PreferStandardClipSpaceYDirection = true,
                PreferDepthRangeZeroToOne = true
            };

            var backend = preferredBackend ?? VeldridStartup.GetPlatformDefaultBackend();

            if (window != null)
            {
                return window.CreateGraphicsDevice(options, backend);
            }

            return backend switch
            {
                GraphicsBackend.Direct3D11 => GraphicsDevice.CreateD3D11(options),
                GraphicsBackend.Vulkan => GraphicsDevice.CreateVulkan(options),
                GraphicsBackend.Metal => GraphicsDevice.CreateMetal(options),
                _ => throw new InvalidOperationException($"Cannot create a graphics device for {backend} backend without a window"),
            };
        }
    }
}
