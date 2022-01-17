using System;
using System.Collections.Generic;
using System.Text;
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
            // Console.WriteLine(g_.ToDotForm());

            Console.WriteLine("Gan Si Huang Xu Dong");

            var g = g_; // InstructionGraph.OptimizeGraph(g_);
            var gd = g.ToDotForm();

            System.IO.File.WriteAllText("E:/1.dot", gd);

            var dinst = StringParsingUtils.DumpInstructionStorage(ci, constSource);
            System.IO.File.WriteAllText("E:/1.txt", dinst);


            Console.WriteLine("Gan Si Huang Xu Dong");


            var c = StructurizedBlockChain.Parse(g.BaseBlock);
            // TODO constpool & constsource

            // try form const pool
            var p = SyntaxNodePool.ConvertToAST(c, constSource, g.RegNames);


            var sc = new StatementCollection(p);
            var code = sc.Compile().ToString();
            System.IO.File.WriteAllText("E:/1.js", code);
            Console.Write(code);

            return g;
        }

        public static void TestVM(
            RawInstructionStorage ci,
            IList<ConstantEntry> constSource
            )
        {

        }

        static void Main(string[] args)
        {
            Console.WriteLine("Test");

            string basePath = "E:/aptcodes/";
            string codeFilePath = "", constFilePath = "";

            codeFilePath = "main_mouse_C0_F0_I1_Action";
            // codeFilePath = "main_mouse_C0_F0_I2___Packages.Cafe2_Imp_BaseControl_Init";
            constFilePath = "main_mouse_Constants";

            var code = StringParsingUtils.ParseInstructionStorage(System.IO.File.ReadAllText($"{basePath}/{codeFilePath}.asc"));
            var cstf = StringParsingUtils.ParseConstantStorage(System.IO.File.ReadAllText($"{basePath}/{constFilePath}.cst"));

            Console.WriteLine("/Test");

            // Graphify(code, cstf);
            TestVM(code, cstf);
        }
    }
}
