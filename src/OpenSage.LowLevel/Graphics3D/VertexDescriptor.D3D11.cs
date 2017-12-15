using OpenSage.LowLevel.Graphics3D.Util;
using D3D11 = SharpDX.Direct3D11;

namespace OpenSage.LowLevel.Graphics3D
{
    partial class VertexDescriptor
    {
        private VertexLayoutDescription[] _layoutDescriptions;

        internal D3D11.InputElement[] DeviceInputElements { get; private set; }

        private void PlatformConstruct(
            VertexAttributeDescription[] attributeDescriptions,
            VertexLayoutDescription[] layoutDescriptions)
        {
            DeviceInputElements = new D3D11.InputElement[attributeDescriptions.Length];
            for (var i = 0; i < attributeDescriptions.Length; i++)
            {
                var attributeDescription = attributeDescriptions[i];
                var layoutDescription = layoutDescriptions[attributeDescription.BufferIndex];

                DeviceInputElements[i] = new D3D11.InputElement
                {
                    AlignedByteOffset = attributeDescription.Offset,
                    Classification = layoutDescription.Classification.ToInputClassification(),
                    Format = attributeDescription.Format.ToDxgiFormat(),
                    InstanceDataStepRate = (layoutDescription.Classification == InputClassification.PerInstanceData) ? 1 : 0,
                    SemanticIndex = attributeDescription.SemanticIndex,
                    SemanticName = attributeDescription.SemanticName,
                    Slot = attributeDescription.BufferIndex
                };
            }

            _layoutDescriptions = layoutDescriptions;
        }

        internal int GetStride(int bufferIndex)
        {
            return _layoutDescriptions[bufferIndex].Stride;
        }
    }
}
