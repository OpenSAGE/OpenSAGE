using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using OpenSage.Data.Apt.Characters;
using OpenSage.Data.Apt.FrameItems;
using OpenSage.LowLevel.Graphics2D;
using OpenSage.LowLevel.Graphics3D;

namespace OpenSage.Gui.Apt
{
    public interface IDisplayItem
    {
        AptContext Context { get; }
        SpriteItem Parent { get; }
        //the underlying structure that will be used
        Character Character { get; }
        Matrix3x2 Transform { get; }

        void Create(Character chararacter,AptContext context, SpriteItem parent =null);
        void Update(Matrix3x2 pTransform, GameTime gt, DrawingContext dc);
    }

    public sealed class RenderItem : IDisplayItem
    {
        private SpriteItem _parent;
        private Character _character;
        private AptContext _context;

        public SpriteItem Parent => _parent;
        public Character Character => _character;
        public AptContext Context => _context;
        public Matrix3x2 Transform { get; internal set; }

        public void Create(Character chararacter, AptContext context, SpriteItem parent = null)
        {
            _character = chararacter;
            _context = context;
            _parent = parent;
        }

        public void Update(Matrix3x2 pTransform, GameTime gt, DrawingContext dc)
        {
            switch(_character)
            {
                case Shape s:
                    var geometry = _context.GetGeometry(s.Geometry, _character);
                    AptRenderer.RenderGeometry(dc, _context, geometry, pTransform);
                    break;
            }
        }
    }

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
        public Matrix3x2 Transform { get; internal set; }

        public void Create(Character chararacter, AptContext context, SpriteItem parent = null)
        {
            _sprite = (Playable)chararacter;
            _context = context;
            _content = new DisplayList();
            _parent = parent;
            _currentFrame = 0;
        }

        public void Update(Matrix3x2 pTransform, GameTime gt, DrawingContext dc)
        {
            //get the current frame
            var frame = _sprite.Frames[_currentFrame];

            //process all frame items
            foreach(var item in frame.FrameItems)
            {
                HandleFrameItem(item);
            }

            //calculate the transform for this element
            var cTransform = pTransform * Transform;

            //update all subitems
            foreach(var item in _content.Items.Values)
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
            switch(item)
            {
                case PlaceObject po:
                    var character = _context.GetCharacter(po.Character,_sprite);
                    IDisplayItem placeObject;
                    var poTransform = new Matrix3x2(po.RotScale.M11, po.RotScale.M12,
                                                    po.RotScale.M21, po.RotScale.M22,
                                                    po.Translation.X, po.Translation.Y);

                    if (character is Playable)
                        placeObject = new SpriteItem() { Transform = poTransform };
                    else
                        placeObject = new RenderItem() { Transform = poTransform };

                    placeObject.Create(character, _context, this);
                    _content.Items[po.Depth] = placeObject;
                    break;
                case RemoveObject ro:
                    _content.Items.Remove(ro.Depth);
                    break;
            }
        }
    }
}
