using System.Collections.Generic;
using OpenSage.Mathematics;

namespace OpenSage.Logic.Object
{
    public interface IConditionState
    {
        List<BitArray<ModelConditionFlag>> ConditionFlags { get; }
    }
}
