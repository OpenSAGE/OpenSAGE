using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenSage.Data.Apt.Characters;
using OpenSage.LowLevel.Graphics2D;

namespace OpenSage.Gui.Apt
{
    public sealed class RenderItem : IDisplayItem
    {
        private SpriteItem _parent;
        private Character _character;
        private AptContext _context;

        public SpriteItem Parent => _parent;
        public Character Character => _character;
        public AptContext Context => _context;
        public ItemTransform Transform { get; set; }

        public void Create(Character chararacter, AptContext context, SpriteItem parent = null)
        {
            _character = chararacter;
            _context = context;
            _parent = parent;
        }

        public void Update(ItemTransform pTransform, GameTime gt, DrawingContext dc)
        {
            switch (_character)
            {
                case Shape s:
                    var geometry = _context.GetGeometry(s.Geometry, _character);
                    AptRenderer.RenderGeometry(dc, _context, geometry, pTransform);
                    break;
                case Text t:
                    AptRenderer.RenderText(dc, _context, t, pTransform);
                    break;
            }
        }
    }
}
