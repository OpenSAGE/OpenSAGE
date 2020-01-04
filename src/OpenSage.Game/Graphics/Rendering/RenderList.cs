namespace OpenSage.Graphics.Rendering
{
    public sealed class RenderBucket
    {
        public readonly RenderItemCollection RenderItems;

        public RenderBucket()
        {
            RenderItems = new RenderItemCollection();
        }

        public void Clear()
        {
            RenderItems.Clear();
        }
    }

    public sealed class RenderList
    {
        public readonly RenderBucket Terrain = new RenderBucket();
        public readonly RenderBucket Road = new RenderBucket();
        public readonly RenderBucket Water = new RenderBucket();
        public readonly RenderBucket Opaque = new RenderBucket();
        public readonly RenderBucket Transparent = new RenderBucket();
        public readonly RenderBucket Shadow = new RenderBucket();

        public void Clear()
        {
            Terrain.Clear();
            Road.Clear();
            Water.Clear();
            Opaque.Clear();
            Transparent.Clear();
            Shadow.Clear();
        }
    }
}
