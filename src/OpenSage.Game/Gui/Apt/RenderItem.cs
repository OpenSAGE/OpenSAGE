using OpenSage.Content.Translation;
using OpenSage.Data.Apt.Characters;
using OpenSage.Gui.Apt.ActionScript;
using Veldrid;

namespace OpenSage.Gui.Apt
{
    public sealed class RenderItem : IDisplayItem
    {
        public SpriteItem Parent { get; private set; }
        public Character Character { get; private set; }
        public AptContext Context { get; private set; }
        public ItemTransform Transform { get; set; }
        public ObjectContext ScriptObject { get; private set; }
        public Texture Texture { get; set; }
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

        public void Update(TimeInterval gt)
        {

        }

        public void RunActions(TimeInterval gt)
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
                    renderer.RenderGeometry(dc, Context, geometry, pTransform, Texture);
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
                    t.Content = t.Content.Translate();

                    renderer.RenderText(dc, Context, t, pTransform);
                    break;
            }
        }
    }
}
