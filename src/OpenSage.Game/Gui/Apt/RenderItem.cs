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
        public string Name { get; set; }

        public void Create(Character chararacter, AptContext context, SpriteItem parent = null)
        {
            _character = chararacter;
            _context = context;
            _parent = parent;
            _scriptObject = new ObjectContext(this);
            Name = "";
        }

        public void Update(GameTime gt)
        {

        }

        public void RunActions(GameTime gt)
        {

        }

        public void Render(ItemTransform pTransform, DrawingContext2D dc)
        {
            switch (_character)
            {
                case Shape s:
                    var geometry = _context.GetGeometry(s.Geometry, _character);
                    AptRenderer.RenderGeometry(dc, _context, geometry, pTransform);
                    break;
                case Text t:
                    if(t.Value.Length>0)
                    {
                        var val = ScriptObject.ResolveValue(t.Value,ScriptObject);
                        if (val.Type != ValueType.Undefined)
                            t.Content = val.ToString();
                    }

                    //localize our content
                    t.Content = t.Content.Replace("$", "APT:");
                    t.Content = _context.ContentManager.TranslationManager.Lookup(t.Content);

                    AptRenderer.RenderText(dc, _context, t, pTransform);
                    break;
            }
        }
    }
}
