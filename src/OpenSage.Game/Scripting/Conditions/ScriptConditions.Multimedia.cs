namespace OpenSage.Scripting
{
    partial class ScriptConditions
    {
        [ScriptCondition(ScriptConditionType.MusicTrackHasCompleted, "Multimedia/Finished/Music track has completed some number of times", "'{0}' has completed at least {1} times. (NOTE: This can only be used to start other music. USING THIS SCRIPT IN ANY OTHER WAY WILL CAUSE REPLAYS TO NOT WORK.)")]
        public static bool MusicTrackHasCompleted(ScriptExecutionContext context, [ScriptArgumentType(ScriptArgumentType.MusicName)] string trackName, [ScriptArgumentType(ScriptArgumentType.Integer)] int count)
        {
            return context.Scene.Audio.GetFinishedCount(trackName) >= count;
        }
    }
}
