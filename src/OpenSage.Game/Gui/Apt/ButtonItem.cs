using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using OpenSage.Data.Apt.Characters;
using OpenSage.Gui.Apt.ActionScript;
using OpenSage.Mathematics;
using Veldrid;

namespace OpenSage.Gui.Apt
{
    internal class ButtonItem : IDisplayItem
    {
        private bool _isHovered = false;
        private ItemTransform _curTransform;
        private List<InstructionCollection> _actionList;

        public SpriteItem Parent { get; private set; }
        public Character Character { get; private set; }
        public AptContext Context { get; private set; }
        public ItemTransform Transform { get; set; }
        public Texture Texture { get; set; }
        public string Name { get; set; }
        public bool Visible { get; set; }

        public ObjectContext ScriptObject => null;

        public void Create(Character chararacter, AptContext context, SpriteItem parent = null)
        {
            Character = chararacter;
            Context = context;
            Parent = parent;
            Name = "";
            Visible = true;

            var button = Character as Button;

            context.Window.InputHandler.MouseMoved += MouseMoved;

            _actionList = new List<InstructionCollection>();
        }

        private bool MouseMoved(object sender, EventArgs e, int x, int y)
        {
            var button = Character as Button;

            //var scaling = _outputSize / movieSize;
            //m_curTransform.GeometryRotation.M11 *= scaling.X;
            //m_curTransform.GeometryRotation.M22 *= scaling.Y;
            var transform = _curTransform.GeometryRotation;
            transform.Translation = _curTransform.GeometryTranslation;// * scaling;

            var verts = button.Vertices;
            var mouse = new Point2D(x, y);

            foreach (var tri in button.Triangles)
            {
                var v1 = Vector2.Transform(verts[tri.IDX0], transform);
                var v2 = Vector2.Transform(verts[tri.IDX1], transform);
                var v3 = Vector2.Transform(verts[tri.IDX2], transform);

                if (TriangleUtility.IsPointInside(v1, v2, v3, mouse))
                {
                    if (!_isHovered)
                    {
                        try
                        {
                            var ev = button.Actions.First(ba => ba.Flags.HasFlag(ButtonActionFlags.IdleToOverUp));
                            _actionList.Add(ev.Instructions);
                        }
                        catch (Exception exc)
                        {

                        }
                        _isHovered = true;
                    }

                    return true;
                }
            }

            if(_isHovered)
            {
                try
                {
                    var ev = button.Actions.First(ba => ba.Flags.HasFlag(ButtonActionFlags.OverUpToIdle));
                    _actionList.Add(ev.Instructions);
                }
                catch (Exception exc)
                {

                }
                _isHovered = false;
            }
            return false;
        }

        public void Render(AptRenderer renderer, ItemTransform pTransform, DrawingContext2D dc)
        {
            _curTransform = (ItemTransform)pTransform.Clone();
        }

        public void RunActions(GameTime gt)
        {
            foreach (var action in _actionList)
            {
                Context.Avm.Execute(action, Parent.ScriptObject);
            }
            _actionList.Clear();
        }

        public void Update(GameTime gt)
        {
        }
    }
}
