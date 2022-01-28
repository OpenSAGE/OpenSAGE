using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenAS2.Base;

namespace OpenAS2.Compilation
{

    // https://www.csd.uwo.ca/~mmorenom/CS447/Lectures/CodeOptimization.html/node6.html
    public static class ControlFlowUtils
    {
        public static Dictionary<InstructionBlock, HashSet<InstructionBlock>> FindAllDominators(InstructionGraph g)
        {
            if (g.BaseBlock == null)
                return new();

            List<InstructionBlock> v = new();
            Dictionary<InstructionBlock, List<InstructionBlock>> e = new();
            for (var n = g.BaseBlock; n != null; n = n.NextBlockDefault)
            {
                v.Add(n);
                if (n.HasConditionalBranch)
                    e[n] = new() { n.NextBlockDefault!, n.NextBlockCondition! };
                else if (n.HasConstantBranch)
                    e[n] = new() { n.NextBlockCondition! };
                else
                    e[n] = new() { n.NextBlockDefault! };
                e[n] = e[n].Where(x => x != null).ToList();
            }
                

            // D(n)
            Dictionary<InstructionBlock, HashSet<InstructionBlock>> d = new();
            d[v[0]] = new() { v[0] };
            foreach (var n in v.Skip(1))
                d[n] = new(v);

            var flag = true; // true if any changes occur
            while (flag)
            {
                var fcache = false;
                foreach (var n in v)
                {
                    var oldDn = d[n];
                    var newDn = new HashSet<InstructionBlock>() { n };
                    foreach (var p in e[n])
                        newDn.UnionWith(d[p]);
                    fcache = fcache || (!oldDn.SetEquals(newDn));
                    d[n] = newDn;
                }
                flag = fcache;
            }

            return d;
        }
    }
}
