using System.Numerics;
using System.Runtime.InteropServices;
using LL.Graphics3D;
using OpenSage.Content;
using OpenSage.Graphics.Effects;
using OpenSage.Mathematics;

namespace OpenSage.Graphics
{
    internal sealed class SpriteBatch : DisposableBase
    {
        private readonly Buffer<SpriteVertex> _vertexBuffer;
        private readonly ConstantBuffer<SpriteMaterial.MaterialConstants> _materialConstantsBuffer;
        private readonly SpriteMaterial _material;

        private CommandEncoder _commandEncoder;
        private Rectangle _viewport;

        public EffectMaterial Material => _material;

        public SpriteBatch(ContentManager contentManager)
        {
            _vertexBuffer = AddDisposable(Buffer<SpriteVertex>.CreateDynamicArray(
                contentManager.GraphicsDevice,
                6,
                BufferBindFlags.VertexBuffer));

            _materialConstantsBuffer = AddDisposable(new ConstantBuffer<SpriteMaterial.MaterialConstants>(contentManager.GraphicsDevice));
            _material = new SpriteMaterial(contentManager.EffectLibrary.Sprite);
            _material.SetMaterialConstants(_materialConstantsBuffer.Buffer);
        }

        public void Begin(CommandEncoder commandEncoder, in Rectangle viewport)
        {
            _commandEncoder = commandEncoder;
            _viewport = viewport;
        }

        public void Draw(Texture texture, in Rectangle sourceRect, in Rectangle destinationRect, uint mipMapLevel = 0)
        {
            var uvL = sourceRect.X / (float) texture.Width;
            var uvT = sourceRect.Y / (float) texture.Height;
            var uvR = (sourceRect.X + sourceRect.Width) / (float) texture.Width;
            var uvB = (sourceRect.Y + sourceRect.Height) / (float) texture.Height;

            var left = (destinationRect.X / (float) _viewport.Width) * 2 - 1;
            var top = (destinationRect.Y / (float) _viewport.Height) * 2 - 1;
            var right = ((destinationRect.X + destinationRect.Width) / (float) _viewport.Width) * 2 - 1;
            var bottom = ((destinationRect.Y + destinationRect.Height) / (float) _viewport.Height) * 2 - 1;

            var vertices = new[]
            {
                new SpriteVertex(new Vector2(left, top * -1), new Vector2(uvL, uvT)),
                new SpriteVertex(new Vector2(right, top * -1), new Vector2(uvR, uvT)),
                new SpriteVertex(new Vector2(left, bottom * -1), new Vector2(uvL, uvB)),
                new SpriteVertex(new Vector2(right, top * -1), new Vector2(uvR, uvT)),
                new SpriteVertex(new Vector2(right, bottom * -1), new Vector2(uvR, uvB)),
                new SpriteVertex(new Vector2(left, bottom * -1), new Vector2(uvL, uvB))
            };
            _vertexBuffer.SetData(vertices);

            _commandEncoder.SetVertexBuffer(0, _vertexBuffer);

            _materialConstantsBuffer.Value.MipMapLevel = mipMapLevel;
            _materialConstantsBuffer.Update();

            _material.SetTexture(texture);

            _material.Apply();

            _material.Effect.Apply(_commandEncoder);

            _commandEncoder.Draw(PrimitiveType.TriangleList, 0, 6);
        }

        public void End()
        {
            // TODO: Batch draw calls.
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SpriteVertex
    {
        public Vector2 Position;
        public Vector2 UV;

        public SpriteVertex(Vector2 position, Vector2 uv)
        {
            Position = position;
            UV = uv;
        }

        public static readonly VertexDescriptor VertexDescriptor = new VertexDescriptor(
            new[]
            {
                new VertexAttributeDescription(InputClassification.PerVertexData, "POSITION", 0, VertexFormat.Float2, 0, 0),
                new VertexAttributeDescription(InputClassification.PerVertexData, "TEXCOORD", 0, VertexFormat.Float2, 8, 0)
            },
            new[]
            {
                new VertexLayoutDescription(16)
            });
    }
}
