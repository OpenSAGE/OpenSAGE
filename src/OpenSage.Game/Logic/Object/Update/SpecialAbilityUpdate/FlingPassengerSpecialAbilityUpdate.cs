using System.Numerics;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme2)]
    public sealed class FlingPassengerSpecialAbilityUpdateModuleData : UpdateModuleData
    {
        internal static FlingPassengerSpecialAbilityUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<FlingPassengerSpecialAbilityUpdateModuleData> FieldParseTable = new IniParseTable<FlingPassengerSpecialAbilityUpdateModuleData>
        {
           { "SpecialPowerTemplate", (parser, x) => x.SpecialPowerTemplate = parser.ParseString() },
           { "UnpackTime", (parser, x) => x.UnpackTime = parser.ParseInteger() },
           { "FlingPassengerVelocity", (parser, x) => x.FlingPassengerVelocity = parser.ParseVector3() },
           { "FlingPassengerLandingWarhead", (parser, x) => x.FlingPassengerLandingWarhead = parser.ParseAssetReference() },
           { "PackTime", (parser, x) => x.PackTime = parser.ParseInteger() },
           { "CustomAnimAndDuration", (parser, x) => x.GetCustomAnimAndDuration = AnimAndDuration.Parse(parser) },
           { "MustFinishAbility", (parser, x) => x.MustFinishAbility = parser.ParseBoolean() }
        };

        public string SpecialPowerTemplate { get; private set; }
        public int UnpackTime { get; private set; }
        public Vector3 FlingPassengerVelocity { get; private set; }
        public string FlingPassengerLandingWarhead { get; private set; }
        public int PackTime { get; private set; }
        public AnimAndDuration GetCustomAnimAndDuration { get; private set; }

        [AddedIn(SageGame.Bfme2Rotwk)]
        public bool MustFinishAbility { get; private set; }
    }
}
