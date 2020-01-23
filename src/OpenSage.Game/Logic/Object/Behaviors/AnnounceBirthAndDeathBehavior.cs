using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme2)]
    public sealed class AnnounceBirthAndDeathBehaviorModuleData : BehaviorModuleData
    {
        internal static AnnounceBirthAndDeathBehaviorModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<AnnounceBirthAndDeathBehaviorModuleData> FieldParseTable = new IniParseTable<AnnounceBirthAndDeathBehaviorModuleData>();
    }
}
