using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using OpenSage.Data.Apt.Characters;
using OpenSage.Data.Apt.FrameItems;
using OpenSage.Gui.Apt.ActionScript;
using OpenSage.Mathematics;

namespace OpenSage.Gui.Apt
{
    public enum PlayState
    {
        PLAYING,
        STOPPED
    }

    public sealed class SpriteItem : IDisplayItem
    {
        private SpriteItem _parent;
        private DisplayList _content;
        private Playable _sprite;
        private uint _currentFrame;
        private AptContext _context;
        private GameTime _lastUpdate;
        private ObjectContext _scriptObject;
        private PlayState _state;
        private Dictionary<string, uint> _frameLabels;
        public string Name { get; set; }

        /// <summary>
        /// required, because actions are always executed at the end of each frame
        /// </summary>
        private List<Action> _actionList;

        public SpriteItem Parent => _parent;
        public Character Character => _sprite;
        public AptContext Context => _context;
        public ItemTransform Transform { get; set; }
        public ObjectContext ScriptObject => _scriptObject;

        public void Create(Character chararacter, AptContext context, SpriteItem parent = null)
        {
            _sprite = (Playable) chararacter;
            _context = context;
            _content = new DisplayList();
            _parent = parent;
            _currentFrame = 0;
            _scriptObject = new ObjectContext(this);
            _actionList = new List<Action>();
            _frameLabels = new Dictionary<string, uint>();
            _state = PlayState.PLAYING;

            //fill the frameLabels in advance
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

        public void Render(ItemTransform pTransform,DrawingContext2D dc)
        {
            //calculate the transform for this element
            var cTransform = pTransform * Transform;

            //render all subitems
            foreach (var item in _content.Items.Values)
            {
                item.Render(cTransform, dc);
            }
        }

        public void Update(GameTime gt)
        {
            if (IsNewFrame(gt))
            {
                //get the current frame
                var frame = _sprite.Frames[(int) _currentFrame];

                //process all frame items
                foreach (var item in frame.FrameItems)
                {
                    HandleFrameItem(item);
                }
                           
                _currentFrame++;

                //reset to the start, we are looping by default
                if (_currentFrame >= _sprite.Frames.Count)
                    _currentFrame = 0;
            }

            //update all subitems
            foreach (var item in _content.Items.Values)
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

        public void Goto(string label)
        {
            Debug.WriteLine("[INFO] Goto: "+ label);
            _currentFrame = _frameLabels[label];
        }

        public void GotoFrame(int frame)
        {
            _currentFrame = (uint)frame;
        }

        private bool IsNewFrame(GameTime gt)
        {
            if (_lastUpdate.TotalGameTime.Milliseconds == 0)
            {
                _lastUpdate = gt;
                return true;
            }

            if (_state != PlayState.PLAYING)
                return false;

            if ((gt.TotalGameTime - _lastUpdate.TotalGameTime).Milliseconds >= _context.MillisecondsPerFrame)
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
                    _content.Items.Remove(ro.Depth);
                    break;
                case Action action:
                    _actionList.Add(action);
                    break;
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
                                            0,0);
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

            return new ItemTransform(colorTransform, geoRotate,geoTranslate);
        }

        private void MoveItem(PlaceObject po)
        {
            var displayItem = _content.Items[po.Depth];
            var cTransform = displayItem.Transform;

            if (po.Flags.HasFlag(PlaceObjectFlags.HasMatrix))
            {
                cTransform.GeometryRotation = new Matrix3x2(po.RotScale.M11, po.RotScale.M12,
                                                        po.RotScale.M21, po.RotScale.M22,
                                                        0,0);
                cTransform.GeometryTranslation = new Vector2(po.Translation.X, po.Translation.Y);
            }

            if (po.Flags.HasFlag(PlaceObjectFlags.HasColorTransform))
            {
                cTransform.ColorTransform = po.Color.ToColorRgbaF();
            }

            if (po.Flags.HasFlag(PlaceObjectFlags.HasName))
            {
                _scriptObject.Variables[po.Name] = Value.FromObject(displayItem.ScriptObject);
            }

            displayItem.Transform = cTransform;
        }

        private void PlaceItem(PlaceObject po)
        {
            var character = _context.GetCharacter(po.Character, _sprite);
            var itemTransform = CreateTransform(po);

            IDisplayItem displayItem;
            if (character is Playable)
                displayItem = new SpriteItem() { Transform = itemTransform };
            else
                displayItem = new RenderItem() { Transform = itemTransform };

            displayItem.Create(character, _context, this);

            //add this object as an AS property
            if (po.Flags.HasFlag(PlaceObjectFlags.HasName))
            {
                _scriptObject.Variables[po.Name] = Value.FromObject(displayItem.ScriptObject);
                displayItem.Name = po.Name;
            }

            _content.Items[po.Depth] = displayItem;
        }

        public void RunActions(GameTime gt)
        {
            //execute all actions now
            foreach (var action in _actionList)
            {
                _context.AVM.Execute(action.Instructions, _scriptObject);
            }
            _actionList.Clear();

            //execute all subitems actions now
            //update all subitems
            foreach (var item in _content.Items.Values)
            {
                item.RunActions(gt);
            }
        }
    }
}
