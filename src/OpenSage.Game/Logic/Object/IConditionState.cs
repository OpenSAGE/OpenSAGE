using OpenSage.Mathematics;

namespace OpenSage.Logic.Object
{
    public interface IConditionState
    {
        BitArray<ModelConditionFlag> ConditionFlags { get; }
    }
}
