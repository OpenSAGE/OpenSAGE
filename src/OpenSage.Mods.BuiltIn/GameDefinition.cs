using System;
using System.Collections.Generic;
using System.Linq;
using OpenSage.Mods.Generals;
using OpenSage.Mods.Bfme;
using OpenSage.Mods.Bfme2;
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

        public static bool TryGetByName(string name, out IGameDefinition definition)
        {
            // TODO: Use a short identifier defined in IGameDefinition instead of stringified SageGame
            definition = All.FirstOrDefault(def =>
                string.Equals(def.Game.ToString(), name, StringComparison.InvariantCultureIgnoreCase));
            return definition != null;
        }

        static GameDefinition()
        {
            Games = new Dictionary<SageGame, IGameDefinition>
            {
                [SageGame.CncGenerals] = GeneralsDefinition.Instance,
                [SageGame.CncGeneralsZeroHour] = GeneralsZeroHourDefinition.Instance,
                [SageGame.Bfme] = BfmeDefinition.Instance,
                [SageGame.Bfme2] = Bfme2Definition.Instance,
                [SageGame.Bfme2Rotwk] = Bfme2RotwkDefinition.Instance,
                [SageGame.Cnc3] = Cnc3Definition.Instance,
                [SageGame.Cnc3KanesWrath] = Cnc3KanesWrathDefinition.Instance,
                [SageGame.Ra3] = Ra3Definition.Instance,
                [SageGame.Ra3Uprising] = Ra3UprisingDefinition.Instance,
                [SageGame.Cnc4] = Cnc4Definition.Instance
            };
        }
    }
}
