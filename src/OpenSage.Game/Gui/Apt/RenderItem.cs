using OpenSage.Content.Translation;
using OpenSage.Data.Apt.Characters;
using OpenSage.Gui.Apt.ActionScript;
using Veldrid;

namespace OpenSage.Gui.Apt
{
    public sealed class RenderItem : DisplayItem
    {
        public Texture Texture { get; set; }

        private bool IsHovered { get; set; }

        public override void Create(Character character, AptContext context, SpriteItem parent = null)
        {
            Character = character;
            Context = context;
            Parent = parent;
            ScriptObject = new ObjectContext(this);
            Name = "";
            Visible = true;
            IsHovered = false;
        }

        public override void Render(AptRenderer renderer, ItemTransform pTransform, DrawingContext2D dc)
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
