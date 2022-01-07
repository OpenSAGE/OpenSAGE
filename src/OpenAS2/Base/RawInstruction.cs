using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using OpenSage.FileFormats;
using System.Text.Json;

namespace OpenAS2.Base
{

    public class RawInstruction
    {
        public virtual InstructionType Type { get; internal set; }
        public List<RawValue> Parameters { get; private set; }

        public RawInstruction(InstructionType type, List<RawValue> parameters = null)
        {
            Type = type;
            Parameters = parameters;
        }

        public string Serialize()
        {
            List<string> s = new() { ((int) Type).ToString() };
            foreach (var val in Parameters)
                s.Add(val.Serialize());
            return JsonSerializer.Serialize(s);
        }

        public static RawInstruction Deserialize(string str)
        {
            var s = JsonSerializer.Deserialize<List<string>>(str);
            var p = (InstructionType) int.Parse(s[0]);
            List<RawValue> pars = new();
            foreach (var val in s.Skip(1))
                pars.Add(RawValue.Deserialize(val));
            return new RawInstruction(p, pars);
        }

    }
}
