using System;
using System.Numerics;
using Veldrid;

namespace OpenSage.Graphics.Rendering.Shadows
{
    internal sealed class ShadowData : DisposableBase
    {
        public const PixelFormat ShadowMapPixelFormat = PixelFormat.D32_Float_S8_UInt;

        public static readonly OutputDescription DepthPassDescription = new OutputDescription(
            new OutputAttachmentDescription(ShadowMapPixelFormat));

        public float NearPlaneDistance { get; }
        public float FarPlaneDistance { get; }

        public uint ShadowMapSize { get; }

        public Texture ShadowMap { get; }
        public Framebuffer[] ShadowMapFramebuffers;

        public Matrix4x4 ShadowMatrix { get; set; }

        public uint NumSplits { get; }

        public float[] CascadeSplits { get; }
        public Vector4[] CascadeOffsets { get; }
        public Vector4[] CascadeScales { get; }
        public Matrix4x4[] ShadowCameraViewProjections { get; }

        public ShadowData(
            uint numCascades,
            float nearPlaneDistance,
            float farPlaneDistance,
            uint shadowMapSize,
            GraphicsDevice graphicsDevice)
        {
            NumSplits = numCascades;
            CascadeSplits = new float[numCascades];
            CascadeOffsets = new Vector4[numCascades];
            CascadeScales = new Vector4[numCascades];
            ShadowCameraViewProjections = new Matrix4x4[numCascades];

            NearPlaneDistance = nearPlaneDistance;
            FarPlaneDistance = farPlaneDistance;

            ShadowMapSize = shadowMapSize;

            ShadowMap = AddDisposable(graphicsDevice.ResourceFactory.CreateTexture(
                TextureDescription.Texture2D(
                    shadowMapSize,
                    shadowMapSize,
                    1,
                    numCascades,
                    ShadowMapPixelFormat,
                    TextureUsage.DepthStencil | TextureUsage.Sampled)));

            ShadowMapFramebuffers = new Framebuffer[numCascades];
            for (var i = 0u; i < numCascades; i++)
            {
                ShadowMapFramebuffers[i] = AddDisposable(graphicsDevice.ResourceFactory.CreateFramebuffer(
                    new FramebufferDescription(
                        new FramebufferAttachmentDescription(ShadowMap, i),
                        Array.Empty<FramebufferAttachmentDescription>())));
            }
        }
    }
}
