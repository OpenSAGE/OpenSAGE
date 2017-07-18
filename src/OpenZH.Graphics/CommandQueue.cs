namespace OpenZH.Graphics
{
    public abstract class CommandQueue : GraphicsObject
    {
        public abstract CommandBuffer GetCommandBuffer();
    }
}
