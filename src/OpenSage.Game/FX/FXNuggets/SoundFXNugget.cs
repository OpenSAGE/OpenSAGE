using OpenSage.Audio;
using OpenSage.Content;
using OpenSage.Data.Ini;

namespace OpenSage.FX
{
    public sealed class SoundFXNugget : FXNugget
    {
        internal static SoundFXNugget Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<SoundFXNugget> FieldParseTable = FXNuggetFieldParseTable.Concat(new IniParseTable<SoundFXNugget>
        {
            { "Name", (parser, x) => x.Value = parser.ParseAudioEventReference() }
        });

        public LazyAssetReference<BaseAudioEventInfo> Value { get; private set; }

        internal override void Execute(FXListExecutionContext context)
        {
            context.GameContext.AudioSystem.PlayAudioEvent(Value.Value);
        }
    }
}
