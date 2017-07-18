namespace OpenZH.Graphics
{
    public abstract class GraphicsDevice : GraphicsObject
    {
        public abstract CommandQueue CommandQueue { get; }

        public abstract RenderPassDescriptor CreateRenderPassDescriptor();
    }
}
