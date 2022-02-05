using System.Collections.Generic;
using System.Numerics;
using OpenSage.FileFormats.Apt.Characters;
using OpenSage.FileFormats.Apt.FrameItems;
using OpenAS2.FlashDom.Script;
using OpenSage.Mathematics;

namespace OpenAS2.FlashDom.HostObjects
{
    public class ButtonItem : TexturedItem
    {
        private Button _button;

        private bool _isHovered = false;
        private bool _isDown = false;
        private ItemTransform _curTransform;
        private List<(ButtonActionFlags, InstructionCollection)> _instsList;
        private List<InstructionCollection> _actionList;

        public override void Create(Character chararacter, AptContext context, SpriteItem parent = null)
        {
            Character = chararacter;
            Context = context;
            Parent = parent;
            Name = "";
            Visible = true;

            _button = Character as Button;
            _instsList = new();
            _actionList = new();
            foreach (var insts in _button.Actions)
                _instsList.Add((insts.Flags, InstructionCollection.Parse(insts.Instructions)));
        }


        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public override DisplayItem GetMouseFocus(Vector2 mousePos)
        {
            var verts = _button.Vertices;
            foreach (var tri in _button.Triangles)
            {
                if (TriangleUtility.IsPointInside(verts[tri.IDX0], verts[tri.IDX0], verts[tri.IDX0], mousePos))
                    return this;
            }
            return null;
        }

        public override bool HandleEvent(ClipEventFlags flags)
        {
            if (flags.HasFlag(ClipEventFlags.RollOver))
                return HandleLocalEvent(ButtonActionFlags.IdleToOverUp);
            if (flags.HasFlag(ClipEventFlags.Press))
                return HandleLocalEvent(ButtonActionFlags.OverUpToOverDown);
            if (flags.HasFlag(ClipEventFlags.Release))
                return HandleLocalEvent(ButtonActionFlags.OverDownToOverUp);
            if (flags.HasFlag(ClipEventFlags.RollOut))
                return HandleLocalEvent(ButtonActionFlags.OverUpToIdle);
            return false;
        }

        public bool HandleLocalEvent(ButtonActionFlags flags)
        {
            var idx = _instsList.FindIndex(ba => ba.Item1.HasFlag(flags));
            if (idx != -1)
            {
                _actionList.Add(_instsList[idx].Item2);
                return true;
            }
            return false;
        }
        public override void EnqueueActions(TimeInterval gt)
        {
            // enqueue all actions
            foreach (var action in _actionList)
                Context.Avm.EnqueueContext(this, action, $"ButtonAction: \"{Name}\"");
            _actionList.Clear();
        }
        public override bool HandleInput(Point2D mousePos, bool mouseDown)
        {

            var transform = _curTransform.GeometryTransform;

            ApplyCurrentRecord(ref transform);

            var verts = _button.Vertices;
            var mouse = new Point2D(mousePos.X, mousePos.Y);

            foreach (var tri in _button.Triangles)
            {
                var v1 = Vector2.Transform(verts[tri.IDX0], transform);
                var v2 = Vector2.Transform(verts[tri.IDX1], transform);
                var v3 = Vector2.Transform(verts[tri.IDX2], transform);

                if (TriangleUtility.IsPointInside(v1, v2, v3, mouse))
                {
                    if (!_isHovered)
                    {
                        logger.Debug("Hit: " + mousePos.X + "-" + mousePos.Y);
                        if (HandleLocalEvent(ButtonActionFlags.IdleToOverUp))
                        {
                            return true;
                        }
                        _isHovered = true;
                    }

                    if (_isHovered && mouseDown && !_isDown)
                    {
                        logger.Debug("Down: " + mousePos.X + "-" + mousePos.Y);
                        if (HandleLocalEvent(ButtonActionFlags.OverUpToOverDown))
                        {
                            // _actionList.Add(_button.Actions[idx].Instructions);
                        }
                        _isDown = true;
                    }

                    if (_isHovered && !mouseDown && _isDown)
                    {
                        logger.Debug("Up: " + mousePos.X + "-" + mousePos.Y);
                        if (HandleLocalEvent(ButtonActionFlags.OverDownToOverUp))
                        {
                            // _actionList.Add(_button.Actions[idx].Instructions);
                        }
                        _isDown = false;
                    }

                    return true;
                }
            }

            if (_isHovered)
            {
                if (HandleLocalEvent(ButtonActionFlags.OverUpToIdle))
                {
                    // _actionList.Add(_button.Actions[idx].Instructions);
                }
                _isHovered = false;
                logger.Debug("Unhovered: " + mousePos.X + "-" + mousePos.Y);
            }
            return false;
        }

        private void ApplyCurrentRecord(ref Matrix3x2 t)
        {
            var button = _button;
            var idx = button.Records.FindIndex(br => br.Flags.HasFlag(ButtonRecordFlags.StateHit));
            if (idx != -1)
            {
                var br = button.Records[idx];

                var a = new Matrix3x2(t.M11, t.M12, t.M21, t.M22, t.M31, t.M32);
                var b = new Matrix3x2(br.RotScale.M11, br.RotScale.M12, br.RotScale.M21, br.RotScale.M22, 0, 0);
                var c = Matrix3x2.Multiply(a, b);

                t.M11 = c.M11;
                t.M12 = c.M12;
                t.M21 = c.M21;
                t.M22 = c.M22;

                t.M31 += br.Translation.X;
                t.M32 += br.Translation.Y;
            }
        }

        protected override void RenderImpl(AptRenderingContext renderingContext)
        {
            _curTransform = renderingContext.CurrentTransform * Transform;

            var windowScaling = renderingContext.Window.GetScaling();
            _curTransform.GeometryTransform *= Matrix3x2.CreateScale(windowScaling);
        }

    }
}
