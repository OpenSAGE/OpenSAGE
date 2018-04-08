using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using OpenSage.Gui;
using OpenSage.Logic.Object;
using OpenSage.Mathematics;

namespace OpenSage.Logic
{
    public sealed class SelectionSystem : GameSystem
    {
        private enum SelectionStatus
        {
            NotSelecting,
            SingleSelecting,
            MultiSelecting
        }

        // TODO: Find out if there's an INI setting for this.
        // If not, add this to our custom settings when we have those.
        // This should probably scale with resolution.
        private const int BoxSelectionMinimumSize = 30;

        private readonly SelectionGui _selectionGui;
        private readonly List<GameObject> _selectedObjects;

        private Point2D _startPoint;
        private Point2D _endPoint;
        private SelectionStatus _status = SelectionStatus.NotSelecting;
        public bool Selecting => _status != SelectionStatus.NotSelecting;

        private Rectangle SelectionRect
        {
            get
            {
                var topLeft = Point2D.Min(_startPoint, _endPoint);
                var bottomRight = Point2D.Max(_startPoint, _endPoint);

                return new Rectangle(topLeft,
                    new Size(bottomRight.X - topLeft.X, bottomRight.Y - topLeft.Y));
            }
        }

        public SelectionSystem(Game game) : base(game)
        {
            _selectionGui = game.Scene2D.SelectionGui;
            _selectedObjects = new List<GameObject>();
        }

        public void OnStartDragSelection(Point2D startPoint)
        {
            _status = SelectionStatus.SingleSelecting;
            _startPoint = startPoint;
            _endPoint = startPoint;
            _selectionGui.SelectionRectangle = SelectionRect;
        }

        public void OnDragSelection(Point2D point)
        {
            _endPoint = point;

            var rect = SelectionRect;

            // If either dimension is under 50 pixels, don't show the box selector.
            if (!_selectionGui.SelectionBoxVisible && UseBoxSelectionForRect(rect))
            {
                _status = SelectionStatus.MultiSelecting;
                // Note that the box can be scaled down after this.
                _selectionGui.SelectionBoxVisible = true;
            }

            _selectionGui.SelectionRectangle = rect;
        }

        public void OnEndDragSelection()
        {
            // TODO: Handle multi / single selection

            _selectedObjects.Clear();
            _selectionGui.DebugOverlays.Clear();

            var boxFrustum = GetSelectionFrustum(SelectionRect);

            // TODO: Optimize with a quadtree / use frustum culling?
            foreach (var gameObject in Game.Scene3D.GameObjects.Items)
            {
                if (gameObject.Collider is BoxCollider box)
                {
                    var worldBox = box.Bounds.Transform(gameObject.Transform.Matrix);

                    if (boxFrustum.Intersects(worldBox))
                    {
                        _selectedObjects.Add(gameObject);
                        _selectionGui.DebugOverlays.Add(worldBox.ToScreenRectangle(Game.Scene3D.Camera));
                    }
                }
            }

            if (_selectedObjects.Count > 0)
            {
                Debug.WriteLine("Objects: ");
                foreach (var obj in _selectedObjects)
                {
                    Debug.WriteLine(obj.Definition.Name);
                }
                Debug.WriteLine("");
            }

            _selectionGui.SelectionBoxVisible = false;
            _status = SelectionStatus.NotSelecting;
        }

        private static bool UseBoxSelectionForRect(Rectangle rect)
        {
            return Math.Max(rect.Width, rect.Height) >= BoxSelectionMinimumSize;
        }

        // Based on
        // https://ghoscher.com/2010/12/09/xna-picking-tutorial-part-ii/
        private BoundingFrustum GetSelectionFrustum(Rectangle rect)
        {
            var viewport = Game.Viewport;
            var viewportSize = new Vector2(viewport.Width, viewport.Height);
 
            var rectSize = new Vector2(rect.Width, rect.Height);
            var rectSizeHalf = rectSize / 2f;
            var rectCenter = new Vector2(rect.Left, rect.Top) + rectSizeHalf;

            var sizeDivisor = rectSize / viewportSize;
            var center = (rectCenter - viewportSize / 2f) / rectSizeHalf;

            var boxFrustumMatrix = Game.Scene3D.Camera.Projection;
            boxFrustumMatrix.M11 /= sizeDivisor.X;
            boxFrustumMatrix.M22 /= sizeDivisor.Y;
            boxFrustumMatrix.M31 = center.X;
            boxFrustumMatrix.M32 = -center.Y;

            var boxFrustum = new BoundingFrustum(Game.Scene3D.Camera.View * boxFrustumMatrix);
            return boxFrustum;
        }
    }
}
