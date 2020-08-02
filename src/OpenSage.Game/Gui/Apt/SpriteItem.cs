using System;
using System.Collections.Generic;
using System.Numerics;
using OpenSage.Data.Apt.Characters;
using OpenSage.Data.Apt.FrameItems;
using OpenSage.Gui.Apt.ActionScript;
using OpenSage.Mathematics;
using Action = OpenSage.Data.Apt.FrameItems.Action;

namespace OpenSage.Gui.Apt
{
    public enum PlayState
    {
        PLAYING,
        STOPPED
    }

    public sealed class SpriteItem : DisplayItem
    {
        private Playable _sprite;
        private uint _currentFrame;
        private TimeInterval _lastUpdate;
        private PlayState _state;
        private Dictionary<string, uint> _frameLabels;
        public delegate void ColorDelegate(ColorRgbaF color);

        /// <summary>
        /// required, because actions are always executed at the end of each frame
        /// </summary>
        private List<Action> _actionList;

        public ColorDelegate SetBackgroundColor { get; set; }

        public DisplayList Content { get; private set; }

        public override void Create(Character character, AptContext context, SpriteItem parent = null)
        {
            _sprite = (Playable) character;
            _currentFrame = 0;
            _actionList = new List<Action>();
            _frameLabels = new Dictionary<string, uint>();
            _state = PlayState.PLAYING;

            Name = "";
            Visible = true;
            Character = _sprite;
            Context = context;
            Content = new DisplayList();
            Parent = parent;
            ScriptObject = new ObjectContext(this);

            // Fill the frameLabels in advance
            foreach (var frame in _sprite.Frames)
            {
                foreach (var item in frame.FrameItems)
                {
                    switch (item)
                    {
                        case FrameLabel fl:
                            _frameLabels[fl.Name] = fl.FrameId;
                            break;
                    }

                }
            }
        }

        public override void Render(AptRenderer renderer, ItemTransform pTransform, DrawingContext2D dc)
        {
            if (!Visible)
                return;

            //calculate the transform for this element
            var cTransform = pTransform * Transform;

            //render all subItems
            foreach (var item in Content.Items.Values)
            {
                item.Render(renderer, cTransform, dc);
            }
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

                _currentFrame++;

                //reset to the start, we are looping by default
                if (_currentFrame >= _sprite.Frames.Count)
                    _currentFrame = 0;
            }

            //update all subItems
            foreach (var item in Content.Items.Values)
            {
                item.Update(gt);
            }
        }

        public void Stop()
        {
            _state = PlayState.STOPPED;
        }

        public void Play()
        {
            _state = PlayState.PLAYING;
        }

        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public void Goto(string label)
        {
            logger.Info($"Goto: {label}");
            if (_frameLabels.ContainsKey(label))
            {
                _currentFrame = _frameLabels[label];
            }
            else
            {
                logger.Warn($"Missing framelabel: {label}");
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
            if (_lastUpdate.TotalTime.Milliseconds == 0)
            {
                _lastUpdate = gt;
                return true;
            }

            if (_state != PlayState.PLAYING)
                return false;

            if ((gt.TotalTime - _lastUpdate.TotalTime).Milliseconds >= Context.MillisecondsPerFrame)
            {
                _lastUpdate = gt;
                return true;
            }
            else
                return false;
        }

        private void HandleFrameItem(FrameItem item)
        {
            switch (item)
            {
                case PlaceObject po:
                    //place a new display item
                    if (po.Flags.HasFlag(PlaceObjectFlags.HasCharacter) &&
                       !po.Flags.HasFlag(PlaceObjectFlags.Move) && !po.Flags.HasFlag(PlaceObjectFlags.HasClipDepth))
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
                    _actionList.Add(action);
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
            Matrix3x2 geoRotate;
            Vector2 geoTranslate;
            if (po.Flags.HasFlag(PlaceObjectFlags.HasMatrix))
            {
                geoRotate = new Matrix3x2(po.RotScale.M11, po.RotScale.M12,
                                            po.RotScale.M21, po.RotScale.M22,
                                            0, 0);
                geoTranslate = new Vector2(po.Translation.X, po.Translation.Y);
            }
            else
            {
                geoRotate = Matrix3x2.Identity;
                geoTranslate = Vector2.Zero;
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

            return new ItemTransform(colorTransform, geoRotate, geoTranslate);
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
                cTransform.GeometryRotation = new Matrix3x2(po.RotScale.M11, po.RotScale.M12,
                                                        po.RotScale.M21, po.RotScale.M22,
                                                        0, 0);
                cTransform.GeometryTranslation = new Vector2(po.Translation.X, po.Translation.Y);
            }

            if (po.Flags.HasFlag(PlaceObjectFlags.HasColorTransform))
            {
                cTransform.ColorTransform = po.Color.ToColorRgbaF();
            }

            if (po.Flags.HasFlag(PlaceObjectFlags.HasName))
            {
                ScriptObject.Variables[po.Name] = Value.FromObject(displayItem.ScriptObject);
            }

            displayItem.Transform = cTransform;
        }

        private void PlaceItem(PlaceObject po)
        {
            if (Content.Items.ContainsKey(po.Depth))
            {
                return;
            }

            var character = Context.GetCharacter(po.Character, _sprite);
            var itemTransform = CreateTransform(po);

            DisplayItem displayItem;
            if (character is Playable)
                displayItem = new SpriteItem();
            else if (character is Button)
                displayItem = new ButtonItem();
            else
                displayItem = new RenderItem();

            displayItem.Transform = itemTransform;
            displayItem.Create(character, Context, this);

            //add this object as an AS property
            if (po.Flags.HasFlag(PlaceObjectFlags.HasName))
            {
                ScriptObject.Variables[po.Name] = Value.FromObject(displayItem.ScriptObject);
                displayItem.Name = po.Name;
            }

            if (po.Flags.HasFlag(PlaceObjectFlags.HasClipAction))
            {
                if (po.ClipEvents != null)
                {
                    foreach (var clipEvent in po.ClipEvents)
                    {
                        if (clipEvent.Flags.HasFlag(ClipEventFlags.Initialize))
                        {
                            Context.Avm.Execute(clipEvent.Instructions, displayItem.ScriptObject,
                                                Character.Container.Constants.Entries);
                        }
                    }
                }
            }

            Content.AddItem(po.Depth, displayItem);
        }

        public override void RunActions(TimeInterval gt)
        {
            //execute all actions now
            foreach (var action in _actionList)
            {
                Context.Avm.Execute(action.Instructions, ScriptObject,
                        ScriptObject.Item.Character.Container.Constants.Entries);
            }
            _actionList.Clear();

            //execute all subitems actions now
            //update all subitems
            foreach (var item in Content.Items.Values)
            {
                item.RunActions(gt);
            }
        }

        public override bool HandleInput(Point2D mousePos, bool mouseDown)
        {
            foreach (var item in Content.ReverseItems.Values)
            {
                if (item.HandleInput(mousePos, mouseDown))
                {
                    return true;
                }
            }

            return false;
        }

        public DisplayItem GetNamedItem(string name)
        {
            return ScriptObject.Variables[name].ToObject().Item;
        }
    }
}
