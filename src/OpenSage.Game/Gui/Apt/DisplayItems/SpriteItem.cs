using System;
using System.Collections.Generic;
using System.Numerics;
using OpenSage.FileFormats.Apt;
using OpenSage.FileFormats.Apt.Characters;
using OpenSage.FileFormats.Apt.FrameItems;
using OpenSage.Graphics;
using OpenSage.Gui.Apt.Script;
using OpenSage.Mathematics;
using Veldrid;
using Action = OpenSage.FileFormats.Apt.FrameItems.Action;

namespace OpenSage.Gui.Apt
{
    public enum PlayState
    {
        PLAYING,
        PENDING_STOPPED,
        STOPPED
    }

    public sealed class SpriteItem : DisplayItem
    {
        private Playable _sprite;
        private uint _currentFrame;
        private TimeInterval _lastUpdate;

        public delegate void ColorDelegate(ColorRgbaF color);

        /// <summary>
        /// required, because actions are always executed at the end of each frame
        /// </summary>
        private List<InstructionStorage> _actionList;

        public ColorDelegate SetBackgroundColor { get; set; }

        public DisplayList Content { get; private set; }

        public int CurrentFrame => (int) _currentFrame;

        public Dictionary<string, uint> FrameLabels { get; private set; }
        public PlayState State { get; private set; }

        public override void CreateFrom(Character character, AptContext context, SpriteItem parent = null)
        {
            _sprite = (Playable) character;
            _currentFrame = 0;
            _actionList = new();
            Content?.Dispose();
            RemoveToDispose(Content);
            FrameLabels = new();
            State = PlayState.PLAYING;

            Name = "";
            Visible = true;
            Character = _sprite;
            Context = context;
            Content = AddDisposable(new DisplayList());
            Parent = parent;
            ScriptObject = new MovieClip(this);

            // Fill the frameLabels in advance
            foreach (var frame in _sprite.Frames)
            {
                foreach (var item in frame.FrameItems)
                {
                    switch (item)
                    {
                        case FrameLabel fl:
                            FrameLabels[fl.Name] = fl.FrameId;
                            break;
                    }
                }
            }
        }

        protected override void RenderImpl(AptRenderingContext renderingContext)
        {
            if (!Visible)
                return;

            //calculate the transform for this element
            renderingContext.PushTransform(Transform);

            var clipMask = (Texture) null;
            var clipDepth = 0;

            //render all subItems
            foreach (var (depth, item) in Content.Items)
            {
                item.Render(renderingContext);

                if (depth > clipDepth && clipMask != null)
                {
                    renderingContext.SetClipMask(null);
                    clipDepth = 0;
                }

                if (item.ClipDepth != null)
                {
                    renderingContext.SetClipMask(item.ClipMask);
                    clipDepth = item.ClipDepth.Value;
                }
            }

            // In case the clipMask wans't cleared inside the loop
            if (clipDepth > 0)
            {
                renderingContext.SetClipMask(null);
            }
            renderingContext.PopTransform();
        }

        public override void Update(TimeInterval gt)
        {
            if (IsNewFrame(gt))
            {
                //get the current frame
                var frame = _sprite.Frames[(int) _currentFrame];

                //process all frame items
                foreach (var item in frame.FrameItems)
                {
                    if (!(item is FrameLabel))
                    {
                        HandleFrameItem(item);
                    }
                }

                if (State == PlayState.PLAYING)
                {
                    NextFrame();

                    //reset to the start, we are looping by default
                    if (_currentFrame >= _sprite.Frames.Count)
                        _currentFrame = 0;
                }
                else if (State == PlayState.PENDING_STOPPED)
                {
                    State = PlayState.STOPPED;
                }
            }

            //update all subItems
            foreach (var item in Content.Items.Values)
            {
                item.Update(gt);
            }
        }

        public void Stop(bool pending = false)
        {
            State = pending ? PlayState.PENDING_STOPPED : PlayState.STOPPED;
        }

        public void Play()
        {
            State = PlayState.PLAYING;
        }

        private static NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public void Goto(string label)
        {
            Logger.Info($"Goto: {label}");
            if (FrameLabels.ContainsKey(label))
            {
                _currentFrame = FrameLabels[label];
            }
            else
            {
                Logger.Warn($"Missing framelabel: {label}");
            }
        }

        public void GotoFrame(int frame)
        {
            if (frame < 1)
            {
                frame = 0;
            }
            else if (frame >= _sprite.Frames.Count)
            {
                frame = _sprite.Frames.Count - 1;
            }

            _currentFrame = (uint) frame;
        }

        public void NextFrame()
        {
            _currentFrame++;
        }

        private bool IsNewFrame(TimeInterval gt)
        {
            if (_lastUpdate.TotalTime.TotalMilliseconds == 0)
            {
                _lastUpdate = gt;
                return true;
            }

            if (State == PlayState.STOPPED)
                return false;

            if ((gt.TotalTime - _lastUpdate.TotalTime).TotalMilliseconds >= Context.MsPerFrame)
            {
                _lastUpdate = gt;
                return true;
            }
            else
                return false;
        }

        public void HandleFrameItem(FrameItem item)
        {
            switch (item)
            {
                case PlaceObject po:
                    //place a new display item
                    if (po.Flags.HasFlag(PlaceObjectFlags.HasCharacter) &&
                       !po.Flags.HasFlag(PlaceObjectFlags.Move))
                    {
                        PlaceItem(po);
                    }
                    //modify an existing display item
                    else if (!po.Flags.HasFlag(PlaceObjectFlags.HasCharacter) &&
                              po.Flags.HasFlag(PlaceObjectFlags.Move))
                    {
                        MoveItem(po);
                    }
                    //this will erase an existing item and place a new item right away
                    else if (po.Flags.HasFlag(PlaceObjectFlags.HasCharacter) &&
                             po.Flags.HasFlag(PlaceObjectFlags.Move))
                    {
                        PlaceItem(po);
                    }
                    break;
                case RemoveObject ro:
                    Content.RemoveItem(ro.Depth);
                    break;
                case Action action:
                    var a = action.Instructions;
                    _actionList.Add(a);
                    break;
                case InitAction iaction:
                    // executed in importing
                    break;
                case BackgroundColor bg:
                    if (SetBackgroundColor != null)
                    {
                        SetBackgroundColor(bg.Color.ToColorRgbaF());
                    }
                    else
                    {
                        //throw new InvalidOperationException("BackgroundColor can only be set from root!");
                    }
                    break;
                default:
                    throw new NotImplementedException("Unimplemented frameitem");
            }
        }

        private ItemTransform CreateTransform(PlaceObject po)
        {
            Matrix3x2 geoTransform;
            if (po.Flags.HasFlag(PlaceObjectFlags.HasMatrix))
            {
                geoTransform = new Matrix3x2(po.RotScale.M11, po.RotScale.M12,
                                            po.RotScale.M21, po.RotScale.M22,
                                            po.Translation.X, po.Translation.Y);
            }
            else
            {
                geoTransform = Matrix3x2.Identity;
            }

            ColorRgbaF colorTransform;
            if (po.Flags.HasFlag(PlaceObjectFlags.HasColorTransform))
            {
                colorTransform = po.Color.ToColorRgbaF();
            }
            else
            {
                colorTransform = ColorRgbaF.White;
            }

            return new ItemTransform(colorTransform, geoTransform);
        }

        private void MoveItem(PlaceObject po)
        {
            if (!Content.Items.ContainsKey(po.Depth))
            {
                //TODO WARN
                return;
            }

            var displayItem = Content.Items[po.Depth];
            var cTransform = displayItem.Transform;

            if (po.Flags.HasFlag(PlaceObjectFlags.HasMatrix))
            {
                cTransform.GeometryTransform = new Matrix3x2(po.RotScale.M11, po.RotScale.M12,
                                                        po.RotScale.M21, po.RotScale.M22,
                                                        po.Translation.X, po.Translation.Y);
                // cTransform.GeometryTranslation = new Vector2();
            }

            if (po.Flags.HasFlag(PlaceObjectFlags.HasColorTransform))
            {
                cTransform.ColorTransform = po.Color.ToColorRgbaF();
            }

            if (po.Flags.HasFlag(PlaceObjectFlags.HasName))
            {
                ScriptObject.SetMember(po.Name, Value.FromObject(displayItem.ScriptObject));
            }

            displayItem.Transform = cTransform;
        }

        private void PlaceItem(PlaceObject po)
        {
            if (Content.Items.ContainsKey(po.Depth))
            {
                return;
            }
            var itemTransform = CreateTransform(po);
            var displayItem = Context.GetInstantiatedCharacter(po.Character, itemTransform, this);

            //add this object as an AS property
            if (po.Flags.HasFlag(PlaceObjectFlags.HasName))
            {
                ScriptObject.SetMember(po.Name, Value.FromObject(displayItem.ScriptObject));
                displayItem.Name = po.Name;
            }

            if (po.Flags.HasFlag(PlaceObjectFlags.HasClipAction) && po.ClipEvents != null)
            {
                displayItem.RegisterClipEvents(po.ClipEvents);
                displayItem.ClipEventDefinedContext = Context;
                displayItem.CallClipEventLocal(ClipEventFlags.Initialize);
                displayItem.CallClipEventLocal(ClipEventFlags.Load);
                displayItem.CallClipEventLocal(ClipEventFlags.Construct); // TODO can't tell the difference
            }

            if(po.Flags.HasFlag(PlaceObjectFlags.HasClipDepth))
            {
                displayItem.ClipDepth = po.ClipDepth;
                displayItem.ClipMask = new RenderTarget(Context.Window.ContentManager.GraphicsDevice);
            }

            Content.AddItem(po.Depth, displayItem);
        }


        public override void EnqueueActions(TimeInterval gt)
        {
            // enqueue all actions
            foreach (var action in _actionList)
                Context.VM.EnqueueContext(this, action, $"FrameAction: \"{Name}\"");
            _actionList.Clear();

            // enqueue all subitems actions
            // subitems should be already updated by AptWindow
            foreach (var item in Content.Items.Values)
                item.EnqueueActions(gt);
        }

        public override DisplayItem GetMouseFocus(Vector2 mousePos)
        {
            var ans = (DisplayItem) null;
            foreach (var item in Content.ReverseItems.Values)
            {
                if (Matrix3x2.Invert(item.Transform.GeometryTransform, out var mat_inv))
                {
                    var mp_new = Vector2.Transform(mousePos, mat_inv);
                    var ans_item = item.GetMouseFocus(mp_new);
                    if (ans_item != null && (
                        item is SpriteItem ||
                        item is ButtonItem ||
                        (item is RenderItem && item.Character is Text) // TODO not sure if correct, trying to retrieve textbox
                        ))
                        ans = ans_item;
                    else if (ans_item != null)
                        ans = ans_item;
                    // ans = this;
                    if (ans != null)
                        break;
                }
            }
            return ans;
        }

        public override bool HandleEvent(ClipEventFlags flags)
        {
            CallClipEvent(flags);
            return true;
        }

        public void HandleEventOnTree(ClipEventFlags flags)
        {
            foreach (var item in Content.ReverseItems.Values)
            {
                item.HandleEvent(flags);
                if (item is SpriteItem si)
                    si.HandleEventOnTree(flags);
            }
        }

        public override bool HandleInput(Point2D mousePos, bool mouseDown)
        {
            // Logger.Info($"MouseInput {mousePos}, {mouseDown}");\
            var ans = true;

            foreach (var item in Content.ReverseItems.Values)
            {
                if (!item.HandleInput(mousePos, mouseDown))
                {
                    ans = false;
                }
            }

            return ans;
        }

        public DisplayItem GetNamedItem(string name)
        {
            return ScriptObject.GetMember(name).ToObject<StageObject>().Item;
        }
    }
}
