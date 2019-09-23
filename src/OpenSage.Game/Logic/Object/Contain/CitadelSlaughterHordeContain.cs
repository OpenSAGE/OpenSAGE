using System.Numerics;
using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme2)]
    public sealed class CitadelSlaughterHordeContainModuleData : SlaughterHordeContainModuleData
    {
        internal static new CitadelSlaughterHordeContainModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        internal static new readonly IniParseTable<CitadelSlaughterHordeContainModuleData> FieldParseTable = SlaughterHordeContainModuleData.FieldParseTable
            .Concat(new IniParseTable<CitadelSlaughterHordeContainModuleData>
            {
                { "AllowOwnPlayerInsideOverride", (parser, x) => x.AllowOwnPlayerInsideOverride = parser.ParseBoolean() },
                { "StatusForRingEntry", (parser, x) => x.StatusForRingEntry = parser.ParseEnum<ObjectStatus>() },
                { "UpgradeForRingEntry", (parser, x) => x.UpgradeForRingEntry = parser.ParseAssetReference() },
                { "ObjectToDestroyForRingEntry", (parser, x) => x.ObjectToDestroyForRingEntry = ObjectFilter.Parse(parser) },
                { "FXForRingEntry", (parser, x) => x.FXForRingEntry = parser.ParseAssetReference() }
            });

        public bool AllowOwnPlayerInsideOverride { get; private set; }
        public ObjectStatus StatusForRingEntry { get; private set; }
        public string UpgradeForRingEntry { get; private set; }
        public ObjectFilter ObjectToDestroyForRingEntry { get; private set; }
        public string FXForRingEntry { get; private set; }
    }
}
