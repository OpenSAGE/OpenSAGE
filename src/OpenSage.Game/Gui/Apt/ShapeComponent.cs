using System;
using System.Collections.Generic;
using System.Numerics;
using OpenSage.Content;
using OpenSage.Data.Apt;
using OpenSage.Graphics;
using OpenSage.Graphics.Effects;
using OpenSage.LowLevel;
using OpenSage.LowLevel.Graphics2D;
using OpenSage.LowLevel.Graphics3D;
using OpenSage.Mathematics;

namespace OpenSage.Gui.Apt
{
    public sealed class ShapeComponent : EntityComponent
    {
        private Texture _texture;
        private AptContext _context;

        private Buffer<SpriteVertex> _vertexBuffer;
        private ConstantBuffer<SpriteMaterial.MaterialConstants> _materialConstantsBuffer;

        public Geometry Shape { get; set; }
        public String MovieName { get; set; }

        public Buffer<SpriteVertex> VertexBuffer => _vertexBuffer;
        public SpriteMaterial Material { get; private set; }

        public void Initialize(ContentManager contentManager, ImageMap map)
        {
            _context = new AptContext(map,MovieName, contentManager);

            Material = new SpriteMaterial(contentManager.EffectLibrary.Sprite);

            _materialConstantsBuffer = new ConstantBuffer<SpriteMaterial.MaterialConstants>(contentManager.GraphicsDevice);
            _materialConstantsBuffer.Value.Opacity = 1;
            _materialConstantsBuffer.Update();

            Material.SetMaterialConstants(_materialConstantsBuffer.Buffer);
        }

        protected override void Destroy()
        {
            base.Destroy();

            if (_texture != null)
            {
                _texture.Dispose();
                _texture = null;
            }

            if (_vertexBuffer != null)
            {
                _vertexBuffer.Dispose();
                _vertexBuffer = null;
            }

            if (_materialConstantsBuffer != null)
            {
                _materialConstantsBuffer.Dispose();
                _materialConstantsBuffer = null;
            }
        }

        public void Layout(GraphicsDevice gd, in Size windowSize)
        {
            float _scale = 0.0f;

            var frame = RectangleF.CalculateRectangleFittingAspectRatio(
                Shape.BoundingBox,
                Shape.BoundingBox.Size,
                windowSize,
                out _scale);

            if (_texture != null)
            {
                _texture.Dispose();
                _texture = null;
            }

            _texture = Texture.CreateTexture2D(
                gd,
                PixelFormat.Rgba8UNorm,
                frame.Width,
                frame.Height,
                TextureBindFlags.ShaderResource | TextureBindFlags.RenderTarget);

            Material.SetTexture(_texture);

            var left = (frame.X / (float) windowSize.Width) * 2 - 1;
            var top = (frame.Y / (float) windowSize.Height) * 2 - 1;
            var right = ((frame.X + frame.Width) / (float) windowSize.Width) * 2 - 1;
            var bottom = ((frame.Y + frame.Height) / (float) windowSize.Height) * 2 - 1;

            var vertices = new[]
            {
                new SpriteVertex(new Vector2(left, top * -1), new Vector2(0, 0)),
                new SpriteVertex(new Vector2(right, top * -1), new Vector2(1, 0)),
                new SpriteVertex(new Vector2(left, bottom * -1), new Vector2(0, 1)),
                new SpriteVertex(new Vector2(right, top * -1), new Vector2(1, 0)),
                new SpriteVertex(new Vector2(right, bottom * -1), new Vector2(1, 1)),
                new SpriteVertex(new Vector2(left, bottom * -1), new Vector2(0, 1))
            };

            if (_vertexBuffer != null)
            {
                _vertexBuffer.Dispose();
                _vertexBuffer = null;
            }

            _vertexBuffer = Buffer<SpriteVertex>.CreateStatic(
                gd,
                vertices,
                BufferBindFlags.VertexBuffer);

        }

        public void Draw(GraphicsDevice gd)
        {
            using (var drawingContext = new DrawingContext(HostPlatform.GraphicsDevice2D, _texture))
            {
                drawingContext.Begin();

                drawingContext.Clear(new ColorRgbaF(0, 0, 0, 0));

                AptRenderer.RenderGeometry(drawingContext, _context, Shape,Matrix3x2.Identity);

                drawingContext.End();
            }
        }
    }
}
