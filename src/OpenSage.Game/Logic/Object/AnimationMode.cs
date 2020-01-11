using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    public enum AnimationMode
    {
        [IniEnum("ONCE")]
        Once,

        [IniEnum("ONCE_BACKWARDS")]
        OnceBackwards,

        [IniEnum("LOOP")]
        Loop,

        [IniEnum("LOOP_BACKWARDS")]
        LoopBackwards,

        [IniEnum("PING_PONG")]
        PingPong,

        [IniEnum("MANUAL")]
        Manual
    }
}
