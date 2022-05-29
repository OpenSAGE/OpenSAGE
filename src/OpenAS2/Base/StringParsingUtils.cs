using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace OpenAS2.Base
{
    using RawInstructionStorage = SortedList<uint, RawInstruction>;
    using RawConstantStorage = IList<ConstantEntry>;

    public static class StringParsingUtils
    {

        public static string FormInstructionStorage(RawInstructionStorage insts)
        {
            SortedList<uint, string> dtemp = new();
            foreach (var (pos, inst) in insts)
                dtemp[pos] = inst.Serialize();
            return JsonConvert.SerializeObject(dtemp);
        }

        public static RawInstructionStorage ParseInstructionStorage(string sinsts)
        {
            var dtemp = JsonConvert.DeserializeObject<Dictionary<uint, string>>(sinsts);
            RawInstructionStorage res = new();
            foreach (var (pos, inst) in dtemp!)
                res[pos] = RawInstruction.Deserialize(inst);
            return res;
        }

        public static string FormConstantStorage(RawConstantStorage consts)
        {
            List<string> ltemp = new();
            foreach (var c in consts)
                ltemp.Add(c.Serialize());
            return JsonConvert.SerializeObject(ltemp);
        }

        public static RawConstantStorage ParseConstantStorage(string sconsts)
        {
            RawConstantStorage ans = new List<ConstantEntry>();
            var ltemp = JsonConvert.DeserializeObject<List<string>>(sconsts);
            foreach (var l in ltemp)
                ans.Add(ConstantEntry.Deserialize(l));
            return ans;
        }

        public static string DumpInstructionStorage(RawInstructionStorage insts, RawConstantStorage? consts = null)
        {
            RawInstruction? cstp = null;
            int i = 0;
            StringBuilder sb = new();
            foreach (var (pos, inst) in insts)
            {
                if (inst.Type == InstructionType.ConstantPool)
                    cstp = inst;
                sb.AppendLine($"#{i}/${pos}: {inst.ToString(consts, cstp)}");
                ++i;
            }
            return sb.ToString();
        }

    }
}
