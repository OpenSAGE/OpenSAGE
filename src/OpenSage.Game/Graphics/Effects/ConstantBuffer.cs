using System;
using System.Collections.Generic;
using System.Linq;
using LLGfx;
using Buffer = LLGfx.Buffer;

namespace OpenSage.Graphics.Effects
{
    internal sealed class ConstantBuffer : GraphicsObject
    {
        private readonly Dictionary<string, ConstantBufferField> _fields;
        private readonly byte[] _data;

        public Buffer Buffer { get; }

        public ConstantBuffer(
            GraphicsDevice graphicsDevice, 
            uint sizeInBytes,
            ConstantBufferField[] fields)
        {
            Buffer = AddDisposable(Buffer.CreateDynamic(
                graphicsDevice, 
                sizeInBytes, 
                BufferBindFlags.ConstantBuffer));

            _data = new byte[sizeInBytes];

            _fields = fields.ToDictionary(x => x.Name);
        }

        public void SetData(string fieldName, byte[] value)
        {
            if (!_fields.TryGetValue(fieldName, out var field))
            {
                throw new InvalidOperationException();
            }

            if (value.Length != field.Size)
            {
                throw new InvalidOperationException();
            }

            System.Buffer.BlockCopy(value, 0, _data, field.Offset, field.Size);
        }

        public void ApplyChanges(CommandEncoder commandEncoder)
        {
            Buffer.SetData(_data);
        }
    }
}
