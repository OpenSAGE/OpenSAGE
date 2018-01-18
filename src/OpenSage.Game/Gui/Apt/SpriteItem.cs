using System.Numerics;
using OpenSage.Data.Apt.Characters;
using OpenSage.Data.Apt.FrameItems;
using OpenSage.LowLevel.Graphics2D;
using OpenSage.LowLevel.Graphics3D;

namespace OpenSage.Gui.Apt
{
    public sealed class SpriteItem : IDisplayItem
    {
        private SpriteItem _parent;
        private DisplayList _content;
        private Playable _sprite;
        private int _currentFrame;
        private AptContext _context;
        private GameTime _lastUpdate;

        public SpriteItem Parent => _parent;
        public Character Character => _sprite;
        public AptContext Context => _context;
        public ItemTransform Transform { get; set; }

        public void Create(Character chararacter, AptContext context, SpriteItem parent = null)
        {
            _sprite = (Playable) chararacter;
            _context = context;
            _content = new DisplayList();
            _parent = parent;
            _currentFrame = 0;
        }

        public void Update(ItemTransform pTransform, GameTime gt, DrawingContext dc)
        {
            //get the current frame
            var frame = _sprite.Frames[_currentFrame];

            //process all frame items
            foreach (var item in frame.FrameItems)
            {
                HandleFrameItem(item);
            }

            //calculate the transform for this element
            var cTransform = pTransform * Transform;

            //update all subitems
            foreach (var item in _content.Items.Values)
            {
                item.Update(cTransform, gt, dc);
            }

            //check if we are going to the next frame
            if (IsNewFrame(gt))
                _currentFrame++;

            //reset to the start, we are looping by default
            if (_currentFrame >= _sprite.Frames.Count)
                _currentFrame = 0;
        }

        private bool IsNewFrame(GameTime gt)
        {
            if ((gt.ElapsedGameTime - _lastUpdate.ElapsedGameTime).Milliseconds > _context.MillisecondsPerFrame)
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
            var displayItem = _content.Items[po.Depth];
            var cTransform = displayItem.Transform;

            if (po.Flags.HasFlag(PlaceObjectFlags.HasMatrix))
            {
                cTransform.GeometryTransform = new Matrix3x2(po.RotScale.M11, po.RotScale.M12,
                                                        po.RotScale.M21, po.RotScale.M22,
                                                        po.Translation.X, po.Translation.Y);
            }

            if (po.Flags.HasFlag(PlaceObjectFlags.HasColorTransform))
            {
                cTransform.ColorTransform = po.Color.ToColorRgbaF();
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
            _content.Items[po.Depth] = displayItem;
        }
    }
}
