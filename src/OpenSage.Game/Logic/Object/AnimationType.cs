using System;
using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme)]
    public enum AnimationType
    {
        None = 0,

        [IniEnum("FRONTKICKONBUILDING")]
        FrontKickOnBuilding,
        [IniEnum("IDLE")]
        Idle,
        [IniEnum("MOVING")]
        Moving,
        [IniEnum("STAND")]
        Stand,    
    }
}
