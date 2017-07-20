using System;
using Metal;
using OpenZH.Graphics.Platforms.Metal;

namespace OpenZH.Graphics
{
    partial class VertexDescriptor
    {
        internal MTLVertexDescriptor DeviceVertexDescriptor { get; private set; }

        private void PlatformConstruct()
        {
            DeviceVertexDescriptor = MTLVertexDescriptor.Create();
        }

        private void PlatformSetAttributeDescriptor(
            int index,
            string semanticName,
            int semanticIndex,
            VertexFormat format,
            int bufferIndex,
            int offset)
        {
            var attributeDescriptor = DeviceVertexDescriptor.Attributes[index];

            attributeDescriptor.Format = format.ToMTLVertexFormat();
            attributeDescriptor.BufferIndex = (nuint) bufferIndex;
            attributeDescriptor.Offset = (nuint) offset;
        }

        private void PlatformSetLayoutDescriptor(int bufferIndex, int stride)
        {
            var layout = DeviceVertexDescriptor.Layouts[bufferIndex];

            layout.StepFunction = MTLVertexStepFunction.PerVertex;
            layout.StepRate = 1;
            layout.Stride = (nuint) stride;
        }
    }
}
