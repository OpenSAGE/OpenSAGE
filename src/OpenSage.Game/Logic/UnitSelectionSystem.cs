using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using OpenSage.Gui;
using OpenSage.Logic.Object;
using OpenSage.Mathematics;

namespace OpenSage.Logic
{
    public sealed class UnitSelectionSystem : GameSystem
    {
        private readonly SelectionGui _selectionGui;
        private List<GameObject> _selectedObjects;

        public UnitSelectionSystem(Game game) : base(game)
        {
            _selectionGui = game.Scene2D.SelectionGui;
        }

        public bool Selecting
        {
            get => _selectionGui.Selecting;
            set => _selectionGui.Selecting = value;
        }

        public void UpdateSelectionUi(Rectangle rect)
        {
            _selectionGui.SelectionRect = rect;
        }

        public void SelectObjectsInRectangle(Rectangle rect)
        {
            _selectedObjects = new List<GameObject>();

            _selectionGui.DebugOverlays.Clear();

            var boxFrustum = GetSelectionFrustum(rect);

            // TODO: Optimize with a quadtree / use frustum culling?
            foreach (var gameObject in Game.Scene3D.GameObjects.Items)
            {
                if (gameObject.Collider is BoxCollider box)
                {
                    var worldBox = box.Bounds.Transform(gameObject.Transform.Matrix);

                    if (boxFrustum.Contains(worldBox) == ContainmentType.Contains)
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
        }

        // Based on
        // https://ghoscher.com/2010/12/09/xna-picking-tutorial-part-ii/
        private BoundingFrustum GetSelectionFrustum(Rectangle rect)
        {
            var viewport = Game.Viewport;
            var viewportSize = new Vector2(viewport.Width, viewport.Height);
 
            var rectSize = new Vector2(rect.Width, rect.Height);
            var rectSizeHalf = (rectSize / 2f);
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
