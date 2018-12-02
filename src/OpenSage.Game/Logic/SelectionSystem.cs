using System;
using System.Collections.Generic;
using System.Numerics;
using OpenSage.Gui;
using OpenSage.Logic.Object;
using OpenSage.Logic.Orders;
using OpenSage.Mathematics;

namespace OpenSage.Logic
{
    public sealed class SelectionSystem : GameSystem
    {
        public enum SelectionStatus
        {
            NotSelecting,
            SingleSelecting,
            MultiSelecting
        }

        // TODO: Find out if there's an INI setting for this.
        // If not, add this to our custom settings when we have those.
        // This should probably scale with resolution.
        private const int BoxSelectionMinimumSize = 30;

        private SelectionGui SelectionGui => Game.Scene3D.SelectionGui;
        private readonly List<GameObject> _selectedObjects;

        private Point2D _startPoint;
        private Point2D _endPoint;

        public SelectionStatus Status { get; private set; } = SelectionStatus.NotSelecting;
        public bool Selecting => Status != SelectionStatus.NotSelecting;

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
            _selectedObjects = new List<GameObject>();
        }

        public void OnStartDragSelection(Point2D startPoint)
        {
            Status = SelectionStatus.SingleSelecting;
            _startPoint = startPoint;
            _endPoint = startPoint;
            SelectionGui.SelectionRectangle = SelectionRect;
        }

        public void OnDragSelection(Point2D point)
        {
            _endPoint = point;

            var rect = SelectionRect;

            // If either dimension is under 50 pixels, don't show the box selector.
            if (Status != SelectionStatus.MultiSelecting && UseBoxSelection(rect))
            {
                Status = SelectionStatus.MultiSelecting;
                // Note that the box can be scaled down after this.
                SelectionGui.SelectionBoxVisible = true;
            }

            SelectionGui.SelectionRectangle = rect;
        }

        public void OnEndDragSelection()
        {
            _selectedObjects.Clear();
            SelectionGui.SelectedObjects.Clear();

            if (Status == SelectionStatus.SingleSelecting)
            {
                SingleSelect();
            }
            else
            {
                MultiSelect();
            }

            SelectionGui.SelectionBoxVisible = false;
            Status = SelectionStatus.NotSelecting;
        }

        public void SetSelectedObject(Player player, GameObject unitOrBuilding)
        {
            player.SelectUnits(new[] { unitOrBuilding });

            if (player == Game.Scene3D.LocalPlayer)
            {
                SelectionGui.SelectedObjects.Add(unitOrBuilding.Collider);
                unitOrBuilding.OnLocalSelect(Game.Audio);
            }
        }

        public void ClearSelectedObjects(Player player)
        {
            player.DeselectUnits();

            if (player == Game.Scene3D.LocalPlayer)
            {
                SelectionGui.SelectedObjects.Clear();
            }
        }

        private void SingleSelect()
        {
            var ray = Game.Scene3D.Camera.ScreenPointToRay(new Vector2(_startPoint.X, _startPoint.Y));

            var closestDepth = float.MaxValue;
            GameObject closestObject = null;

            foreach (var gameObject in Game.Scene3D.GameObjects.Items)
            {
                if (!gameObject.IsSelectable || gameObject.Collider == null)
                {
                    continue;
                }

                if (gameObject.Collider.Intersects(ray, out var depth) && depth < closestDepth)
                {
                    closestDepth = depth;
                    closestObject = gameObject;
                }
            }

            if (closestObject != null)
            {
                _selectedObjects.Add(closestObject);

                Game.NetworkMessageBuffer.AddLocalOrder(Order.CreateSetSelection(
                    (uint) Game.Scene3D.GetPlayerIndex(Game.Scene3D.LocalPlayer),
                    (uint) Game.Scene3D.GameObjects.GetObjectId(closestObject)));
            }
        }

        private void MultiSelect()
        {
            var boxFrustum = GetSelectionFrustum(SelectionRect);

            // TODO: Optimize with a quadtree / use frustum culling?
            foreach (var gameObject in Game.Scene3D.GameObjects.Items)
            {
                if (!gameObject.IsSelectable || gameObject.Collider == null)
                {
                    continue;
                }

                // TODO: Support other colliders
                if (gameObject.Collider.Intersects(boxFrustum))
                {
                    _selectedObjects.Add(gameObject);
                    SelectionGui.SelectedObjects.Add(gameObject.Collider);
                }
            }
        }

        private static bool UseBoxSelection(Rectangle rect)
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
