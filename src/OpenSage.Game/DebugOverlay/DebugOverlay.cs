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
        public bool Enabled { get; set; }
        public bool ShowColliders { get; set; }

        public Point2D MousePosition { get; internal set; }
        private Vector3? _mouseWorldPosition = null;

        private readonly Scene3D _scene3D;
        private readonly Font _debugFont;
        private readonly StringBuilder _debugStringBuilder;
        private readonly List<DebugPoint> _points;

        public DebugOverlay(Scene3D scene3D, ContentManager contentManager)
        {
            _points = new List<DebugPoint>();

            _scene3D = scene3D;
            _debugFont = contentManager.GetOrCreateFont("Arial", 16, FontWeight.Normal);
            _debugStringBuilder = new StringBuilder();
        }

        public void AddPoint(DebugPoint point)
        {
            _points.Add(point);
        }

        public void Update(GameTime gameTime)
        {
            var ray = _scene3D.Camera.ScreenPointToRay(new Vector2(MousePosition.X, MousePosition.Y));
            _mouseWorldPosition = _scene3D.Terrain.Intersect(ray);

            _debugStringBuilder.Clear();
            _debugStringBuilder.AppendFormat("Screen: X:{0} Y: {1}\n", MousePosition.X, MousePosition.Y);

            if (_mouseWorldPosition != null)
            {
                var worldPos = _mouseWorldPosition.Value;
                _debugStringBuilder.AppendFormat("Terrain: X:{0} Y: {1} Z: {2}\n", Math.Round(worldPos.X, 3), Math.Round(worldPos.Y, 3), Math.Round(worldPos.Z, 3));
                _debugStringBuilder.AppendFormat("Tile: X:{0} Y: {1}\n", (int) worldPos.X / 10, (int) worldPos.Y / 10);
            }
        }

        public void Draw(DrawingContext2D context, Camera camera)
        {
            if (!Enabled)
            {
                return;
            }

            foreach (var point in _points)
            {
                if (!camera.BoundingFrustum.Contains(point.Position))
                {
                    continue;
                }

                var rect = camera.WorldToScreenRectangle(point.Position, new Size(2));

                if (rect.HasValue)
                {
                    context.DrawRectangle(rect.Value, point.Color, 1);
                }
            }

            if (ShowColliders)
            {
                foreach (var gameObject in _scene3D.GameObjects.Items)
                {
                    // TODO: Reuse frustum culling results.
                    if (gameObject.Collider != null && gameObject.Collider.Intersects(camera.BoundingFrustum))
                    {
                        gameObject.Collider?.DebugDraw(context, camera);
                    }
                }
            }

            context.DrawText(_debugStringBuilder.ToString(), _debugFont, TextAlignment.Leading, ColorRgbaF.White, new RectangleF(10, 10, 400, 80));
        }

        public void Toggle()
        {
            Enabled = !Enabled;
        }

        public void ToggleColliders()
        {
            ShowColliders = !ShowColliders;
        }
    }
}
