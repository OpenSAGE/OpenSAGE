using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using OpenSage.Logic.Object;
using OpenSage.Mathematics;

namespace OpenSage.Logic
{
    public sealed class ActionSystem : GameSystem
    {
        public ActionSystem(Game game) : base(game)
        {
        }

        public void OnRequestAction(Point2D clickPoint)
        {
            /*var firstSelectedGameObject = Game.Selection.SelectedObjects.FirstOrDefault();
            if (firstSelectedGameObject != null && firstSelectedGameObject.IsBuilding)
            {
                //TODO implement
            }
            else if (firstSelectedGameObject != null && firstSelectedGameObject.IsMovable)
            {
                HandleActionForMovableObject(Game.Selection.SelectedObjects, clickPoint);
            }
            else
            {
                //TODO implement
            }*/
        }

        private void HandleActionForMovableObject(List<GameObject> selectedObjects, Point2D clickPoint)
        {
            //TODO z is not correct
            var targetPosition = Game.Scene3D.Camera.ScreenToWorldPoint(clickPoint);
            foreach (var selectedObject in selectedObjects)
            {
                selectedObject.Transform.Translation = targetPosition;
            }
        }
    }
}
