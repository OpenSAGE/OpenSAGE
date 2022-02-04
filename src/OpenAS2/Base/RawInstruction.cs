using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace OpenAS2.Base
{

    [JsonObject()]
    public class RawInstruction
    {
        public virtual InstructionType Type { get; internal set; }
        public virtual IList<RawValue> Parameters { get; private set; }

        public RawInstruction(InstructionType type, List<RawValue>? parameters = null, bool reformList = true)
        {
            Type = type;
            parameters = parameters ?? new();
            Parameters = (reformList ? new List<RawValue>(parameters) : parameters).AsReadOnly();
        }

        public string Serialize()
        {
            List<string> s = new() { ((int)Type).ToString() };
            foreach (var val in Parameters)
                s.Add(val.Serialize());
            return JsonConvert.SerializeObject(s);
        }

        public override string ToString() { return ToString(null, null); }
        public string ToString(IList<ConstantEntry>? cp = null, RawInstruction? cas = null)
        {
            if (Type == InstructionType.ConstantPool)
            {
                if (Parameters.Count < 6)
                    return $"{Type}|{string.Join(", ", Parameters.Skip(1).Select(x => $"#{x.Integer}"))}";
                else
                    return $"{Type}|{Parameters[0]} Parameters: {string.Join(", ", Parameters.Skip(1).Take(3).Select(x => $"#{x.Integer}"))}...#{Parameters.Last().Integer}";
            }
            return $"{Type}|{string.Join(", ", Parameters.Select(x => x.ToString(cp, cas)))}";
        }

        public static RawInstruction Deserialize(string str)
        {
            var s = JsonConvert.DeserializeObject<List<string>>(str);
            var p = (InstructionType)int.Parse(s[0]);
            List<RawValue> pars = new();
            foreach (var val in s.Skip(1))
                pars.Add(RawValue.Deserialize(val));
            return new RawInstruction(p, pars);
        }

        public static RawInstruction CreateEnd() { return new(InstructionType.End, null, false); }

        // judgements

        [JsonIgnore]
        public bool IsEnd => Type == InstructionType.End;

        [JsonIgnore]
        public bool IsBranch => Type == InstructionType.BranchIfTrue || Type == InstructionType.EA_BranchIfFalse || Type == InstructionType.BranchAlways;
        [JsonIgnore]
        public bool IsBranchAlways => Type == InstructionType.BranchAlways;
        [JsonIgnore]
        public bool IsConditionalBranch => Type == InstructionType.BranchIfTrue || Type == InstructionType.EA_BranchIfFalse;

        [JsonIgnore]
        public bool IsEnumerate => Type == InstructionType.Enumerate || Type == InstructionType.Enumerate2;

        [JsonIgnore]
        public bool IsDefineFunction => Type == InstructionType.DefineFunction || Type == InstructionType.DefineFunction2;

    }
}
