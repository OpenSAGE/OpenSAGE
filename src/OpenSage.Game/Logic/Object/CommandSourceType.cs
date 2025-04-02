using System;
using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object;

public enum CommandSourceType
{
    [IniEnum("FROM_PLAYER")]
    FromPlayer = 0,

    [IniEnum("FROM_SCRIPT")]
    FromScript = 1,

    [IniEnum("FROM_AI")]
    FromAI = 2,

    /// <summary>
    /// Special rare command when the dozer originates a command to attack a
    /// mine. Mines are not AI-attackable, and it seems deceitful for the dozer
    /// to generate a player or script command.
    /// </summary>
    [IniEnum("FROM_DOZER")]
    FromDozer = 3,

    /// <summary>
    /// Special case: a weapon that can be chosen -- this is the default case (machine gun vs flashbang).
    /// </summary>
    [IniEnum("DEFAULT_SWITCH_WEAPON"), AddedIn(SageGame.CncGeneralsZeroHour)]
    DefaultSwitchWeapon = 4,
}
