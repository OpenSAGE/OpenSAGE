namespace OpenZH.Graphics
{
    public abstract class Texture : GraphicsObject
    {
        public abstract void SetData(
            ResourceUploadBatch uploadBatch,
            int level,
            byte[] data,
            int bytesPerRow);
    }
}
