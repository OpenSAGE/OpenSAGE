using System.Collections.Generic;
using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme)]
    public class HordeSiegeEngineContainModuleData : SiegeEngineContainModuleData
    {
        internal static new HordeSiegeEngineContainModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        internal static readonly new IniParseTable<HordeSiegeEngineContainModuleData> FieldParseTable = SiegeEngineContainModuleData.FieldParseTable
            .Concat(new IniParseTable<HordeSiegeEngineContainModuleData>
            {
                { "EnterSound", (parser, x) => x.EnterSound = parser.ParseAssetReference() },
                { "ExitSound", (parser, x) => x.ExitSound = parser.ParseAssetReference() },
                { "FadeFilter", (parser, x) => x.FadeFilter = ObjectFilter.Parse(parser) },
                { "FadePassengerOnEnter", (parser, x) => x.FadePassengerOnEnter = parser.ParseBoolean() },
                { "EnterFadeTime", (parser, x) => x.EnterFadeTime = parser.ParseInteger() },
                { "FadePassengerOnExit", (parser, x) => x.FadePassengerOnExit = parser.ParseBoolean() },
                { "ExitFadeTime", (parser, x) => x.ExitFadeTime = parser.ParseInteger() },
                { "FadeReverse", (parser, x) => x.FadeReverse = parser.ParseBoolean() },
                { "UpgradeCreationTrigger", (parser, x) => x.UpgradeCreationTriggers.Add(UpgradeCreationTrigger.Parse(parser)) },
            });

        public string EnterSound { get; private set; }
        public string ExitSound { get; private set; }
        public ObjectFilter FadeFilter { get; private set; }
        public bool FadePassengerOnEnter { get; private set; }
        public int EnterFadeTime { get; private set; }
        public bool FadePassengerOnExit { get; private set; }
        public int ExitFadeTime { get; private set; }
        public bool FadeReverse { get; private set; }

        [AddedIn(SageGame.Bfme2Rotwk)]
        public List<UpgradeCreationTrigger> UpgradeCreationTriggers { get; } = new List<UpgradeCreationTrigger>();
    }
}
