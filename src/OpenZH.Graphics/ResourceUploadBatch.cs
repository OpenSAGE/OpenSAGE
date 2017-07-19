namespace OpenZH.Graphics
{
    public abstract class ResourceUploadBatch
    {
        public abstract void Begin();

        public abstract void End(CommandQueue commandQueue);
    }
}
