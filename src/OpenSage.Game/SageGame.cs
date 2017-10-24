using System.Collections.Generic;

namespace OpenSage
{
    public enum SageGame
    {
        CncGenerals,
        CncGeneralsZeroHour,
        BattleForMiddleEarth
    }

    public static class SageGames
    {
        public static IEnumerable<SageGame> GetAll()
        {
            yield return SageGame.CncGenerals;
            yield return SageGame.CncGeneralsZeroHour;
        }
    }
}
