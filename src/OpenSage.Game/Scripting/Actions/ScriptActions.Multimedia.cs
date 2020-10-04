namespace OpenSage.Scripting
{
    partial class ScriptActions
    {
        [ScriptAction(ScriptActionType.MusicSetTrack, "Multimedia/Music/Play a music track", "Play '{0}' using fadeout ({1}) and fadein ({2})")]
        public static void MusicSetTrack(ScriptExecutionContext context, [ScriptArgumentType(ScriptArgumentType.MusicName)] string trackName, bool fadeOut, bool fadeIn)
        {
            context.Scene.Audio.PlayMusicTrack(context.Scene.AssetLoadContext.AssetStore.MusicTracks.GetByName(trackName), fadeIn, fadeOut);
        }
    }
}
