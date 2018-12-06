using System.Numerics;
using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme)]
    public sealed class SlaughterHordeContainModuleData : UpgradeModuleData
    {
        internal static SlaughterHordeContainModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<SlaughterHordeContainModuleData> FieldParseTable = UpgradeModuleData.FieldParseTable
            .Concat(new IniParseTable<SlaughterHordeContainModuleData>
            {
                { "PassengerFilter", (parser, x) => x.PassengerFilter = ObjectFilter.Parse(parser) },
                { "ObjectStatusOfContained", (parser, x) => x.ObjectStatusOfContained = parser.ParseEnumBitArray<ObjectStatus>() },
                { "CashBackPercent", (parser, x) => x.CashBackPercent = parser.ParsePercentage() },
                { "ContainMax", (parser, x) => x.ContainMax = parser.ParseInteger() },
                { "MaxHordeCapacity", (parser, x) => x.MaxHordeCapacity = parser.ParseInteger() },
                { "AllowAlliesInside", (parser, x) => x.AllowAlliesInside = parser.ParseBoolean() },
                { "AllowEnemiesInside", (parser, x) => x.AllowEnemiesInside = parser.ParseBoolean() },
                { "AllowNeutralInside", (parser, x) => x.AllowNeutralInside = parser.ParseBoolean() },
                { "EnterSound", (parser, x) => x.EnterSound = parser.ParseAssetReference() },
                { "EntryOffset", (parser, x) => x.EntryOffset = parser.ParseVector3() },
                { "ExitOffset", (parser, x) => x.ExitOffset = parser.ParseVector3() },
            });

        public ObjectFilter PassengerFilter { get; private set; }
        public BitArray<ObjectStatus> ObjectStatusOfContained { get; private set; }
        public float CashBackPercent { get; private set; }
        public int ContainMax { get; private set; }
        public int MaxHordeCapacity { get; private set; }
        public bool AllowAlliesInside { get; private set; }
        public bool AllowEnemiesInside { get; private set; }
        public bool AllowNeutralInside { get; private set; }
        public string EnterSound { get; private set; }
        public Vector3 EntryOffset { get; private set; }
        public Vector3 ExitOffset { get; private set; }
    }
}
