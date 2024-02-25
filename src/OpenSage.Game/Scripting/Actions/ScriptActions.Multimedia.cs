namespace OpenSage.Scripting
{
    partial class ScriptActions
    {
        [ScriptAction(ScriptActionType.MusicSetTrack, "Multimedia/Music/Play a music track", "Play '{0}' using fadeout ({1}) and fadein ({2})")]
        public static void MusicSetTrack(ScriptExecutionContext context, [ScriptArgumentType(ScriptArgumentType.MusicName)] string trackName, bool fadeOut, bool fadeIn)
        {
            context.Scene.Audio.PlayMusicTrack(context.Scene.AssetLoadContext.AssetStore.MusicTracks.GetByName(trackName), fadeIn, fadeOut);
        }

        [ScriptAction(ScriptActionType.SpeechPlay, "Play a speech file.", "'{0}' plays, allowing overlap {1} (true to allow, false to disallow)")]
        public static void PlaySpeechFile(ScriptExecutionContext context, [ScriptArgumentType(ScriptArgumentType.SpeechName)] string speechName, bool overlap)
        {
            context.Scene.Audio.PlayAudioEvent(context.Scene.AssetLoadContext.AssetStore.DialogEvents.GetByName(speechName));
        }
    }
}
