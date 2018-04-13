using System;
using System.IO;
using System.Numerics;
using ImGuiNET;
using OpenSage.Content;
using OpenSage.Data.Apt;
using OpenSage.Graphics.Rendering;
using OpenSage.Gui;
using OpenSage.Gui.Apt;
using OpenSage.Mathematics;
using Veldrid;

namespace OpenSage.Viewer.UI.Views
{
    internal sealed class RuView : GameView
    {
        private readonly string _ruText;

        public RuView(AssetViewContext context)
            : base(context)
        {
            using (var fileStream = context.Entry.Open())
            using (var streamReader = new StreamReader(fileStream))
            {
                _ruText = streamReader.ReadToEnd();
            }

            var game = context.Game;

            //load the corresponding .dat file
            var movieName = context.Entry.FilePath.Split('/')[0].Split('_')[0];
            var datPath = movieName + ".dat";
            var datEntry = game.ContentManager.FileSystem.GetFile(datPath);
            var imageMap = ImageMap.FromFileSystemEntry(datEntry);

            var shape = Geometry.FromFileSystemEntry(context.Entry);

            var shapeRenderer = new ShapeRenderer(
                shape,
                game.ContentManager,
                imageMap,
                movieName);

            void onRendering2D(object sender, Rendering2DEventArgs e)
            {
                shapeRenderer.Render(e.DrawingContext);
            }

            game.Rendering2D += onRendering2D;

            AddDisposeAction(() => game.Rendering2D -= onRendering2D);

            void onClientSizeChanged(object sender, EventArgs e)
            {
                shapeRenderer.Update(
                    game.GraphicsDevice,
                    game.Panel.ClientBounds.Size);
            }

            game.Panel.ClientSizeChanged += onClientSizeChanged;

            AddDisposeAction(() => game.Panel.ClientSizeChanged -= onClientSizeChanged);
        }

        public override void Draw(ref bool isGameViewFocused)
        {
            ImGui.BeginChild("ru sidebar", new Vector2(250, 0), true, 0);

            ImGui.TextWrapped(_ruText);

            ImGui.EndChild();

            ImGui.SameLine();

            base.Draw(ref isGameViewFocused);
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
