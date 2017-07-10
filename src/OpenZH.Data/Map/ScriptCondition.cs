namespace OpenZH.Data.Map
{
    public sealed class ScriptCondition : ScriptContent<ScriptCondition, ScriptConditionType>
    {
        
    }

    public enum ScriptConditionType : uint
    {
        False = 0,
        Counter = 1,
        True = 3,
        TimerExpired = 4,
        CameraMovementFinished = 9,
        NamedDestroyed = 15,
        PlayerHasCredits = 26,
        NamedSelected = 36,
        HasFinishedSpeech = 48,
        HasFinishedAudio = 49,
        EnemySighted = 51,
        UnitHealth = 52,
        BridgeBroken = 54,
        TeamObjectStatusPartial = 81,
        SkirmishPlayerFaction = 86,
        SkirmishSuppliesValueWithinDistance = 87,
        SkirmishPlayerHasBeenAttackedByPlayer = 97
    }
}
