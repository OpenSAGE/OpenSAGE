using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Newtonsoft.Json;
using OpenAS2.Base;
using OpenAS2.Compilation;
using OpenAS2.Runtime;
using OpenAS2.Runtime.Dom.Default;

namespace OpenAS2.Tests
{
    using RawInstructionStorage = SortedList<uint, RawInstruction>;
    using InstKvp = KeyValuePair<uint, RawInstruction>;

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


            Definition.GetParamLength(InstructionType.End);
            var g_ = new InstructionGraph(ci, 0, null, regNames);

            List<InstKvp> cil = new List<InstKvp>(ci);
            var ci2 = GraphifyUtils.ReverseGraphify(g_, cil[0].Key);
            List<InstKvp> ci2l = new List<InstKvp>(ci2);

            int i = 0;
            
            List<uint> origOffset = cil.Select(x => x.Key - cil[0].Key).ToList();
            var offset = GraphifyUtils.CalculateOffset(ci.Select(x => x.Value.Type));
            foreach (var (pos, ins) in ci)
            {
                var (pos2, ins2) = ci2l[i];
                var (npos, nins) = cil[(i + 1) % ci.Count];
                Console.WriteLine($"{pos} {pos % 4} {(npos > pos ? npos - pos : 0)} {GraphifyUtils.GetRealParamLength(pos, ins.Type)} {origOffset[i]} {offset[i]} {ins.Type} {Definition.IsAlignmentRequired(ins.Type)}");
                Console.WriteLine($"{ins} \n{ins2}");
                ++i;
            }

            Console.Write($"IsStorageEquivalent = {GraphifyUtils.IsStorageEquivalent(ci, ci2, out var msg)}\n{msg}");

            
            // Console.WriteLine(JsonConvert.SerializeObject(g_, Formatting.Indented));

            Console.WriteLine("Gan Si Huang Xu Dong");

            var g = g_; // GraphifyUtils.OptimizeGraph(g_);
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
            var dom = new SimpleDomHandler();
            var vm = new VirtualMachine(dom);

            Console.WriteLine("Test2");
        }

        static void Main(string[] args)
        {
            Console.WriteLine("Test");
            
            string basePath = "E:/aptcodes/";
            string codeFilePath = "", constFilePath = "";

            codeFilePath = "main_mouse_C0_F0_I1_Action";
            // codeFilePath = "main_mouse_C0_F0_I2___Packages.Cafe2_Imp_BaseControl_Init";
            // codeFilePath = "main_mouse_C0_F0_I41___Packages.main_mouse_settings_Init";
            // codeFilePath = "main_mouse_C0_F0_I40___Packages.ICafe2_FWSettings_Init";

            // codeFilePath = "fes_m_chapterSelect_C0_F0_I12___Packages.Cafe2_BaseUIScreen_Init";
            // codeFilePath = "fes_m_chapterSelect_C0_F0_I13___Packages.fes_m_chapterSelect_Init";


            constFilePath = "main_mouse_Constants";

            ///constFilePath = "fes_m_chapterSelect_Constants";

            var code = StringParsingUtils.ParseInstructionStorage(System.IO.File.ReadAllText($"{basePath}/{codeFilePath}.asc"));

            // var cj = JsonConvert.SerializeObject(code, Formatting.Indented);
            // Console.WriteLine(cj);
            // var cj2 = JsonConvert.DeserializeObject(cj);
            // Console.WriteLine(JsonConvert.SerializeObject(cj2, Formatting.Indented) == cj);

            var cstf = StringParsingUtils.ParseConstantStorage(System.IO.File.ReadAllText($"{basePath}/{constFilePath}.cst"));
            // Console.WriteLine(JsonConvert.SerializeObject(cstf));

            Console.WriteLine("/Test");

            Graphify(code, cstf);
            // TestVM(code, cstf);
        }
    }
}
