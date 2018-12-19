using System;
using System.Collections.Generic;
using System.Numerics;
using OpenSage.Graphics.Cameras;
using OpenSage.Gui;
using OpenSage.Mathematics;
using SixLabors.Fonts;

namespace OpenSage.DebugOverlay
{
    public class DebugOverlay : GameSystem
    {
        private bool _showDebugInformations;
        private bool _showGridPoints;
        private readonly Scene3D _scene3D;

        public DebugOverlay(Game game, Scene3D scene3D) : base(game)
        {
            _scene3D = scene3D;
            Points = new List<DebugPoint>();
            new List<DebugPoint>();
        }

        public List<DebugPoint> Points { get; set; }
        public Point2D MousePosition { get; set; }

        public void AddPoint(DebugPoint point)
        {
            Points.Add(point);
        }

        public void Draw(DrawingContext2D context, Camera camera)
        {
            if (_showDebugInformations)
            {
                foreach (var point in Points)
                {
                    var rect = point.GetBoundingRectangle(camera);
                    //ugly hack to remove wrong calculated points
                    if (rect.Width > 10)
                    {
                        continue;
                    }

                    if (point.Intersects(camera.BoundingFrustum))
                    {
                        context.DrawRectangle(rect.ToRectangleF(), point.Color, 1);
                    }
                }

                foreach (var gameObject in _scene3D.GameObjects.Items)
                {
                    gameObject.Collider?.Draw(context, camera);
                }

                context.DrawText($"Screen: X:{MousePosition.X} Y: {MousePosition.Y}",
                    SystemFonts.CreateFont("Arial", 16, FontStyle.Regular),
                    TextAlignment.Leading, ColorRgbaF.White, new RectangleF(10, 10, 400, 25));
                var worldPos =
                    camera.ScreenToWorldPoint(new Vector3(MousePosition.X, MousePosition.Y, camera.FarPlaneDistance));
                context.DrawText(
                    $"World: X:{Math.Round(worldPos.X, 3)} Y: {Math.Round(worldPos.Y, 3)} Z: {Math.Round(worldPos.Z, 3)}",
                    SystemFonts.CreateFont("Arial", 16, FontStyle.Regular),
                    TextAlignment.Leading, ColorRgbaF.White, new RectangleF(10, 30, 400, 25));
                var cursor = new DebugPoint(worldPos);
                var rectCursor = cursor.GetBoundingRectangle(camera);
                context.DrawRectangle(rectCursor.ToRectangleF(), new ColorRgbaF(255, 0, 0, 255), 1);
                context.DrawText($"Tile: X:{(int) worldPos.X / 10} Y: {(int) worldPos.Y / 10}",
                    SystemFonts.CreateFont("Arial", 16, FontStyle.Regular),
                    TextAlignment.Leading, ColorRgbaF.White, new RectangleF(10, 50, 400, 25));
            }
        }

        public void ToggleDebugView()
        {
            _showDebugInformations = !_showDebugInformations;
        }

        public void ToggleGridPointDebugView()
        {
            _showGridPoints = !_showGridPoints;
        }
    }
}
