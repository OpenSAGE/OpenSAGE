using OpenSage.Data.Apt.Characters;
using OpenSage.Gui.Apt.ActionScript;

namespace OpenSage.Gui.Apt
{
    public sealed class RenderItem : IDisplayItem
    {
        public SpriteItem Parent { get; private set; }
        public Character Character { get; private set; }
        public AptContext Context { get; private set; }
        public ItemTransform Transform { get; set; }
        public ObjectContext ScriptObject { get; private set; }
        public string Name { get; set; }
        public bool Visible { get; set; }

        public void Create(Character chararacter, AptContext context, SpriteItem parent = null)
        {
            Character = chararacter;
            Context = context;
            Parent = parent;
            ScriptObject = new ObjectContext(this);
            Name = "";
            Visible = true;
        }

        public void Update(GameTime gt)
        {

        }

        public void RunActions(GameTime gt)
        {

        }

        public void Render(AptRenderer renderer, ItemTransform pTransform, DrawingContext2D dc)
        {
            if (!Visible)
                return;

            switch (Character)
            {
                case Shape s:
                    var geometry = Context.GetGeometry(s.Geometry, Character);
                    renderer.RenderGeometry(dc, Context, geometry, pTransform);
                    break;
                case Text t:
                    if (t.Value.Length > 0)
                    {
                        var val = ScriptObject.ResolveValue(t.Value, ScriptObject);
                        if (val.Type != ValueType.Undefined)
                            t.Content = val.ToString();
                    }

                    //localize our content
                    t.Content = t.Content.Replace("$", "APT:");
                    t.Content = Context.ContentManager.TranslationManager.Lookup(t.Content);

                    renderer.RenderText(dc, Context, t, pTransform);
                    break;
            }
        }
    }
}
