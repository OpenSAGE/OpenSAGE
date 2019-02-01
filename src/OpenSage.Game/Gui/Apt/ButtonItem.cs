using System.Collections.Generic;
using System.Numerics;
using OpenSage.Data.Apt.Characters;
using OpenSage.Gui.Apt.ActionScript;
using OpenSage.Mathematics;
using Veldrid;

namespace OpenSage.Gui.Apt
{
    internal class ButtonItem : DisplayItem
    {
        private bool _isHovered = false;
        private ItemTransform _curTransform;
        private List<InstructionCollection> _actionList;
        public Texture Texture { get; set; }   

        public override void Create(Character chararacter, AptContext context, SpriteItem parent = null)
        {
            Character = chararacter;
            Context = context;
            Parent = parent;
            Name = "";
            Visible = true;

            var button = Character as Button;

            _actionList = new List<InstructionCollection>();
        }

        public override bool HandleInput(Point2D mousePos, bool mouseDown)
        {
            var button = Character as Button;

            //var scaling = _outputSize / movieSize;
            //m_curTransform.GeometryRotation.M11 *= scaling.X;
            //m_curTransform.GeometryRotation.M22 *= scaling.Y;
            var transform = _curTransform.GeometryRotation;
            transform.Translation = _curTransform.GeometryTranslation;// * scaling;

            var verts = button.Vertices;
            var mouse = new Point2D(mousePos.X, mousePos.Y);

            foreach (var tri in button.Triangles)
            {
                var v1 = Vector2.Transform(verts[tri.IDX0], transform);
                var v2 = Vector2.Transform(verts[tri.IDX1], transform);
                var v3 = Vector2.Transform(verts[tri.IDX2], transform);

                if (TriangleUtility.IsPointInside(v1, v2, v3, mouse))
                {
                    if (!_isHovered)
                    {
                        var idx = button.Actions.FindIndex(ba => ba.Flags.HasFlag(ButtonActionFlags.IdleToOverUp));
                        if (idx != -1)
                        {
                            _actionList.Add(button.Actions[idx].Instructions);
                        }
                        _isHovered = true;
                        return true;
                    }
                }
            }

            if (_isHovered)
            {
                var idx = button.Actions.FindIndex(ba => ba.Flags.HasFlag(ButtonActionFlags.OverUpToIdle));
                if (idx != -1)
                {
                    _actionList.Add(button.Actions[idx].Instructions);
                }
                _isHovered = false;
            }
            return false;
        }

        public override void Render(AptRenderer renderer, ItemTransform pTransform, DrawingContext2D dc)
        {
            _curTransform = (ItemTransform) pTransform.Clone();
        }

        public override void RunActions(TimeInterval gt)
        {
            foreach (var action in _actionList)
            {
                Context.Avm.Execute(action, Parent.ScriptObject);
            }
            _actionList.Clear();
        }
    }
}
