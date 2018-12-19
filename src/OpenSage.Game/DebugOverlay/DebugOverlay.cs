using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using OpenSage.Content;
using OpenSage.Graphics.Cameras;
using OpenSage.Gui;
using OpenSage.Mathematics;
using SixLabors.Fonts;

namespace OpenSage.DebugOverlay
{
    public class DebugOverlay
    {
        private bool _overlayEnabled;

        private readonly Scene3D _scene3D;
        private readonly Font _debugFont;

        private readonly StringBuilder _debugStringBuilder;

        public DebugOverlay(Scene3D scene3D, ContentManager contentManager)
        {
            Points = new List<DebugPoint>();

            _scene3D = scene3D;
            _debugFont = contentManager.GetOrCreateFont("Arial", 16, FontWeight.Normal);
            _debugStringBuilder = new StringBuilder();
        }

        public List<DebugPoint> Points { get; }

        public Point2D MousePosition { get; internal set; }

        public void AddPoint(DebugPoint point)
        {
            Points.Add(point);
        }

        public void Update(GameTime gameTime)
        {
            _debugStringBuilder.Clear();
        }

        public void Draw(DrawingContext2D context, Camera camera)
        {
            if (!_overlayEnabled)
            {
                return;
            }

            foreach (var point in Points)
            {
                if (!point.Intersects(camera.BoundingFrustum))
                {
                    continue;
                }

                var rect = point.GetBoundingRectangle(camera).ToRectangleF();
                context.DrawRectangle(rect, point.Color, 1);
            }

            foreach (var gameObject in _scene3D.GameObjects.Items)
            {
                // TODO: Reuse frustum culling results.
                if (gameObject.Collider != null && gameObject.Collider.Intersects(camera.BoundingFrustum))
                {
                    gameObject.Collider?.Draw(context, camera);

                }
            }

            var worldPos = camera.ScreenToWorldPoint(new Vector3(MousePosition.X, MousePosition.Y, 0));

            _debugStringBuilder.AppendFormat("Screen: X:{0} Y: {1}\n", MousePosition.X, MousePosition.Y);

            // TODO: Calculate these based on a raycast?
            _debugStringBuilder.AppendFormat("World: X:{0} Y: {1} Z: {2}\n", Math.Round(worldPos.X, 3), Math.Round(worldPos.Y, 3), Math.Round(worldPos.Z, 3));
            _debugStringBuilder.AppendFormat("Tile: X:{0} Y: {1}\n", (int) worldPos.X / 10, (int) worldPos.Y / 10);

            context.DrawText(_debugStringBuilder.ToString(), _debugFont, TextAlignment.Leading, ColorRgbaF.White, new RectangleF(10, 10, 400, 80));
        }

        public void Toggle()
        {
            _overlayEnabled = !_overlayEnabled;
        }
    }
}
