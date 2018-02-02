using System;
using OpenSage.Content;
using OpenSage.Data.Apt;
using OpenSage.Graphics;
using OpenSage.Mathematics;
using Veldrid;

namespace OpenSage.Gui.Apt
{
    public sealed class ShapeComponent : EntityComponent
    {
        private Texture _texture;
        private DrawingContext2D _primitiveBatch;
        private AptContext _context;

        private Rectangle _frame;

        public Geometry Shape { get; set; }
        public string MovieName { get; set; }

        public void Initialize(ContentManager contentManager, ImageMap map)
        {
            _context = new AptContext(map,MovieName, contentManager);
        }

        protected override void Destroy()
        {
            base.Destroy();

            if (_primitiveBatch != null)
            {
                _primitiveBatch.Dispose();
                _primitiveBatch = null;
            }

            if (_texture != null)
            {
                _texture.Dispose();
                _texture = null;
            }
        }

        public void Layout(GraphicsDevice gd, in Size windowSize)
        {
            float _scale = 0.0f;

            _frame = RectangleF.CalculateRectangleFittingAspectRatio(
                Shape.BoundingBox,
                Shape.BoundingBox.Size,
                windowSize,
                out _scale);

            if (_texture != null)
            {
                _texture.Dispose();
                _texture = null;
            }

            _texture = gd.ResourceFactory.CreateTexture(
                TextureDescription.Texture2D(
                    (uint) _frame.Width,
                    (uint) _frame.Height,
                    1,
                    1,
                    PixelFormat.R8_G8_B8_A8_UNorm,
                    TextureUsage.Sampled | TextureUsage.RenderTarget));

            if (_primitiveBatch != null)
            {
                _primitiveBatch.Dispose();
                _primitiveBatch = null;
            }

            _primitiveBatch = new DrawingContext2D(ContentManager, _texture);
        }

        public void Draw(GraphicsDevice gd)
        {
            _primitiveBatch.Begin(ContentManager.LinearClampSampler, ColorRgbaF.Transparent);

            AptRenderer.RenderGeometry(_primitiveBatch, _context, Shape, ItemTransform.None);

            _primitiveBatch.End();
        }

        internal void Render(SpriteBatch spriteBatch)
        {
            spriteBatch.DrawImage(
                _texture,
                null,
                _frame.ToRectangleF(),
                ColorRgbaF.White);
        }
    }
}
