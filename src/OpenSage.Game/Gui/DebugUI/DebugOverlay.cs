using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using OpenSage.Content;
using OpenSage.Graphics.Cameras;
using OpenSage.Mathematics;
using SixLabors.Fonts;

namespace OpenSage.Gui.DebugUI
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

        private readonly List<IDebugDrawable> _debugDrawables;

        public DebugOverlay(Scene3D scene3D, ContentManager contentManager)
        {
            _scene3D = scene3D;
            _debugFont = contentManager.FontManager.GetOrCreateFont("Arial", 16, FontWeight.Normal);
            _debugStringBuilder = new StringBuilder();

            _debugDrawables = new List<IDebugDrawable>();
        }

        public void AddPoint(in Vector3 point, in ColorRgbaF color, float? duration = null)
        {
            _debugDrawables.Add(new DebugPoint(point, color, duration));
        }

        public void DrawPoint(in Vector3 point, in ColorRgbaF color)
        {
            AddPoint(point, color, 0);
        }

        public void AddLine(in Vector3 start, in Vector3 end, in ColorRgbaF color, float? duration = null)
        {
            _debugDrawables.Add(new DebugLine(start, end, color, duration));
        }

        public void DrawLine(in Vector3 start, in Vector3 end, ColorRgbaF color)
        {
            AddLine(start, end, color, 0);
        }

        public void AddCoordAxes(in Vector3 point, float? duration = null)
        {
            _debugDrawables.Add(new DebugCoordAxes(point, duration));
        }

        public void DrawCoordAxes(in Vector3 point)
        {
            AddCoordAxes(point, 0);
        }

        public void Update(in TimeInterval gameTime)
        {
            foreach (var drawable in _debugDrawables)
            {
                if (!drawable.Timer.HasValue || drawable.Timer == 0)
                {
                    continue;
                }

                drawable.Timer = Math.Max(0, drawable.Timer.Value - (float) gameTime.DeltaTime.TotalSeconds);
            }

            if (!Enabled)
            {
                return;
            }

            var ray = _scene3D.Camera.ScreenPointToRay(new Vector2(MousePosition.X, MousePosition.Y));
            _mouseWorldPosition = _scene3D.Terrain?.Intersect(ray);

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

            foreach (var drawable in _debugDrawables)
            {
                drawable.Render(context, camera);
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

                    if(gameObject.TargetPoints != null)
                    {
                        for(int i=1;i<gameObject.TargetPoints.Count;i++)
                        {
                            var p1 = gameObject.TargetPoints[i - 1];
                            var p2 = gameObject.TargetPoints[i];
                            var wp1 = camera.WorldToScreenPoint(p1).Vector2XY();
                            var wp2 = camera.WorldToScreenPoint(p2).Vector2XY();
                            context.DrawLine(new Line2D(wp1, wp2), 1.0f, ColorRgbaF.White);
                        }
                    }
                }
            }

            context.DrawText(_debugStringBuilder.ToString(), _debugFont, TextAlignment.Leading, ColorRgbaF.White, new RectangleF(10, 10, 400, 80));

            // This is done here instead of in Update so that drawables with time of 0 will be drawn at least once.
            _debugDrawables.RemoveAll(x => x.Timer.HasValue && x.Timer.Value == 0);
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
