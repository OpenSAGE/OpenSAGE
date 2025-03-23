using OpenSage.Logic.Object;
using OpenSage.Mathematics;

namespace OpenSage.Utilities;

public static class RandomExtensions
{
    public static LogicFrameSpan NextLogicFrameSpan(
        this RandomValue random,
        LogicFrameSpan min,
        LogicFrameSpan max)
    {
        var value = random.Next((int)min.Value, (int)max.Value);
        return new LogicFrameSpan((uint)value);
    }

    public static bool NextBoolean(this RandomValue random)
    {
        return random.Next(0, 1) == 1;
    }
}
