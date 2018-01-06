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
    public sealed class AptComponent : EntityComponent
    {
        private Texture _texture;
        private Buffer<SpriteVertex> _vertexBuffer;
        private ConstantBuffer<SpriteMaterial.MaterialConstants> _materialConstantsBuffer;
        private AptContext _context;

        public AptFile Apt { get; set; }
        public SpriteItem Root { get; private set; }

        public Buffer<SpriteVertex> VertexBuffer => _vertexBuffer;
        public SpriteMaterial Material { get; private set; }

        public void Initialize(ContentManager contentManager)
        {
            //Create our context
            _context = new AptContext(Apt, contentManager);

            Material = new SpriteMaterial(contentManager.EffectLibrary.Sprite);

            _materialConstantsBuffer = new ConstantBuffer<SpriteMaterial.MaterialConstants>(contentManager.GraphicsDevice);
            _materialConstantsBuffer.Value.Opacity = 1;
            _materialConstantsBuffer.Update();

            Material.SetMaterialConstants(_materialConstantsBuffer.Buffer);

            //First thing to do here is to initialize the display list
            Root = new SpriteItem();
            Root.Create(Apt.Movie, _context);
        }

        public void Layout(GraphicsDevice gd, in Size windowSize)
        {
            var frame = new Rectangle(0, 0, 1024, 768);

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
            _vertexBuffer = Buffer<SpriteVertex>.CreateStatic(
                gd,
                vertices,
                BufferBindFlags.VertexBuffer);

        }

        public void Update(GameTime gt, GraphicsDevice gd)
        {
            using (var drawingContext = new DrawingContext(HostPlatform.GraphicsDevice2D, _texture))
            {
                drawingContext.Begin();
                drawingContext.Clear(new ColorRgbaF(0, 0, 0, 0));

                //draw the movieclip, which has no transformation
                Root.Update(new Matrix3x2(), gt, drawingContext);

                drawingContext.End();
            }
        }
    }
}
