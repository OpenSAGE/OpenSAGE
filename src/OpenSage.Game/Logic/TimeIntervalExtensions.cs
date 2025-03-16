namespace OpenSage.Logic;

public static class TimeIntervalExtensions
{
    public static double GetLogicFrameRelativeDeltaTime(this TimeInterval timeInterval)
    {
        return timeInterval.DeltaTime.TotalMilliseconds / GameEngine.LogicUpdateInterval;
    }
}
