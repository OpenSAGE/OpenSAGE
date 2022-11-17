using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        // TODO: consider allowing configuration for accessibility
        // time after which releasing right click will not result in deselecting units, in ms
        private const int DeselectMaxTimeMs = 250;

        // TODO: scale with resolution, allow configuration for accessibility
        // if the cursor moves further than this, don't deselect no matter how short right click was held for
        private const int DeselectMaxDelta = 5;

        private SelectionGui SelectionGui => Game.Scene3D.SelectionGui;

        private Point2D _selectionStartPoint;
        private Point2D _selectionEndPoint;

        private Point2D _panStartPoint;
        private TimeSpan _panStartTime;

        public SelectionStatus Status { get; private set; } = SelectionStatus.NotSelecting;
        public bool Selecting => Status != SelectionStatus.NotSelecting;
        public bool Panning { get; private set; }

        private Rectangle SelectionRect
        {
            get
            {
                var topLeft = Point2D.Min(_selectionStartPoint, _selectionEndPoint);
                var bottomRight = Point2D.Max(_selectionStartPoint, _selectionEndPoint);

                return new Rectangle(topLeft,
                    new Size(bottomRight.X - topLeft.X, bottomRight.Y - topLeft.Y));
            }
        }

        public SelectionSystem(Game game) : base(game) { }

        public void OnStartRightClickDrag(Point2D point)
        {
            Panning = true;
            _panStartPoint = point;
            _panStartTime = Game.MapTime.TotalTime;
        }

        public void OnEndRightClickDrag(Point2D point2D)
        {
            var selectionDelta = point2D - _panStartPoint;
            var time = Game.MapTime.TotalTime - _panStartTime;
            if (time.Milliseconds < DeselectMaxTimeMs &&
                Math.Abs(selectionDelta.X) < DeselectMaxDelta && Math.Abs(selectionDelta.Y) < DeselectMaxDelta)
            {
                ClearSelectedObjectsForLocalPlayer();
            }

            Panning = false;
        }

        public void OnHoverSelection(Point2D point)
        {
            // We might not have a local player. E.g. shellmap/replay
            if (Game.Scene3D.LocalPlayer == null)
            {
                return;
            }

            Game.Scene3D.LocalPlayer.HoveredUnit = FindClosestObject(point.ToVector2());
        }

        public void OnStartDragSelection(Point2D startPoint)
        {
            Status = SelectionStatus.SingleSelecting;
            _selectionStartPoint = startPoint;
            _selectionEndPoint = startPoint;
            SelectionGui.SelectionRectangle = SelectionRect;
        }

        public void OnDragSelection(Point2D point)
        {
            _selectionEndPoint = point;

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
                    Game.OrderGenerator.ActiveGenerator = Game.Definition.CreateNewOrderGenerator(Game);
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

        public GameObject FindClosestObject(Vector2 point)
        {
            var ray = Game.Scene3D.Camera.ScreenPointToRay(point);

            var closestDepth = float.MaxValue;
            GameObject closestObject = null;

            foreach (var gameObject in Game.Scene3D.GameObjects.Items)
            {
                if (!gameObject.IsSelectable ||
                    gameObject.RoughCollider == null ||
                    !gameObject.RoughCollider.Intersects(ray, out _))
                {
                    continue;
                }

                foreach (var collider in gameObject.Colliders)
                {
                    if (!collider.Intersects(ray, out var depth))
                    {
                        continue;
                    }
                    if (closestObject != null && !gameObject.IsStructure && closestObject.IsStructure ||
                        depth < closestDepth)
                    {
                        closestDepth = depth;
                        closestObject = gameObject;
                    }
                }
            }

            return closestObject;
        }

        private void SingleSelect()
        {
            var closestObject = FindClosestObject(_selectionStartPoint.ToVector2());

            var playerId = Game.Scene3D.GetPlayerIndex(Game.Scene3D.LocalPlayer);
            Game.NetworkMessageBuffer?.AddLocalOrder(Order.CreateClearSelection(playerId));

            if (closestObject != null)
            {
                Game.NetworkMessageBuffer?.AddLocalOrder(Order.CreateSetSelection(playerId, closestObject.ID));
            }
        }

        private void MultiSelect()
        {
            var boxFrustum = GetSelectionFrustum(SelectionRect);
            var selectedObjects = new List<uint>();

            uint? structure = null;

            // TODO: Optimize with quadtree
            foreach (var gameObject in Game.Scene3D.GameObjects.Items)
            {
                if (!gameObject.IsSelectable || gameObject.RoughCollider == null)
                {
                    continue;
                }

                //only allow own objects to be drag selected
                if (gameObject.Owner != Game.Scene3D.LocalPlayer)
                {
                    continue;
                }

                if (gameObject.RoughCollider.Intersects(boxFrustum))
                {
                    if (gameObject.Definition.KindOf.Get(ObjectKinds.Structure) == false)
                    {
                        selectedObjects.Add(gameObject.ID);
                    }
                    else if (gameObject.Definition.KindOf.Get(ObjectKinds.Structure) == true)
                    {
                        structure ??= gameObject.ID;
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
