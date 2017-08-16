using System;
using System.Collections.Generic;
using SharpDX.Direct3D12;
using LLGfx.Util;

namespace LLGfx
{
    partial class VertexDescriptor
    {
        private InputElement[] _inputElements;
        private InputLayoutDescription _inputLayoutDescription;
        private bool _isDirty = true;

        private Dictionary<int, int> _bufferStrides;

        internal InputLayoutDescription DeviceInputLayoutDescription
        {
            get
            {
                if (_isDirty)
                {
                    _inputLayoutDescription = new InputLayoutDescription(_inputElements);
                    _isDirty = false;
                }
                return _inputLayoutDescription;
            }
        }

        private void PlatformConstruct()
        {
            _inputElements = new InputElement[0];
            _bufferStrides = new Dictionary<int, int>();
        }

        private void PlatformSetAttributeDescriptor(
            int index,
            string semanticName,
            int semanticIndex,
            VertexFormat format,
            int bufferIndex,
            int offset)
        {
            if (index + 1 > _inputElements.Length)
            {
                Array.Resize(ref _inputElements, index + 1);
            }

            _inputElements[index] = new InputElement
            {
                AlignedByteOffset = offset,
                Classification = InputClassification.PerVertexData,
                Format = format.ToDxgiFormat(),
                InstanceDataStepRate = 0,
                SemanticIndex = semanticIndex,
                SemanticName = semanticName,
                Slot = bufferIndex
            };

            _isDirty = true;
        }

        private void PlatformSetLayoutDescriptor(int bufferIndex, int stride)
        {
            _bufferStrides[bufferIndex] = stride;
        }

        internal int GetStride(int bufferIndex)
        {
            if (_bufferStrides.TryGetValue(bufferIndex, out var stride))
            {
                return stride;
            }
            return 0;
        }
    }
}
