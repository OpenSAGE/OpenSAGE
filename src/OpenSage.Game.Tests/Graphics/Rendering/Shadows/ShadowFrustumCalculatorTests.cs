using System.Numerics;
using OpenSage.Graphics.Cameras;
using OpenSage.Graphics.Rendering.Shadows;
using OpenSage.Graphics.Shaders;
using Veldrid.StartupUtilities;
using Xunit;

namespace OpenSage.Tests.Graphics.Rendering.Shadows
{
    public class DirectionalLightFrustumCalculatorTests
    {
        [Fact]
        public void CanCalculateFrustum()
        {
            // Arrange.
            var light = new GlobalLightingTypes.Light { Direction = Vector3.Normalize(new Vector3(-1, -1, -1)) };

            var camera = new Camera(() => new Veldrid.Viewport { Width = 800, Height = 600 });
            camera.SetLookAt(new Vector3(5, 3, 10), Vector3.Zero, Vector3.UnitZ);

            var window = new Veldrid.Sdl2.Sdl2Window("Test", 0, 0, 100, 100, 0, false);

            using (var graphicsDevice = VeldridStartup.CreateGraphicsDevice(window))
            {
                var shadowData = new ShadowData(4, 5, 1000, 512, graphicsDevice);
                var settings = new ShadowSettings();

                var lightFrustumCalculator = new ShadowFrustumCalculator();

                // Act.
                lightFrustumCalculator.CalculateShadowData(light, camera, shadowData, settings);

                // Assert.
                Assert.Equal(-0.02f, shadowData.ShadowCameraViewProjections[0].M11, 2);
                Assert.Equal(0.37f, shadowData.ShadowCameraViewProjections[0].M43, 2);
            }
        }
    }
}
