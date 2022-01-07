using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace OpenAS2.Base
{
    using RawInstructionStorage = SortedList<int, RawInstruction>;
    using RawConstantStorage = IList<ConstantEntry>;

    public static class StringParsingUtils
    {

        public static string FormInstructionStorage(RawInstructionStorage insts)
        {
            SortedList<int, string> dtemp = new();
            foreach (var (pos, inst) in insts)
                dtemp[pos] = inst.Serialize();
            return JsonSerializer.Serialize(dtemp);
        }

        public static RawInstructionStorage ParseInstructionStorage(string sinsts)
        {
            var dtemp = JsonSerializer.Deserialize<Dictionary<int, string>>(sinsts);
            RawInstructionStorage res = new SortedList<int, RawInstruction>();
            foreach (var (pos, inst) in dtemp)
                res[pos] = RawInstruction.Deserialize(inst);
            return res;
        }

        public static string FormConstantStorage(RawConstantStorage consts)
        {
            List<string> ltemp = new();
            foreach (var c in consts)
                ltemp.Add(c.Serialize());
            return JsonSerializer.Serialize(ltemp);
        }

        public static RawConstantStorage ParseConstantStorage(string sconsts)
        {
            RawConstantStorage ans = new List<ConstantEntry>();
            var ltemp = JsonSerializer.Deserialize<List<string>>(sconsts);
            foreach (var l in ltemp)
                ans.Add(ConstantEntry.Deserialize(l));
            return ans;
        }

    }
}
