using System;

namespace OpenSage;

public enum SageGame
{
    CncGenerals,
    CncGeneralsZeroHour,
    Bfme,
    Bfme2,
    Bfme2Rotwk,
    Cnc3,
    Cnc3KanesWrath,
    Ra3,
    Ra3Uprising,
    Cnc4,
}

public static class SageGameExtensions
{
    public static int LogicFramesPerSecond(this SageGame sageGame)
    {
        return sageGame switch
        {
            SageGame.CncGenerals or SageGame.CncGeneralsZeroHour => 30,
            SageGame.Bfme or SageGame.Bfme2 or SageGame.Bfme2Rotwk => 5,
            SageGame.Cnc3 or SageGame.Cnc3KanesWrath or SageGame.Ra3 or SageGame.Ra3Uprising or SageGame.Cnc4 =>
                throw new NotImplementedException(),
            _ => throw new ArgumentOutOfRangeException(nameof(sageGame), sageGame, null),
        };
    }

    public static float MsPerLogicFrame(this SageGame sageGame)
    {
        return 1000.0f / sageGame.LogicFramesPerSecond();
    }
}
