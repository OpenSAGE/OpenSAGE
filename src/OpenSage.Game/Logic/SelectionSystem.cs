using System;
using System.Collections.Generic;
using System.Linq;
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

        public SelectionSystem(Game game) : base(game) { }

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

        public void SetSelectedObjects(Player player, GameObject[] objects)
        {
            player.SelectUnits(objects);

            if (player == Game.Scene3D.LocalPlayer)
            {
                objects[0].OnLocalSelect(Game.Audio);

                foreach (var obj in objects)
                {
                    SelectionGui.SelectedObjects.Add(obj.Collider);
                }
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

            var playerId = Game.Scene3D.GetPlayerIndex(Game.Scene3D.LocalPlayer);
            Game.NetworkMessageBuffer?.AddLocalOrder(Order.CreateClearSelection(playerId));

            if (closestObject != null)
            {
                var objectId = (uint) Game.Scene3D.GameObjects.GetObjectId(closestObject);
                Game.NetworkMessageBuffer?.AddLocalOrder(Order.CreateSetSelection(playerId, objectId));
            }
        }

        private void MultiSelect()
        {
            var boxFrustum = GetSelectionFrustum(SelectionRect);
            var selectedObjectIds = new List<uint>();

            // TODO: Optimize with frustum culling?
            foreach (var gameObject in Game.Scene3D.GameObjects.Items)
            {
                if (!gameObject.IsSelectable || gameObject.Collider == null)
                {
                    continue;
                }

                if (gameObject.Collider.Intersects(boxFrustum))
                {
                    var objectId = (uint) Game.Scene3D.GameObjects.GetObjectId(gameObject);
                    selectedObjectIds.Add(objectId);
                }
            }

            var playerId = Game.Scene3D.GetPlayerIndex(Game.Scene3D.LocalPlayer);
            Game.NetworkMessageBuffer?.AddLocalOrder(Order.CreateClearSelection(playerId));

            if (selectedObjectIds.Count > 0)
            {
                Game.NetworkMessageBuffer?.AddLocalOrder(Order.CreateSetSelection(playerId, selectedObjectIds));
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
