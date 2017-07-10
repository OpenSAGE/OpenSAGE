
namespace OpenZH.Data.Map
{
    public sealed class ScriptAction : ScriptContent<ScriptAction, ScriptActionType>
    {
        
    }

    public enum ScriptActionType : uint
    {
        DebugMessageBox = 0,
        SetFlag = 1,
        NoOp = 5,
        PlaySoundEffect = 7,
        EnableScript = 8,
        DecrementCounter = 16,
        NamedDamage = 70,
        NamedKill = 73,
        DebugString = 101,
        CameraFadeAdd = 121,
        SetRandomTimer = 147,
        SetRandomMSecTimer = 148,
        StopTimer = 149,
        AudioOverrideVolumeType = 220,
        SetCaveIndex = 224,
        PlayerRepairNamedStructure = 253
    }
}
