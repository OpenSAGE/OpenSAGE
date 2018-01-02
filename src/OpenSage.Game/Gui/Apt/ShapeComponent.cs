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
        private ImageMap _map;
        private Dictionary<int, Texture> _usedTextures;
        private Texture _texture;
        
        private Buffer<SpriteVertex> _vertexBuffer;
        private ConstantBuffer<SpriteMaterial.MaterialConstants> _materialConstantsBuffer;

        public Geometry Shape { get; set; }
        public String MovieName { get; set; }

        public Buffer<SpriteVertex> VertexBuffer => _vertexBuffer;
        public SpriteMaterial Material { get; private set; }


        public void Initialize(ContentManager contentManager,ImageMap map)
        {
            _map = map;
            _usedTextures = new Dictionary<int, Texture>();

            //get all used image id's
            var usedIds = new HashSet<int>();
            foreach(var entry in Shape.Entries)
            {
                if(entry is GeometryTexturedTriangles gtt)
                {
                    var assignment = _map.Mapping[gtt.Image];

                    usedIds.Add(assignment.TextureId);
                }
            }

            foreach(var id in usedIds)
            {
                var texturePath = "art/Textures/apt_" + MovieName + "_" + id.ToString()+".tga";
                var loadOptions = new TextureLoadOptions() { GenerateMipMaps = false };
                _usedTextures.Add(id, contentManager.Load<Texture>(texturePath, loadOptions));
            }

            Material = new SpriteMaterial(contentManager.EffectLibrary.Sprite);

            _materialConstantsBuffer = new ConstantBuffer<SpriteMaterial.MaterialConstants>(contentManager.GraphicsDevice);
            _materialConstantsBuffer.Value.Opacity = 1;
            _materialConstantsBuffer.Update();

            Material.SetMaterialConstants(_materialConstantsBuffer.Buffer);
        }

        private static Rectangle CalculateFrame(in RectangleF shapeBounding, in Size viewportSize, out float scale)
        {
            // Figure out the ratio.
            var ratioX = viewportSize.Width / (float) shapeBounding.Width;
            var ratioY = viewportSize.Height / (float) shapeBounding.Height;

            // Use whichever multiplier is smaller.
            var ratio = ratioX < ratioY ? ratioX : ratioY;

            scale = ratio;

            var originalWidth = shapeBounding.Width;
            var originalHeight = shapeBounding.Height;

            // Now we can get the new height and width
            var newWidth = (int) Math.Round(originalWidth * ratio);
            var newHeight = (int) Math.Round(originalHeight * ratio);

            newWidth = Math.Max(newWidth, 1);
            newHeight = Math.Max(newHeight, 1);

            var newX = (int) Math.Round(shapeBounding.X * ratio);
            var newY = (int) Math.Round(shapeBounding.Y * ratio);

            // Now calculate the X,Y position of the upper-left corner 
            // (one of these will always be zero for the top level window)
            var posX = (int) Math.Round((viewportSize.Width - (shapeBounding.Width * ratio)) / 2);
            var posY = (int) Math.Round((viewportSize.Height - (shapeBounding.Height * ratio)) / 2);

            return new Rectangle(posX, posY, newWidth, newHeight);
        }

        public void Layout(GraphicsDevice gd, in Size windowSize)
        {
            float _scale = 0.0f;

            var Frame = CalculateFrame(Shape.BoundingBox, windowSize, out _scale);

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
                    else if(e is GeometrySolidTriangles st)
                    {
                        foreach(var tri in st.Triangles)
                        {
                            RawTriangleF rt;
                            rt.X1 = tri.V0.X;
                            rt.Y1 = tri.V0.Y;
                            rt.X2 = tri.V1.X;
                            rt.Y2 = tri.V1.Y;
                            rt.X3 = tri.V2.X;
                            rt.Y3 = tri.V2.Y;
                            drawingContext.FillTriangle(rt, ToFloatColor(st.Color));
                        }
                    }
                    else if (e is GeometryTexturedTriangles tt)
                    {
                        foreach (var tri in tt.Triangles)
                        {
                            RawTriangleF rt;
                            rt.X1 = tri.V0.X;
                            rt.Y1 = tri.V0.Y;
                            rt.X2 = tri.V1.X;
                            rt.Y2 = tri.V1.Y;
                            rt.X3 = tri.V2.X;
                            rt.Y3 = tri.V2.Y;
                            RawMatrix3x2 transform;
                            transform.M11 = tt.Rotation.M11;
                            transform.M12 = tt.Rotation.M12;
                            transform.M21 = tt.Rotation.M21;
                            transform.M22 = tt.Rotation.M22;
                            transform.M31 = -tt.Translation.X;
                            transform.M32 = -tt.Translation.Y;
                            
                            var assignment = _map.Mapping[tt.Image];
                            var texId = assignment.TextureId;

                            //if (assignment is RectangleAssignment)
                            //    throw new NotImplementedException();

                            var tex = _usedTextures[texId];

                            drawingContext.FillTriangle(rt, tex,transform);
                        }
                    }
                }

                drawingContext.End();
            }
        }
    }
}
