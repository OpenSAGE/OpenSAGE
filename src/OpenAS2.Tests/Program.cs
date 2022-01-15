using System;
using System.Collections.Generic;
using OpenAS2.Base;
using OpenAS2.Compilation;
using OpenAS2.Runtime;

namespace OpenAS2.Tests
{
    using RawInstructionStorage = SortedList<int, RawInstruction>;

    class Program
    {

        public static InstructionGraph? Graphify(
            RawInstructionStorage? ci,
            IList<ConstantEntry>? constSource = null,
            Dictionary<int, string>? regNames = null
            )
        {
            if (ci == null)
                return null;

            var g_ = new InstructionGraph(ci, 0, null, regNames);
            Console.WriteLine(g_.ToDotForm());

            Console.WriteLine("Gan Si Huang Xu Dong");

            var g = InstructionGraph.OptimizeGraph(g_);
            g = g_;
            var gd = g.ToDotForm();
            System.IO.File.WriteAllText("E:/1.dot", gd);

            int i = 0;
            RawInstruction? cstp = null;
            foreach (var (pos, inst) in ci)
            {
                if (inst.Type == InstructionType.ConstantPool)
                    cstp = inst;
                Console.WriteLine($"#{i}/${pos}: {inst.ToString(constSource, cstp)}");
                ++i;
            }


            Console.WriteLine("Gan Si Huang Xu Dong");


            var c = StructurizedBlockChain.Parse(g.BaseBlock);
            // TODO constpool & constsource

            // try form const pool
            var p = SyntaxNodePool.ConvertToAST(c, constSource, g.RegNames);

            // var c = BlockChainifyUtils.Parse(g.BaseBlock);
            // var p = new NodePool(g.ConstPool, g.RegNames);
            // p.PushBlock(c);

            var sc = new StatementCollection(p);
            var code = sc.Compile();
            Console.Write(code.ToString());

            return g;
        }

        static void Main(string[] args)
        {
            Console.WriteLine("Test");

            string basePath = "E:/aptcodes/";
            string codeFilePath = "", constFilePath = "";

            codeFilePath = "main_mouse_C0_F0_I1_Action";
            codeFilePath = "main_mouse_C0_F0_I2___Packages.Cafe2_Imp_BaseControl_Init";
            constFilePath = "main_mouse_Constants";

            var code = StringParsingUtils.ParseInstructionStorage(System.IO.File.ReadAllText($"{basePath}/{codeFilePath}.asc"));
            var cstf = StringParsingUtils.ParseConstantStorage(System.IO.File.ReadAllText($"{basePath}/{constFilePath}.cst"));

            Console.WriteLine("/Test");

            Graphify(code, cstf);
        }
    }
}
