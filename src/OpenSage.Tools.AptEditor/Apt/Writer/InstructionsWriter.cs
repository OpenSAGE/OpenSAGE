using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using OpenSage.FileFormats;
using OpenSage.Gui.Apt.ActionScript;
using OpenSage.Gui.Apt.ActionScript.Opcodes;
using OpenSage.Tools.AptEditor.Util;

namespace OpenSage.Tools.AptEditor.Apt.Writer
{
    public static partial class DataWriter
    {
        public static uint WriteImpl(MemoryPool memory, InstructionCollection instructionCollection)
        {
            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream))
            {
                var instructions = instructionCollection.GetInstructions();
                foreach(var instruction in instructions)
                {
                    if(instruction.Type == InstructionType.Padding)
                    {
                        continue;
                    }

                    writer.Write((Byte)instruction.Type);
                    if(InstructionAlignment.IsAligned(instruction.Type))
                    {
                        while(writer.BaseStream.Position % Constants.IntPtrSize != 0)
                        {
                            writer.Write((Byte)0);
                        }
                    }
                    InstructionParameterWriter.Write(writer, memory, instruction);
                }

                memory.AllocateBytesForPadding(Constants.IntPtrSize);
                return memory.WriteDataToNewChunk(stream.ToArray()).StartAddress;
            }
        }
    }

    static class InstructionParameterWriter
    {
        public static void Write(BinaryWriter writer, MemoryPool memory, InstructionBase instruction)
        {
            var type = instruction.Type;
            var parameters = instruction.Parameters;
            switch (type)
            {
                case InstructionType.GotoFrame2:
                case InstructionType.EA_PushRegister:
                case InstructionType.EA_PushByte:
                    writer.Write((Byte)parameters.First().ToInteger());
                    break;

                case InstructionType.EA_PushShort:
                    writer.Write((UInt16)parameters.First().ToInteger());
                    break;

                case InstructionType.GotoFrame:
                case InstructionType.SetRegister:
                case InstructionType.BranchAlways:
                case InstructionType.BranchIfTrue:
                    writer.Write((Int32)parameters.First().ToInteger());
                    break;

                case InstructionType.EA_PushFloat:
                    writer.Write((Single)parameters.First().ToReal());
                    break;

                case InstructionType.EA_PushConstantByte:
                case InstructionType.EA_PushValueOfVar:
                case InstructionType.EA_GetNamedMember:
                case InstructionType.EA_CallNamedFuncPop:
                case InstructionType.EA_CallNamedFunc:
                case InstructionType.EA_CallNamedMethodPop:
                case InstructionType.EA_CallNamedMethod:
                    writer.Write((Byte)parameters.First().ToInteger());
                    break;

                case InstructionType.EA_PushConstantWord:
                    writer.Write((UInt16)parameters.First().ToInteger());
                    break;

                case InstructionType.GotoLabel:
                case InstructionType.EA_PushString:
                case InstructionType.EA_GetStringVar:
                case InstructionType.EA_SetStringVar:
                case InstructionType.EA_GetStringMember:
                case InstructionType.EA_SetStringMember:
                    {
                        var stringAddress = DataWriter.Write(memory, parameters.First().ToString());
                        writer.Write((UInt32)stringAddress);
                    }
                    break;

                case InstructionType.GetURL:
                    {
                        var urlAddress = DataWriter.Write(memory, parameters[0].ToString());
                        var targetAddress = DataWriter.Write(memory, parameters[1].ToString());

                        writer.Write((UInt32)urlAddress);
                        writer.Write((UInt32)targetAddress);
                    }
                    break;

                case InstructionType.ConstantPool:
                    {
                        writer.Write((UInt32)parameters.Count);
                        var constantIDArray = parameters.ConvertAll<UInt32>((value) => (UInt32)value.ToInteger());
                        var constantIDArrayAddress = ArrayWriter.WritePlainArray(memory, constantIDArray);
                        writer.Write((UInt32)constantIDArrayAddress);
                    }
                    break;

                case InstructionType.DefineFunction2:
                    {
                        var functionNameAddress = DataWriter.Write(memory, parameters[0].ToString());
                        var numberOfParameters = parameters[1].ToInteger();
                        var numberOfRegisters = parameters[2].ToInteger();
                        var flags = (UInt32)parameters[3].ToInteger();

                        var registersAndParameterNameAddresses = parameters.Skip(4).SkipLast(1).Select((value, index) =>
                        {
                            if (index % 2 == 0) // register
                            {
                                return (UInt32)value.ToInteger();
                            }
                            else // parameter name
                            {
                                return (UInt32)DataWriter.Write(memory, value.ToString());
                            }

                        }).ToList();

                        if (registersAndParameterNameAddresses.Count != numberOfParameters * 2)
                        {
                            throw new InvalidDataException();
                        }

                        var parameterListAddress = ArrayWriter.WritePlainArray(memory, registersAndParameterNameAddresses);
                        var functionBodySize = parameters.Last().ToInteger();

                        writer.Write((UInt32)functionNameAddress);
                        writer.Write((UInt32)numberOfParameters);
                        writer.Write((Byte)numberOfRegisters);
                        writer.WriteUInt24(flags);
                        writer.Write((UInt32)parameterListAddress);
                        writer.Write((UInt32)functionBodySize);
                        writer.Write((UInt64)Constants.DefineFunctionUnknownNumber);
                    }
                    break;

                case InstructionType.PushData:
                    {
                        writer.Write((UInt32)parameters.Count);
                        var constantIDArray = parameters.ConvertAll<UInt32>((value) => (UInt32)value.ToInteger());
                        var constantIDArrayAddress = ArrayWriter.WritePlainArray(memory, constantIDArray);
                        writer.Write((UInt32)constantIDArrayAddress);
                    }
                    break;

                case InstructionType.DefineFunction:
                    {
                        var functionNameAddress = DataWriter.Write(memory, parameters[0].ToString());
                        var numberOfParameters = parameters[1].ToInteger();
                        var parameterList = parameters.Skip(2).SkipLast(1).Select(value => value.ToString()).ToList();
                        var parameterListAddress = ArrayWriter.WriteArrayOfPointers(memory, parameterList);
                        var functionBodySize = parameters.Last().ToInteger();

                        writer.Write((UInt32)functionNameAddress);
                        writer.Write((UInt32)numberOfParameters);
                        writer.Write((UInt32)parameterListAddress);
                        writer.Write((UInt32)functionBodySize);
                        writer.Write((UInt64)Constants.DefineFunctionUnknownNumber);
                    }
                    break;

                default:
                    if (parameters.Count != 0)
                    {
                        throw new NotImplementedException("Unimplemented instruction:" + type.ToString());
                    }
                    // Otherwise, just exit since we don't have to write out any parameters
                    break;
            }
        }
    }
}