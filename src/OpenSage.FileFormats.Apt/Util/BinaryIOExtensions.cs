using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using OpenSage.Mathematics;
using OpenSage.FileFormats.Apt.ActionScript;

namespace OpenSage.FileFormats.Apt
{
    public static class BinaryIOExtensions
    {
       
        // others

        public static void WriteInstructions(this BinaryWriter writer, InstructionStorage insts, BinaryMemoryChain memory)
        {
            memory.RegisterPostOffset((uint) writer.BaseStream.Position, align: Constants.IntPtrSize);
            writer.Write((UInt32) 0);
            insts.Write(memory.Writer, memory.Post);
            /*
            memory.RegisterGlobalAlignObject((uint) writer.BaseStream.Position, (curWriter, postPool) =>
            {
                curWriter.Align(Constants.IntPtrSize, postPool: postPool);
                uint ret1 = (uint) curWriter.BaseStream.Position;
                insts.Write(curWriter, postPool);
                uint ret2 = (uint) curWriter.BaseStream.Position;
                return (ret1, ret2);
            });
            writer.Write((UInt32) 0); // int pointer
            */
        }

    }
}
