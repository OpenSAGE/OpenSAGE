using System;
using System.Linq;
using System.Numerics;
using ImGuiNET;
using OpenSage.Content;
using OpenSage.Data.Apt;
using OpenSage.Gui;
using OpenSage.Gui.Apt;
using OpenSage.Mathematics;
using Veldrid;

namespace OpenSage.Diagnostics
{
    internal sealed class AptGeometryView : DiagnosticView
    {
        private readonly RenderedView _renderedView;

        private AptWindow _currentWindow;
        private string[] _geometryNames;
        private int _currentGeometry;

        private ShapeRenderer _shapeRenderer;

        private Vector2 _cachedSize;

        public override string DisplayName { get; } = "APT Geometry";

        public AptGeometryView(DiagnosticViewContext context)
            : base(context)
        {
            _renderedView = AddDisposable(new RenderedView(context));

            _renderedView.RenderPipeline.Rendering2D += (sender, e) =>
            {
                _shapeRenderer.Render(e.DrawingContext);
            };
        }

        protected override void DrawOverride(ref bool isGameViewFocused)
        {
            if (Context.SelectedAptWindow == null)
            {
                ImGui.Text("APT geometry is only available when an APT window has been selected in the APT Windows window.");
                return;
            }

            var geometryChanged = false;

            if (Context.SelectedAptWindow != _currentWindow)
            {
                _geometryNames = Context.SelectedAptWindow.AptFile.GeometryMap.Keys
                    .Select(x => x.ToString())
                    .ToArray();
                _currentWindow = Context.SelectedAptWindow;
                _currentGeometry = 0;
                geometryChanged = true;
            }

            if (ImGui.Combo("Geometry", ref _currentGeometry, _geometryNames, _geometryNames.Length))
            {
                geometryChanged = true;
            }

            var geometry = Context.SelectedAptWindow.AptFile.GeometryMap[Convert.ToUInt32(_geometryNames[_currentGeometry])];

            ImGui.BeginChild("geometry sidebar", new Vector2(150, 0), true, 0);
            ImGui.TextWrapped(geometry.RawText);
            ImGui.EndChild();

            ImGui.SameLine();

            if (geometryChanged)
            {
                _shapeRenderer = new ShapeRenderer(
                    geometry,
                    Context.Game.ContentManager,
                    Context.Game.AssetStore,
                    Context.SelectedAptWindow.AptFile.ImageMap,
                    Context.SelectedAptWindow.AptFile.MovieName);
            }

            var currentSize = ImGui.GetContentRegionAvail();
            if (geometryChanged || _cachedSize != currentSize)
            {
                var newSize = new Size((int) currentSize.X, (int) currentSize.Y);

                _shapeRenderer.Update(
                    Context.Game.GraphicsDevice,
                    newSize);

                _cachedSize = currentSize;
            }

            _renderedView.Draw();
        }

        private sealed class ShapeRenderer
        {
            private readonly Geometry _shape;
            private readonly AptContext _context;
            private readonly AptRenderer _renderer;

            private float _scale;

            public ShapeRenderer(
                Geometry shape,
                ContentManager contentManager,
                AssetStore assetStore,
                ImageMap map,
                string movieName)
            {
                _shape = shape;
                _context = new AptContext(map, movieName, assetStore);
                _renderer = new AptRenderer(null, contentManager);
            }

            public void Update(GraphicsDevice gd, in Size windowSize)
            {
                var shapeBoundingBox = _shape.BoundingBox;

                var target = RectangleF.CalculateRectangleFittingAspectRatio(
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

                _renderer.RenderGeometry(
                    drawingContext,
                    _context,
                    _shape,
                    itemTransform,
                    null);
            }
        }
    }
}
