using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenSage.Data.Utilities.Extensions;

namespace OpenSage.Gui.Apt.ActionScript
{
    public sealed class InstructionReader
    {
        private BinaryReader _reader;
        private uint _offset;

        public List<IInstruction> Instructions { get; private set; }

        public InstructionReader(Stream input)
        {           
            _reader = new BinaryReader(input);
            _offset = _reader.ReadUInt32();
            Instructions = new List<IInstruction>();
        }

        public void Parse()
        {
            var current = _reader.BaseStream.Position;
            _reader.BaseStream.Seek(_offset, SeekOrigin.Begin);
            bool parsing = true;

            while (parsing)
            {
                //now reader the instructions
                var type = _reader.ReadByteAsEnum<InstructionType>();
                var aligned = InstructionAlignment.IsAligned(type);

                if (aligned)
                    _reader.Align(4);

                IInstruction instruction;
                List<Value> parameters = new List<Value>();

                switch (type)
                {
                    case InstructionType.ConstantPool:
                        var count = _reader.ReadUInt32();
                        var constants = _reader.ReadFixedSizeArrayAtOffset<uint>(() => _reader.ReadUInt32(), count);

                        foreach (var constant in constants)
                        {
                            parameters.Add(Value.Constant(constant));
                        }

                        instruction = new ConstantPool();
                        instruction.Parameters = parameters;
                        break;
                    case InstructionType.End:
                        parsing = false;
                        break;
                    default:
                        throw new InvalidDataException("Unimplemented bytecode instruction:" + type.ToString());
                }              
            }
            _reader.BaseStream.Seek(current,SeekOrigin.Begin);

        }
    }
}
