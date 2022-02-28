using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using OpenAS2.Base;

namespace OpenSage.FileFormats.Apt
{
    public static class InstructionBinaryIOUtils
    {
        public static RawInstruction Parse(InstructionType type, BinaryReader reader, InstructionParsingHelper helper)
        {
            var paramTypes = Definition.GetParamSequence(type);

            var parameters = new List<RawValue>();

            var cur_index = 0;
            var array_size = -1;
            var array_begin = -1;
            var array_read = -1;
            long old_offset = -1;
            while (cur_index < paramTypes.Count)
            {
                var t = paramTypes[cur_index];
                switch (t)
                {
                    case RawParamType.UI8:
                        parameters.Add(RawValue.FromInteger(reader.ReadByte()));
                        ++cur_index;
                        break;
                    case RawParamType.UI16:
                        parameters.Add(RawValue.FromInteger(reader.ReadUInt16()));
                        ++cur_index;
                        break;
                    case RawParamType.UI24:
                        parameters.Add(RawValue.FromUInteger(reader.ReadUInt24()));
                        ++cur_index;
                        break;
                    case RawParamType.UI32:
                        parameters.Add(RawValue.FromUInteger(reader.ReadUInt32()));
                        ++cur_index;
                        break;

                    case RawParamType.I8:
                        parameters.Add(RawValue.FromInteger(reader.ReadSByte()));
                        ++cur_index;
                        break;
                    case RawParamType.I16:
                        parameters.Add(RawValue.FromInteger(reader.ReadInt16()));
                        ++cur_index;
                        break;
                    case RawParamType.I32:
                        parameters.Add(RawValue.FromInteger(reader.ReadInt32()));
                        ++cur_index;
                        break;

                    case RawParamType.Jump8:
                        reader.ReadByte();
                        ++cur_index;
                        break;
                    case RawParamType.Jump16:
                        reader.ReadUInt16();
                        ++cur_index;
                        break;
                    case RawParamType.Jump32:
                        reader.ReadUInt32();
                        ++cur_index;
                        break;
                    case RawParamType.Jump64:
                        reader.ReadUInt64();
                        ++cur_index;
                        break;

                    case RawParamType.Float:
                        parameters.Add(RawValue.FromFloat(reader.ReadSingle()));
                        ++cur_index;
                        break;
                    case RawParamType.Double:
                        parameters.Add(RawValue.FromFloat(reader.ReadDouble()));
                        ++cur_index;
                        break;

                    case RawParamType.String:
                        parameters.Add(RawValue.FromString(reader.ReadStringAtOffset()));
                        ++cur_index;
                        break;

                    case RawParamType.Constant:
                        parameters[parameters.Count - 1] = parameters.Last().ToConstant();
                        ++cur_index;
                        break;
                    case RawParamType.Register:
                        parameters[parameters.Count - 1] = parameters.Last().ToRegister();
                        ++cur_index;
                        break;

                    case RawParamType.BranchOffset:
                        if (helper != null)
                            helper.ReportBranchOffset((uint) parameters.Last().Integer);
                        ++cur_index;
                        break;

                    case RawParamType.ArraySize:
                        array_size = parameters.Last().Integer;
                        ++cur_index;
                        break;
                    case RawParamType.ArrayBegin:
                        if (array_size < 0)
                            throw new InvalidOperationException("Array size must be assigned in prior.");
                        else if (array_begin >= 0)
                            throw new InvalidOperationException("Unfortunately, we do not support cascaded arrays yet.");
                        else if (array_size == 0)
                        {
                            reader.ReadUInt32();
                            while (paramTypes[cur_index] != RawParamType.ArrayEnd)
                                ++cur_index;
                            ++cur_index;
                            break;
                        }
                        array_begin = cur_index;
                        array_read = 0;
                        ++cur_index;

                        var listOffset = reader.ReadUInt32();
                        old_offset = reader.BaseStream.Position;
                        reader.BaseStream.Seek(listOffset, SeekOrigin.Begin);

                        break;

                    case RawParamType.ArrayEnd:
                        if (array_size < 0)
                            throw new InvalidOperationException("Array size must be assigned in prior.");
                        ++array_read;
                        if (array_read >= array_size)
                        {
                            array_begin = -1;
                            array_read = -1;
                            array_size = -1;
                            reader.BaseStream.Seek(old_offset, SeekOrigin.Begin);
                            old_offset = -1;
                            ++cur_index;
                        }
                        else
                        {
                            cur_index = array_begin + 1;
                        }
                        break;


                    default:
                        throw new NotImplementedException();
                }
            }

            var ans = new RawInstruction(type, parameters);
            return ans;
        }

        public static void Write(this RawInstruction inst, BinaryWriter writer, BinaryMemoryChain pool)
        {
            var cur_index = 0;
            var cur_param_index = 0;
            var array_size = -1;
            var array_begin = -1;
            var array_read = -1;
            var orig_writer = writer;
            var orig_pool = pool;

            var paramTypes = Definition.GetParamSequence(inst.Type);
            var parameters = inst.Parameters;

            while (cur_index < paramTypes.Count)
            {
                var t = paramTypes[cur_index];
                var p = cur_param_index >= parameters.Count ? null : parameters[cur_param_index];
                var p_prev = cur_param_index == 0 ? null : parameters[cur_param_index - 1];
                ++cur_param_index;
                switch (t)
                {
                    case RawParamType.UI8:
                        writer.Write((Byte)p.Integer);
                        ++cur_index;
                        break;
                    case RawParamType.UI16:
                        writer.Write((UInt16)p.Integer);
                        ++cur_index;
                        break;
                    case RawParamType.UI24:
                        writer.WriteUInt24((uint)p.Integer);
                        ++cur_index;
                        break;
                    case RawParamType.UI32:
                        writer.Write((UInt32)p.Integer);
                        ++cur_index;
                        break;

                    case RawParamType.I16:
                        writer.Write((Int16)p.Integer);
                        ++cur_index;
                        break;
                    case RawParamType.I32:
                        writer.Write((Int32)p.Integer);
                        ++cur_index;
                        break;

                    case RawParamType.Jump8:
                        writer.Write((Byte)0);
                        --cur_param_index;
                        ++cur_index;
                        break;
                    case RawParamType.Jump16:
                        writer.Write((UInt16)0);
                        --cur_param_index;
                        ++cur_index;
                        break;
                    case RawParamType.Jump32:
                        writer.Write((UInt32)0);
                        --cur_param_index;
                        ++cur_index;
                        break;
                    case RawParamType.Jump64:
                        writer.Write((UInt64)0);
                        --cur_param_index;
                        ++cur_index;
                        break;

                    case RawParamType.Float:
                        writer.Write((Single)p.Double);
                        ++cur_index;
                        break;
                    case RawParamType.Double:
                        writer.Write((Double)p.Double);
                        ++cur_index;
                        break;

                    case RawParamType.String:
                        writer.WriteStringAtOffset(p.String, pool);
                        ++cur_index;
                        break;

                    case RawParamType.Constant:
                    case RawParamType.Register:
                    case RawParamType.BranchOffset:
                        --cur_param_index;
                        ++cur_index;
                        break;

                    case RawParamType.ArraySize:
                        array_size = p_prev.Integer;
                        --cur_param_index;
                        ++cur_index;
                        break;
                    case RawParamType.ArrayBegin:
                        --cur_param_index;
                        if (array_size < 0)
                            throw new InvalidOperationException("Array size must be assigned in prior.");
                        else if (array_begin >= 0)
                            throw new InvalidOperationException("Unfortunately, we do not support cascaded arrays yet.");

                        array_begin = cur_index;
                        pool.RegisterPostOffset((uint)writer.BaseStream.Position);
                        writer.Write((UInt32)0);
                        array_read = 0;
                        writer = pool.Writer;
                        pool = pool.Post;

                        if (array_size == 0)
                        {
                            while (paramTypes[cur_index] != RawParamType.ArrayEnd)
                                ++cur_index;
                            break;
                        }
                        ++cur_index;
                        break;

                    case RawParamType.ArrayEnd:
                        --cur_param_index;
                        if (array_size < 0)
                            throw new InvalidOperationException("Array size must be assigned in prior.");
                        ++array_read;
                        if (array_read >= array_size)
                        {
                            array_begin = -1;
                            array_read = -1;
                            array_size = -1;
                            writer = orig_writer;
                            pool = orig_pool;
                            ++cur_index;
                        }
                        else
                        {
                            cur_index = array_begin + 1;
                        }
                        break;


                    default:
                        throw new NotImplementedException();
                }
            }
        }
    }
}
