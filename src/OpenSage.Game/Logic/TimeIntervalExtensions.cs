namespace OpenSage.Logic;

public static class TimeIntervalExtensions
{
    public static double GetLogicFrameRelativeDeltaTime(this TimeInterval timeInterval, float msPerLogicFrame)
    {
        return timeInterval.DeltaTime.TotalMilliseconds / msPerLogicFrame;
    }
}
