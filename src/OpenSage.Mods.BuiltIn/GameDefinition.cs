using System.Collections.Generic;

using OpenSage.Mods.Generals;
using OpenSage.Mods.BFME;
using OpenSage.Mods.BfmeII;
using OpenSage.Mods.Cnc4;
using OpenSage.Mods.CnC3;
using OpenSage.Mods.Ra3;

namespace OpenSage.Mods.BuiltIn
{
    public static class GameDefinition
    {
        private static readonly Dictionary<SageGame, IGameDefinition> Games;

        public static IEnumerable<IGameDefinition> All => Games.Values;
        public static IGameDefinition FromGame(SageGame game) => Games[game];

        static GameDefinition()
        {
            Games = new Dictionary<SageGame, IGameDefinition>
            {
                [SageGame.CncGenerals] = GeneralsDefinition.Instance,
                [SageGame.CncGeneralsZeroHour] = GeneralsZeroHourDefinition.Instance,
                [SageGame.BattleForMiddleEarth] = BfmeDefinition.Instance,
                [SageGame.BattleForMiddleEarthII] = BfmeIIDefinition.Instance,
                [SageGame.Cnc3] = Cnc3Definition.Instance,
                [SageGame.Cnc3KanesWrath] = Cnc3KanesWrathDefinition.Instance,
                [SageGame.Ra3] = Ra3Definition.Instance,
                [SageGame.Ra3Uprising] = Ra3UprisingDefinition.Instance,
                [SageGame.Cnc4] = Cnc4Definition.Instance
            };
        }
    }
}
