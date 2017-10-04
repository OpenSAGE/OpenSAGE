using System;

namespace LLGfx
{
    public sealed partial class VertexDescriptor
    {
        public VertexDescriptor()
        {
            PlatformConstruct();
        }

        public void SetAttributeDescriptor(
            InputClassification classification,
            int index,
            string semanticName,
            int semanticIndex,
            VertexFormat format,
            int bufferIndex,
            int offset)
        {
            PlatformSetAttributeDescriptor(
                classification,
                index,
                semanticName,
                semanticIndex,
                format,
                bufferIndex,
                offset);
        }

        public void SetLayoutDescriptor(int bufferIndex, int stride)
        {
            if (stride % 4 != 0)
            {
                throw new ArgumentOutOfRangeException(nameof(stride), "Stride must be a multiple of 4.");
            }

            PlatformSetLayoutDescriptor(bufferIndex, stride);
        }
    }
}
