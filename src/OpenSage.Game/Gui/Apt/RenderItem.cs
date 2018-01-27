using OpenSage.Data.Apt.Characters;
using OpenSage.Gui.Apt.ActionScript;

namespace OpenSage.Gui.Apt
{
    public sealed class RenderItem : IDisplayItem
    {
        private SpriteItem _parent;
        private Character _character;
        private AptContext _context;
        private ObjectContext _scriptObject;

        public SpriteItem Parent => _parent;
        public Character Character => _character;
        public AptContext Context => _context;
        public ItemTransform Transform { get; set; }
        public ObjectContext ScriptObject => _scriptObject;

        public void Create(Character chararacter, AptContext context, SpriteItem parent = null)
        {
            _character = chararacter;
            _context = context;
            _parent = parent;
            _scriptObject = new ObjectContext(this);
        }

        public void Update(ItemTransform pTransform, GameTime gt, DrawingContext2D dc)
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

        public void RunActions(GameTime gt)
        {

        }
    }
}
