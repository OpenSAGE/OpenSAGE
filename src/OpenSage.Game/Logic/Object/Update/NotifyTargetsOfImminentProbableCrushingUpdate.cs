using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme2)]
    public sealed class NotifyTargetsOfImminentProbableCrushingUpdateModuleData : UpdateModuleData
    {
        internal static NotifyTargetsOfImminentProbableCrushingUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<NotifyTargetsOfImminentProbableCrushingUpdateModuleData> FieldParseTable = new IniParseTable<NotifyTargetsOfImminentProbableCrushingUpdateModuleData>
        {
        };

    }
}
