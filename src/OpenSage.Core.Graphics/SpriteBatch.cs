using System;
using System.Collections.Generic;
using System.Numerics;
using OpenSage.Core.Graphics;
using OpenSage.Graphics.Shaders;
using OpenSage.Mathematics;
using OpenSage.Rendering;
using OpenSage.Utilities.Extensions;
using Veldrid;
using Rectangle = OpenSage.Mathematics.Rectangle;

namespace OpenSage.Graphics
{
    public sealed class SpriteBatch : DisposableBase
    {
        private readonly SpriteShaderResources _spriteShaderResources;
        private readonly Texture _solidWhiteTexture;
        private readonly GraphicsDevice _graphicsDevice;
        private readonly Pipeline _pipeline;
        private readonly ConstantBuffer<SpriteShaderResources.MaterialConstantsVS> _materialConstantsVSBuffer;
        private readonly ConstantBuffer<SpriteShaderResources.SpriteConstantsPS> _spriteConstantsPSBuffer;
        private readonly ResourceSet _spriteConstantsResourceSet;
        private readonly Dictionary<Texture, ResourceSet> _textureResourceSets;
        private readonly DeviceBuffer _vertexBuffer;
        private readonly SpriteShaderResources.SpriteVertex[] _vertices;
        private readonly DeviceBuffer _indexBuffer;

        private const int InitialBatchSize = 256;
        private SpriteBatchItem[] _batchItems;
        private int _currentBatchIndex;

        private CommandList _commandList;

        public SpriteBatch(
            GraphicsDeviceManager graphicsDeviceManager,
            ShaderSetStore shaderSetStore,
            in BlendStateDescription blendStateDescription,
            in OutputDescription outputDescription)
        {
            _spriteShaderResources = shaderSetStore.GetShaderSet(() => new SpriteShaderResources(shaderSetStore));
            _solidWhiteTexture = graphicsDeviceManager.GetDefaultTextureWhite();
            _graphicsDevice = graphicsDeviceManager.GraphicsDevice;

            _pipeline = _spriteShaderResources.GetCachedPipeline(
                blendStateDescription,
                outputDescription);

            _materialConstantsVSBuffer = AddDisposable(new ConstantBuffer<SpriteShaderResources.MaterialConstantsVS>(graphicsDeviceManager.GraphicsDevice));

            _spriteConstantsPSBuffer = AddDisposable(new ConstantBuffer<SpriteShaderResources.SpriteConstantsPS>(graphicsDeviceManager.GraphicsDevice));

            _spriteConstantsPSBuffer.Value.IgnoreAlpha = false;
            _spriteConstantsPSBuffer.Update(_graphicsDevice);

            _spriteConstantsResourceSet = AddDisposable(_spriteShaderResources.CreateSpriteConstantsResourceSet(
                _materialConstantsVSBuffer.Buffer,
                _spriteConstantsPSBuffer.Buffer));

            _vertexBuffer = AddDisposable(_graphicsDevice.ResourceFactory.CreateBuffer(
                new BufferDescription(SpriteShaderResources.SpriteVertex.VertexDescriptor.Stride * 4, BufferUsage.VertexBuffer | BufferUsage.Dynamic)));

            _vertices = new SpriteShaderResources.SpriteVertex[4]; // Order is TL, TR, BL, BR

            _indexBuffer = AddDisposable(_graphicsDevice.CreateStaticBuffer(
                new ushort[] { 0, 1, 2, 2, 1, 3 },
                BufferUsage.IndexBuffer));

            _batchItems = new SpriteBatchItem[InitialBatchSize];

            _textureResourceSets = new Dictionary<Texture, ResourceSet>();
        }

        public void Begin(
            CommandList commandList,
            Sampler sampler,
            in SizeF outputSize,
            bool ignoreAlpha = false)
        {
            _commandList = commandList;

            _commandList.SetPipeline(_pipeline);

            var projection = Matrix4x4.CreateOrthographicOffCenter(
               0,
               outputSize.Width,
               outputSize.Height,
               0,
               0,
               -1);
            if (projection != _materialConstantsVSBuffer.Value.Projection)
            {
                _materialConstantsVSBuffer.Value.Projection = projection;
                _materialConstantsVSBuffer.Update(commandList);
            }

            if (ignoreAlpha != _spriteConstantsPSBuffer.Value.IgnoreAlpha)
            {
                _spriteConstantsPSBuffer.Value.IgnoreAlpha = ignoreAlpha;
                _spriteConstantsPSBuffer.Update(commandList);
            }

            _commandList.SetGraphicsResourceSet(0, _spriteConstantsResourceSet);

            var samplerResourceSet = _spriteShaderResources.GetCachedSamplerResourceSet(sampler);
            _commandList.SetGraphicsResourceSet(1, samplerResourceSet);

            _currentBatchIndex = 0;
        }

        private ref SpriteBatchItem CreateBatchItem()
        {
            if (_currentBatchIndex >= _batchItems.Length)
            {
                System.Array.Resize(ref _batchItems, _batchItems.Length * 2);
            }

            return ref _batchItems[_currentBatchIndex++];
        }

        private static Vector2 GetTopLeftUV(in Rectangle src, Texture img, bool flipped)
        {
            return new Vector2(src.Left / (float) img.Width, flipped ?
                (src.Bottom / (float) img.Height) :
                (src.Top / (float) img.Height));
        }

        private static Vector2 GetBottomRightUV(in Rectangle src, Texture img, bool flipped)
        {
            return new Vector2(src.Right / (float) img.Width, flipped ?
                (src.Top / (float) img.Height) :
                (src.Bottom / (float) img.Height));
        }

        private static Triangle2D GetTriangleUV(in Triangle2D src, Texture img, bool flipped)
        {
            return new Triangle2D(
                new Vector2(src.V0.X / img.Width, flipped ? 1 - (src.V0.Y / img.Height) : src.V0.Y / img.Height),
                new Vector2(src.V1.X / img.Width, flipped ? 1 - (src.V1.Y / img.Height) : src.V1.Y / img.Height),
                new Vector2(src.V2.X / img.Width, flipped ? 1 - (src.V2.Y / img.Height) : src.V2.Y / img.Height));
        }

        public void DrawImage(
            Texture image,
            in Rectangle? sourceRect,
            in RectangleF destinationRect,
            in ColorRgbaF color,
            in bool flipped = false,
            SpriteFillMethod fillMethod = SpriteFillMethod.Normal,
            float fillAmount = 0.0f,
            bool grayscale = false,
            Texture alphaMask = null)
        {
            ref var batchItem = ref CreateBatchItem();

            batchItem.Texture = image;
            batchItem.AlphaMask = alphaMask;

            var sourceRectangle = sourceRect ?? new Rectangle(0, 0, (int) image.Width, (int) image.Height);

            var texCoordTL = GetTopLeftUV(sourceRectangle, image, flipped);
            var texCoordBR = GetBottomRightUV(sourceRectangle, image, flipped);

            batchItem.Set(
                destinationRect.X,
                destinationRect.Y,
                destinationRect.Width,
                destinationRect.Height,
                color,
                texCoordTL,
                texCoordBR,
                0,
                fillMethod,
                fillAmount,
                grayscale);
        }

        public void DrawImage(
            Texture image,
            in Rectangle? sourceRect,
            in Vector2 position,
            float rotation,
            Vector2 origin,
            in Vector2 scale,
            in ColorRgbaF color,
            in bool flipped = false,
            Texture alphaMask = null)
        {
            ref var batchItem = ref CreateBatchItem();

            batchItem.Texture = image;
            batchItem.AlphaMask = alphaMask;

            var sourceRectangle = sourceRect ?? new Rectangle(0, 0, (int) image.Width, (int) image.Height);

            var texCoordTL = GetTopLeftUV(sourceRectangle, image, flipped);
            var texCoordBR = GetBottomRightUV(sourceRectangle, image, flipped);

            var width = sourceRectangle.Width * scale.X;
            var height = sourceRectangle.Height * scale.Y;

            origin *= scale;

            batchItem.Set(
                position.X,
                position.Y,
                -origin.X,
                -origin.Y,
                width,
                height,
                MathF.Sin(rotation),
                MathF.Cos(rotation),
                color,
                texCoordTL,
                texCoordBR,
                0);
        }

        public void DrawImage(
            Texture texture,
            in Triangle2D sourceTriangle,
            in Triangle2D destinationTriangle,
            in ColorRgbaF tintColor,
            in bool flipped = false,
            Texture alphaMask = null)
        {
            ref var batchItem = ref CreateBatchItem();

            batchItem.Texture = texture;
            batchItem.AlphaMask = alphaMask;

            var textureCoordinates = GetTriangleUV(sourceTriangle, texture, flipped);

            batchItem.Set(
                destinationTriangle,
                textureCoordinates,
                tintColor,
                0);
        }

        public void End()
        {
            // TODO: Batch draw calls by texture.
            // TODO: Sort by FillMethod

            for (var i = 0; i < _currentBatchIndex; i++)
            {
                ref var batchItem = ref _batchItems[i];

                if (batchItem.OutputOffset != _spriteConstantsPSBuffer.Value.OutputOffset
                    || batchItem.OutputSize != _spriteConstantsPSBuffer.Value.OutputSize
                    || batchItem.FillMethod != _spriteConstantsPSBuffer.Value.FillMethod
                    || batchItem.FillAmount != _spriteConstantsPSBuffer.Value.FillAmount
                    || batchItem.Grayscale != _spriteConstantsPSBuffer.Value.Grayscale)
                {
                    _spriteConstantsPSBuffer.Value.OutputOffset = batchItem.OutputOffset;
                    _spriteConstantsPSBuffer.Value.OutputSize = batchItem.OutputSize;
                    _spriteConstantsPSBuffer.Value.FillMethod = batchItem.FillMethod;
                    _spriteConstantsPSBuffer.Value.FillAmount = batchItem.FillAmount;
                    _spriteConstantsPSBuffer.Value.Grayscale = batchItem.Grayscale;
                    _spriteConstantsPSBuffer.Update(_commandList);
                }

                _vertices[0] = batchItem.VertexTL;
                _vertices[1] = batchItem.VertexTR;
                _vertices[2] = batchItem.VertexBL;
                _vertices[3] = batchItem.VertexBR;

                _commandList.UpdateBuffer(_vertexBuffer, 0, _vertices);

                _commandList.SetVertexBuffer(0, _vertexBuffer);

                var textureResourceSet = GetTextureResourceSet(batchItem.Texture);
                _commandList.SetGraphicsResourceSet(2, textureResourceSet);

                var alphaMaskResourceSet = GetAlphaMaskResourceSet(batchItem.AlphaMask ?? _solidWhiteTexture);
                _commandList.SetGraphicsResourceSet(3, alphaMaskResourceSet);

                _commandList.SetIndexBuffer(_indexBuffer, IndexFormat.UInt16);

                var indexCount = batchItem.ItemType == SpriteBatchItemType.Quad ? 6u : 3u;
                _commandList.DrawIndexed(indexCount);
            }
        }

        private ResourceSet GetTextureResourceSet(Texture texture)
        {
            // TODO: Clear not-recently-used textures from the cache.
            if (!_textureResourceSets.TryGetValue(texture, out var result))
            {
                result = AddDisposable(_spriteShaderResources.CreateTextureResourceSet(texture));
                _textureResourceSets.Add(texture, result);
            }
            return result;
        }

        private ResourceSet GetAlphaMaskResourceSet(Texture alphaMask)
        {
            // TODO: Clear not-recently-used textures from the cache.
            if (!_textureResourceSets.TryGetValue(alphaMask, out var result))
            {
                result = AddDisposable(_spriteShaderResources.CreateAlphaMaskResourceSet(alphaMask));
                _textureResourceSets.Add(alphaMask, result);
            }
            return result;
        }

        private struct SpriteBatchItem
        {
            public Texture Texture;
            public Texture AlphaMask;

            public SpriteBatchItemType ItemType;

            public SpriteShaderResources.SpriteVertex VertexTL;
            public SpriteShaderResources.SpriteVertex VertexTR;
            public SpriteShaderResources.SpriteVertex VertexBL;

            // Not used for a triangle item.
            public SpriteShaderResources.SpriteVertex VertexBR;

            public Vector2 OutputOffset;
            public Vector2 OutputSize;
            public SpriteFillMethod FillMethod;
            public float FillAmount;

            public bool Grayscale;

            public void Set(float x, float y, float dx, float dy, float w, float h, float sin, float cos, in ColorRgbaF color, in Vector2 texCoordTL, in Vector2 texCoordBR, float depth)
            {
                ItemType = SpriteBatchItemType.Quad;

                VertexTL.Position.X = x + dx * cos - dy * sin;
                VertexTL.Position.Y = y + dx * sin + dy * cos;
                VertexTL.Position.Z = depth;
                VertexTL.Color = color;
                VertexTL.UV.X = texCoordTL.X;
                VertexTL.UV.Y = texCoordTL.Y;

                VertexTR.Position.X = x + (dx + w) * cos - dy * sin;
                VertexTR.Position.Y = y + (dx + w) * sin + dy * cos;
                VertexTR.Position.Z = depth;
                VertexTR.Color = color;
                VertexTR.UV.X = texCoordBR.X;
                VertexTR.UV.Y = texCoordTL.Y;

                VertexBL.Position.X = x + dx * cos - (dy + h) * sin;
                VertexBL.Position.Y = y + dx * sin + (dy + h) * cos;
                VertexBL.Position.Z = depth;
                VertexBL.Color = color;
                VertexBL.UV.X = texCoordTL.X;
                VertexBL.UV.Y = texCoordBR.Y;

                VertexBR.Position.X = x + (dx + w) * cos - (dy + h) * sin;
                VertexBR.Position.Y = y + (dx + w) * sin + (dy + h) * cos;
                VertexBR.Position.Z = depth;
                VertexBR.Color = color;
                VertexBR.UV.X = texCoordBR.X;
                VertexBR.UV.Y = texCoordBR.Y;
            }

            public void Set(
                float x, float y, float w, float h,
                in ColorRgbaF color,
                in Vector2 texCoordTL,
                in Vector2 texCoordBR,
                float depth,
                SpriteFillMethod fillMethod,
                float fillAmount,
                bool grayscale)
            {
                ItemType = SpriteBatchItemType.Quad;

                VertexTL.Position.X = x;
                VertexTL.Position.Y = y;
                VertexTL.Position.Z = depth;
                VertexTL.Color = color;
                VertexTL.UV.X = texCoordTL.X;
                VertexTL.UV.Y = texCoordTL.Y;

                VertexTR.Position.X = x + w;
                VertexTR.Position.Y = y;
                VertexTR.Position.Z = depth;
                VertexTR.Color = color;
                VertexTR.UV.X = texCoordBR.X;
                VertexTR.UV.Y = texCoordTL.Y;

                VertexBL.Position.X = x;
                VertexBL.Position.Y = y + h;
                VertexBL.Position.Z = depth;
                VertexBL.Color = color;
                VertexBL.UV.X = texCoordTL.X;
                VertexBL.UV.Y = texCoordBR.Y;

                VertexBR.Position.X = x + w;
                VertexBR.Position.Y = y + h;
                VertexBR.Position.Z = depth;
                VertexBR.Color = color;
                VertexBR.UV.X = texCoordBR.X;
                VertexBR.UV.Y = texCoordBR.Y;

                OutputOffset = new Vector2(x, y);
                OutputSize = new Vector2(w, h);
                FillMethod = fillMethod;
                FillAmount = fillAmount;

                Grayscale = grayscale;
            }

            public void Set(in Triangle2D triangle, in Triangle2D texCoords, in ColorRgbaF color, float depth)
            {
                ItemType = SpriteBatchItemType.Triangle;

                VertexTL.Position.X = triangle.V0.X;
                VertexTL.Position.Y = triangle.V0.Y;
                VertexTL.Position.Z = depth;
                VertexTL.Color = color;
                VertexTL.UV.X = texCoords.V0.X;
                VertexTL.UV.Y = texCoords.V0.Y;

                VertexTR.Position.X = triangle.V1.X;
                VertexTR.Position.Y = triangle.V1.Y;
                VertexTR.Position.Z = depth;
                VertexTR.Color = color;
                VertexTR.UV.X = texCoords.V1.X;
                VertexTR.UV.Y = texCoords.V1.Y;

                VertexBL.Position.X = triangle.V2.X;
                VertexBL.Position.Y = triangle.V2.Y;
                VertexBL.Position.Z = depth;
                VertexBL.Color = color;
                VertexBL.UV.X = texCoords.V2.X;
                VertexBL.UV.Y = texCoords.V2.Y;
            }
        }

        private enum SpriteBatchItemType
        {
            Quad,
            Triangle
        }
    }

    public enum SpriteFillMethod
    {
        Normal,
        Radial360
    }
}
