namespace OpenZH.Graphics.Metal
{
    public sealed class MetalResourceUploadBatch : ResourceUploadBatch
    {
        internal MetalResourceUploadBatch() { }

        public override void Begin()
        {
            // No-op.
        }

        public override void End(CommandQueue commandQueue)
        {
            // No-op.
        }
    }
}