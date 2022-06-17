namespace OpenSage.Graphics.Rendering
{
    public sealed class RenderBucket
    {
        public readonly RenderItemCollection RenderItems;
        public readonly string RenderItemName;

        public RenderBucket(string name)
        {
            RenderItemName = name;
            RenderItems = new RenderItemCollection();
        }

        public void Clear()
        {
            RenderItems.Clear();
        }
    }

    public sealed class RenderList
    {
        public readonly RenderBucket Opaque = new RenderBucket("Opaque");
        public readonly RenderBucket Transparent = new RenderBucket("Transparent");
        public readonly RenderBucket Shadow = new RenderBucket("Shadow");
        public readonly RenderBucket Water = new RenderBucket("Water");

        public void Clear()
        {
            Water.Clear();
            Opaque.Clear();
            Transparent.Clear();
            Shadow.Clear();
        }
    }
}
