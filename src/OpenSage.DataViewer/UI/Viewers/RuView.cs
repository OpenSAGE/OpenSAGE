using System;
using System.IO;
using System.Numerics;
using Eto.Forms;
using OpenSage.Content;
using OpenSage.Data;
using OpenSage.Data.Apt;
using OpenSage.DataViewer.Controls;
using OpenSage.Gui;
using OpenSage.Gui.Apt;
using OpenSage.Mathematics;
using Veldrid;

namespace OpenSage.DataViewer.UI.Viewers
{
    public sealed class RuView : Splitter
    {
        public RuView(FileSystemEntry entry, Func<IntPtr, Game> createGame)
        {
            string ruText;
            using (var fileStream = entry.Open())
            using (var streamReader = new StreamReader(fileStream))
            {
                ruText = streamReader.ReadToEnd();
            }

            Panel1 = new TextBox
            {
                ReadOnly = true,
                Width = 250,
                Text = ruText
            };

            Panel2 = new GameControl
            {
                CreateGame = h =>
                {
                    var game = createGame(h);

                    //load the corresponding .dat file
                    var movieName = entry.FilePath.Split('/')[0].Split('_')[0];
                    var datPath = movieName + ".dat";
                    var datEntry = game.ContentManager.FileSystem.GetFile(datPath);
                    var imageMap = ImageMap.FromFileSystemEntry(datEntry);

                    var shape = Geometry.FromFileSystemEntry(entry);

                    var shapeRenderer = new ShapeRenderer(
                        shape,
                        game.ContentManager,
                        imageMap,
                        movieName);

                    game.Rendering2D += (sender, e) => shapeRenderer.Render(e.DrawingContext);

                    game.Window.ClientSizeChanged += (sender, e) =>
                    {
                        shapeRenderer.Update(
                            game.GraphicsDevice,
                            game.Window.ClientBounds.Size);
                    };

                    return game;
                }
            };
        }

        private sealed class ShapeRenderer : DisposableBase
        {
            private readonly Geometry _shape;
            private readonly AptContext _context;
            private readonly ContentManager _contentManager;

            private Mathematics.Rectangle _frame;
            private float _scale;

            public ShapeRenderer(
                Geometry shape,
                ContentManager contentManager,
                ImageMap map,
                string movieName)
            {
                _shape = shape;
                _context = new AptContext(map, movieName, contentManager);
                _contentManager = contentManager;
            }

            public void Update(GraphicsDevice gd, in Size windowSize)
            {
                var shapeBoundingBox = _shape.BoundingBox;

                _frame = RectangleF.CalculateRectangleFittingAspectRatio(
                    shapeBoundingBox,
                    shapeBoundingBox.Size,
                    windowSize,
                    out _scale);
            }

            public void Render(DrawingContext2D drawingContext)
            {
                var shapeBoundingBox = _shape.BoundingBox;

                var translation = new Vector2(-shapeBoundingBox.X, -shapeBoundingBox.Y);

                var itemTransform = new ItemTransform(
                    ColorRgbaF.White,
                    Matrix3x2.CreateScale(_scale, _scale),
                    translation);

                AptRenderer.RenderGeometry(
                    drawingContext,
                    _context,
                    _shape,
                    itemTransform);
            }
        }
    }
}
