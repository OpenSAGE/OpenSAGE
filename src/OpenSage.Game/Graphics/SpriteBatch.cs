using System.Numerics;
using OpenSage.Content;
using OpenSage.Graphics.Effects;
using OpenSage.Mathematics;
using OpenSage.Utilities.Extensions;
using Veldrid;

namespace OpenSage.Graphics
{
    public sealed class SpriteBatch : DisposableBase
    {
        private readonly GraphicsDevice _graphicsDevice;
        private readonly SpriteMaterial _material;
        private readonly ConstantBuffer<SpriteMaterial.MaterialConstantsVS> _materialConstantsVSBuffer;
        private readonly DeviceBuffer _vertexBuffer;
        private readonly SpriteVertex[] _vertices;
        private readonly DeviceBuffer _indexBuffer;

        private const int InitialBatchSize = 256;
        private SpriteBatchItem[] _batchItems;
        private int _currentBatchIndex;

        private CommandList _commandEncoder;

        private OutputDescription _outputDescription;

        public SpriteBatch(ContentManager contentManager)
        {
            _graphicsDevice = contentManager.GraphicsDevice;

            _material = AddDisposable(new SpriteMaterial(contentManager, contentManager.EffectLibrary.Sprite));

            _materialConstantsVSBuffer = AddDisposable(new ConstantBuffer<SpriteMaterial.MaterialConstantsVS>(contentManager.GraphicsDevice));

            _material.SetMaterialConstantsVS(_materialConstantsVSBuffer.Buffer);

            _vertexBuffer = AddDisposable(_graphicsDevice.ResourceFactory.CreateBuffer(
                new BufferDescription(SpriteVertex.VertexDescriptor.Stride * 4, BufferUsage.VertexBuffer | BufferUsage.Dynamic)));

            _vertices = new SpriteVertex[4]; // Order is TL, TR, BL, BR

            _indexBuffer = AddDisposable(_graphicsDevice.CreateStaticBuffer(
                new ushort[] { 0, 1, 2, 2, 1, 3 },
                BufferUsage.IndexBuffer));

            _batchItems = new SpriteBatchItem[InitialBatchSize];
        }

        public void Begin(
            CommandList commandEncoder,
            Sampler samplerState,
            in OutputDescription outputDescription,
            in Viewport viewport)
        {
            _commandEncoder = commandEncoder;

            _outputDescription = outputDescription;

            _material.Effect.Begin(_commandEncoder);

            _material.SetSampler(samplerState);

            _materialConstantsVSBuffer.Value.Projection = Matrix4x4.CreateOrthographicOffCenter(
               0,
               viewport.Width,
               viewport.Height,
               0,
               0,
               -1);
            _materialConstantsVSBuffer.Update(commandEncoder);

            _material.SetMaterialConstantsVS(_materialConstantsVSBuffer.Buffer);

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

        public void DrawImage(
            Texture image,
            in Rectangle? sourceRect,
            in RectangleF destinationRect,
            in ColorRgbaF color)
        {
            ref var batchItem = ref CreateBatchItem();

            batchItem.Texture = image;

            var sourceRectangle = sourceRect ?? new Rectangle(0, 0, (int) image.Width, (int) image.Height);

            var texCoordTL = new Vector2(
                sourceRectangle.Left / (float) image.Width,
                sourceRectangle.Top / (float) image.Height);

            var texCoordBR = new Vector2(
                sourceRectangle.Right / (float) image.Width,
                sourceRectangle.Bottom / (float) image.Height);

            batchItem.Set(
                destinationRect.X,
                destinationRect.Y,
                destinationRect.Width,
                destinationRect.Height,
                color,
                texCoordTL,
                texCoordBR,
                0);
        }

        public void DrawImage(
            Texture image,
            in Rectangle? sourceRect,
            in Vector2 position,
            float rotation,
            Vector2 origin,
            in Vector2 scale,
            in ColorRgbaF color)
        {
            ref var batchItem = ref CreateBatchItem();

            batchItem.Texture = image;

            var sourceRectangle = sourceRect ?? new Rectangle(0, 0, (int) image.Width, (int) image.Height);

            var texCoordTL = new Vector2(
                sourceRectangle.Left / (float) image.Width,
                sourceRectangle.Top / (float) image.Height);

            var texCoordBR = new Vector2(
                sourceRectangle.Right / (float) image.Width,
                sourceRectangle.Bottom / (float) image.Height);

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
                MathUtility.Sin(rotation),
                MathUtility.Cos(rotation),
                color,
                texCoordTL,
                texCoordBR,
                0);
        }

        public void DrawImage(
            Texture texture,
            in Triangle2D sourceTriangle,
            in Triangle2D destinationTriangle,
            in ColorRgbaF tintColor)
        {
            ref var batchItem = ref CreateBatchItem();

            batchItem.Texture = texture;

            var textureCoordinates = new Triangle2D
            {
                V0 = new Vector2(sourceTriangle.V0.X / texture.Width, sourceTriangle.V0.Y / texture.Height),
                V1 = new Vector2(sourceTriangle.V1.X / texture.Width, sourceTriangle.V1.Y / texture.Height),
                V2 = new Vector2(sourceTriangle.V2.X / texture.Width, sourceTriangle.V2.Y / texture.Height)
            };

            batchItem.Set(
                destinationTriangle,
                textureCoordinates,
                tintColor,
                0);
        }

        public void End()
        {
            // TODO: Batch draw calls by texture.

            for (var i = 0; i < _currentBatchIndex; i++)
            {
                ref var batchItem = ref _batchItems[i];

                _vertices[0] = batchItem.VertexTL;
                _vertices[1] = batchItem.VertexTR;
                _vertices[2] = batchItem.VertexBL;
                _vertices[3] = batchItem.VertexBR;

                _commandEncoder.UpdateBuffer(_vertexBuffer, 0, _vertices);

                _commandEncoder.SetVertexBuffer(0, _vertexBuffer);

                _material.SetTexture(batchItem.Texture);

                _material.ApplyPipelineState(_outputDescription);
                _material.ApplyProperties();

                _material.Effect.ApplyPipelineState(_commandEncoder);
                _material.Effect.ApplyParameters(_commandEncoder);

                var indexCount = batchItem.ItemType == SpriteBatchItemType.Quad ? 6u : 3u;

                _commandEncoder.SetIndexBuffer(_indexBuffer, IndexFormat.UInt16);

                _commandEncoder.DrawIndexed(indexCount);
            }
        }

        private struct SpriteBatchItem
        {
            public Texture Texture;

            public SpriteBatchItemType ItemType;

            public SpriteVertex VertexTL;
            public SpriteVertex VertexTR;
            public SpriteVertex VertexBL;

            // Not used for a triangle item.
            public SpriteVertex VertexBR;

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

            public void Set(float x, float y, float w, float h, in ColorRgbaF color, in Vector2 texCoordTL, in Vector2 texCoordBR, float depth)
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
}
