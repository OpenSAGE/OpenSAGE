using System;
using System.Linq;
using System.Collections.Generic;
using System.Numerics;
using OpenSage.Gui;
using OpenSage.Logic.Object;
using OpenSage.Logic.Orders;
using OpenSage.Mathematics;
using OpenSage.Logic.OrderGenerators;

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

        public void OnHoverSelection(Point2D point)
        {
            Game.Scene3D.LocalPlayer.HoveredUnit = FindClosestObject(point.ToVector2());
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

            Game.Scene3D.LocalPlayer.HoveredUnit = null;
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

        public void SetSelectedObjects(Player player, GameObject[] objects, bool playAudio = true)
        {
            player.SelectUnits(objects);

            if (player == Game.Scene3D.LocalPlayer)
            {
                if (CanSetRallyPoint(objects))
                {
                    Game.OrderGenerator.ActiveGenerator = new RallyPointOrderGenerator();
                }
                else
                {
                    Game.OrderGenerator.ActiveGenerator = new UnitOrderGenerator(Game);
                }

                if (playAudio)
                {
                    //TODO: handle hordes properly
                    objects[0].OnLocalSelect(Game.Audio);
                }
            }
        }

        private bool CanSetRallyPoint(GameObject[] objects)
        {
            foreach (var unit in objects)
            {
                if (unit.Definition.KindOf.Get(ObjectKinds.AutoRallyPoint))
                {
                    return true;
                }
            }

            return false;
        }

        public void SetRallyPointForSelectedObjects(Player player, GameObject[] objects, Vector3 rallyPoint)
        {
            foreach (var obj in objects)
            {
                obj.RallyPoint = rallyPoint;
            }
        }

        public void ClearSelectedObjectsForLocalPlayer()
        {
            ClearSelectedObjects(Game.Scene3D.LocalPlayer);
        }

        public void ClearSelectedObjects(Player player)
        {
            player.DeselectUnits();
        }

        internal GameObject FindClosestObject(Vector2 point)
        {
            var ray = Game.Scene3D.Camera.ScreenPointToRay(point);

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

            return closestObject;
        }

        private void SingleSelect()
        {
            var closestObject = FindClosestObject(_startPoint.ToVector2());

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
            var selectedObjects = new List<uint>();

            uint? structure = null;

            // TODO: Optimize with frustum culling?
            foreach (var gameObject in Game.Scene3D.GameObjects.Items)
            {
                if (!gameObject.IsSelectable || gameObject.Collider == null)
                {
                    continue;
                }

                //only allow own objects to be drag selected
                if (gameObject.Owner != Game.Scene3D.LocalPlayer)
                {
                    continue;
                }

                if (gameObject.Collider.Intersects(boxFrustum))
                {
                    var objectId = (uint) Game.Scene3D.GameObjects.GetObjectId(gameObject);

                    if (gameObject.Definition.KindOf.Get(ObjectKinds.Structure) == false)
                    {
                        selectedObjects.Add(objectId);
                    }
                    else if (gameObject.Definition.KindOf.Get(ObjectKinds.Structure) == true)
                    {
                        structure ??= objectId;
                    }
                }
            }

            if (selectedObjects.Count == 0 && structure.HasValue) selectedObjects.Add(structure.Value);

            var playerId = Game.Scene3D.GetPlayerIndex(Game.Scene3D.LocalPlayer);
            Game.NetworkMessageBuffer?.AddLocalOrder(Order.CreateClearSelection(playerId));
            Game.NetworkMessageBuffer?.AddLocalOrder(Order.CreateSetSelection(playerId, selectedObjects));
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
