using System;
using Metal;
using OpenSage.LowLevel.Graphics3D.Util;

namespace OpenSage.LowLevel.Graphics3D
{
    public sealed partial class VertexDescriptor
    {
        internal MTLVertexDescriptor DeviceVertexDescriptor { get; private set; }

        private void PlatformConstruct(
           VertexAttributeDescription[] attributeDescriptions,
           VertexLayoutDescription[] layoutDescriptions)
        {
            DeviceVertexDescriptor = AddDisposable(MTLVertexDescriptor.Create());

            for (var i = 0; i < attributeDescriptions.Length; i++)
            {
                var attributeDescriptor = DeviceVertexDescriptor.Attributes[i];
                attributeDescriptor.Format = attributeDescriptions[i].Format.ToMTLVertexFormat();
                attributeDescriptor.BufferIndex = (nuint) attributeDescriptions[i].BufferIndex;
                attributeDescriptor.Offset = (nuint) attributeDescriptions[i].Offset;
            }

            for (var i = 0; i < layoutDescriptions.Length; i++)
            {
                var layoutDescriptor = DeviceVertexDescriptor.Layouts[i];
                layoutDescriptor.StepFunction = layoutDescriptions[i].Classification.ToMTLVertexStepFunction();
                layoutDescriptor.StepRate = 1;
                layoutDescriptor.Stride = (nuint) layoutDescriptions[i].Stride;
            }
        }
    }
}
