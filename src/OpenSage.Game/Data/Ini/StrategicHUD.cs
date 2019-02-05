using OpenSage.Data.Ini.Parser;

namespace OpenSage.Data.Ini
{
    [AddedIn(SageGame.Bfme2)]
    public sealed class StrategicHUD
    {
        internal static StrategicHUD Parse(IniParser parser) => parser.ParseTopLevelBlock(FieldParseTable);

        private static readonly IniParseTable<StrategicHUD> FieldParseTable = new IniParseTable<StrategicHUD>
        {
            { "ArmyDetailsPanel", (parser, x) => x.ArmyDetailsPanel = ArmyDetailsPanel.Parse(parser) },
        };

        public ArmyDetailsPanel ArmyDetailsPanel { get; private set; }
    }

    public class ArmyDetailsPanel
    {
        internal static ArmyDetailsPanel Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<ArmyDetailsPanel> FieldParseTable = new IniParseTable<ArmyDetailsPanel>
        {
            { "BackButtonTooltip", (parser, x) => x.BackButtonTooltip = parser.ParseLocalizedStringKey() },
        };

        public string BackButtonTooltip { get; private set; }
    }
}
