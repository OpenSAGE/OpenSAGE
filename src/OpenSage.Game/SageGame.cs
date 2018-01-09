using System.Collections.Generic;

namespace OpenSage
{
    public enum SageGame
    {
        CncGenerals,
        CncGeneralsZeroHour,
        BattleForMiddleEarth,
        BattleForMiddleEarthII,
        Cnc3,
        Cnc3KanesWrath
    }

    public static class SageGames
    {
        public static IEnumerable<SageGame> GetAll()
        {
            yield return SageGame.CncGenerals;
            yield return SageGame.CncGeneralsZeroHour;
            yield return SageGame.BattleForMiddleEarth;
            yield return SageGame.BattleForMiddleEarthII;
            yield return SageGame.Cnc3;
            yield return SageGame.Cnc3KanesWrath;
        }
    }
}
