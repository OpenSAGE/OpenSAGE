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
        public bool ShowQuadTree { get; set; }
        public bool ShowRoadMeshes { get; set; }

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
                _debugStringBuilder.AppendFormat("Terrain: X:{0} Y: {1} Z: {2}\n", MathF.Round(worldPos.X, 3), MathF.Round(worldPos.Y, 3), MathF.Round(worldPos.Z, 3));
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
                    if (gameObject.RoughCollider != null && gameObject.RoughCollider.Intersects(camera.BoundingFrustum))
                    {
                        //if (gameObject.Colliders.Count > 1)
                        //{
                        //    gameObject.RoughCollider.DebugDraw(context, camera);
                        //}

                        foreach (var collider in gameObject.Colliders)
                        {
                            collider.DebugDraw(context, camera);
                        }
                    }

                    var targetPoints = gameObject.AIUpdate?.TargetPoints;
                    if (targetPoints != null)
                    {
                        // Draw line to the first target point
                        if (targetPoints.Count > 0)
                        {
                            var p1 = gameObject.Translation;
                            var p2 = targetPoints[0];
                            var wp1 = camera.WorldToScreenPoint(p1).Vector2XY();
                            var wp2 = camera.WorldToScreenPoint(p2).Vector2XY();
                            context.DrawLine(new Line2D(wp1, wp2), 1.0f, ColorRgbaF.White);
                        }

                        for (var i = 1; i < targetPoints.Count;i++)
                        {
                            var p1 = targetPoints[i - 1];
                            var p2 = targetPoints[i];
                            var wp1 = camera.WorldToScreenPoint(p1).Vector2XY();
                            var wp2 = camera.WorldToScreenPoint(p2).Vector2XY();
                            context.DrawLine(new Line2D(wp1, wp2), 1.0f, ColorRgbaF.White);
                        }
                    }
                }
            }

            if (ShowQuadTree)
            {
                _scene3D.Quadtree.DebugDraw(context, camera);
            }

            if (_scene3D.ShowRoads && ShowRoadMeshes)
            {
                foreach (var road in _scene3D.Roads)
                {
                    road.DebugDraw(context, camera);
                }
            }


            // display impassable area
            //foreach(var node in _scene3D.Navigation._graph._nodes)
            //{
            //    if (!node.IsPassable)
            //    {
            //        var xy = _scene3D.Navigation.GetNodePosition(node);
            //        var xyz = camera.WorldToScreenPoint(new Vector3(xy, _scene3D.Terrain.HeightMap.GetHeight(xy.X, xy.Y)));
            //        var pos = xyz.Vector2XY();
            //        if (pos.X < 0.0 || pos.Y < 0.0 || pos.X > 1920 || pos.Y > 1080) continue;
            //        context.DrawRectangle(new RectangleF(xyz.Vector2XY(), new SizeF(10.0f)), ColorRgbaF.Red, 10.0f);
            //    }
            //}

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

        public void ToggleQuadTree()
        {
            ShowQuadTree = !ShowQuadTree;
        }

        public void ToggleRoadMeshes()
        {
            ShowRoadMeshes = !ShowRoadMeshes;
        }
    }
}
