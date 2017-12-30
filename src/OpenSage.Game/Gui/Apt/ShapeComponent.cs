using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using OpenSage.Content;
using OpenSage.Data.Ini;
using OpenSage.Data.Apt;
using OpenSage.Graphics;
using OpenSage.Graphics.Effects;
using OpenSage.LowLevel;
using OpenSage.LowLevel.Graphics2D;
using OpenSage.LowLevel.Graphics3D;
using OpenSage.Mathematics;
using OpenSage.Data.Wnd;

namespace OpenSage.Gui.Apt
{
    public sealed class ShapeComponent : EntityComponent
    {
        private Texture _texture;
        private Buffer<SpriteVertex> _vertexBuffer;
        private ConstantBuffer<SpriteMaterial.MaterialConstants> _materialConstantsBuffer;

        public Geometry Shape { get; set; }
        public Buffer<SpriteVertex> VertexBuffer => _vertexBuffer;
        public SpriteMaterial Material { get; private set; }


        public void Initialize(ContentManager contentManager)
        {
            Material = new SpriteMaterial(contentManager.EffectLibrary.Sprite);

            _materialConstantsBuffer = new ConstantBuffer<SpriteMaterial.MaterialConstants>(contentManager.GraphicsDevice);
            _materialConstantsBuffer.Value.Opacity = 1;
            _materialConstantsBuffer.Update();

            Material.SetMaterialConstants(_materialConstantsBuffer.Buffer);
        }

        private static Rectangle CalculateFrame(in WndScreenRect wndScreenRect, in Size viewportSize, out float scale)
        {
            // Figure out the ratio.
            var ratioX = viewportSize.Width / (float) wndScreenRect.CreationResolution.Width;
            var ratioY = viewportSize.Height / (float) wndScreenRect.CreationResolution.Height;

            // Use whichever multiplier is smaller.
            var ratio = ratioX < ratioY ? ratioX : ratioY;

            scale = ratio;

            var originalWidth = wndScreenRect.BottomRight.X - wndScreenRect.UpperLeft.X;
            var originalHeight = wndScreenRect.BottomRight.Y - wndScreenRect.UpperLeft.Y;

            // Now we can get the new height and width
            var newWidth = (int) Math.Round(originalWidth * ratio);
            var newHeight = (int) Math.Round(originalHeight * ratio);

            newWidth = Math.Max(newWidth, 1);
            newHeight = Math.Max(newHeight, 1);

            var newX = (int) Math.Round(wndScreenRect.UpperLeft.X * ratio);
            var newY = (int) Math.Round(wndScreenRect.UpperLeft.Y * ratio);

            // Now calculate the X,Y position of the upper-left corner 
            // (one of these will always be zero for the top level window)
            var posX = (int) Math.Round((viewportSize.Width - (wndScreenRect.CreationResolution.Width * ratio)) / 2) + newX;
            var posY = (int) Math.Round((viewportSize.Height - (wndScreenRect.CreationResolution.Height * ratio)) / 2) + newY;

            return new Rectangle(posX, posY, newWidth, newHeight);
        }

        public void Layout(GraphicsDevice gd, in Size windowSize)
        {
            float _scale = 0.0f;
            var rect = new WndScreenRect
            {
                BottomRight = new WndPoint() { X = 1024, Y = 768 },
                UpperLeft = new WndPoint() { X = 0, Y = 0 }
            };
            var Frame = CalculateFrame(rect, windowSize, out _scale);


            _texture = Texture.CreateTexture2D(
                gd,
                PixelFormat.Rgba8UNorm,
                Frame.Width,
                Frame.Height,
                TextureBindFlags.ShaderResource | TextureBindFlags.RenderTarget);

            Material.SetTexture(_texture);

            var left = (Frame.X / (float) windowSize.Width) * 2 - 1;
            var top = (Frame.Y / (float) windowSize.Height) * 2 - 1;
            var right = ((Frame.X + Frame.Width) / (float) windowSize.Width) * 2 - 1;
            var bottom = ((Frame.Y + Frame.Height) / (float) windowSize.Height) * 2 - 1;

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

        private ColorRgbaF ToFloatColor(ColorRgba color)
        {
            ColorRgbaF floatColor;
            floatColor.R = color.R / 255.0f;
            floatColor.G = color.G / 255.0f;
            floatColor.B = color.B / 255.0f;
            floatColor.A = color.A / 255.0f;
            return floatColor;
        }

        public void Draw(GraphicsDevice gd)
        {
            using (var drawingContext = new DrawingContext(HostPlatform.GraphicsDevice2D, _texture))
            {
                drawingContext.Begin();

                drawingContext.Clear(new ColorRgbaF(0,0,0,1));

                foreach (var e in Shape.Entries)
                {
                    if(e is GeometryLines l)
                    {
                        foreach(var line in l.Lines)
                        {
                            RawLineF rl;
                            rl.X1 = line.V0.X;
                            rl.Y1 = line.V0.Y;
                            rl.X2 = line.V1.X;
                            rl.Y2 = line.V1.Y;
                            rl.Thickness = l.Thickness;
                            drawingContext.DrawLine(rl, ToFloatColor(l.Color));
                        }                     
                    }
                }

                drawingContext.End();
            }
        }
    }
}
