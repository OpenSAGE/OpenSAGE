using System.Collections.Generic;
using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme)]
    public sealed class DetachableRiderUpdateModuleData : UpdateModuleData
    {
        internal static DetachableRiderUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<DetachableRiderUpdateModuleData> FieldParseTable = new IniParseTable<DetachableRiderUpdateModuleData>
            {
                { "RiderSubObjects", (parser, x) => x.RiderSubObjects = parser.ParseAssetReferenceArray() },
                { "RiderlessWeaponSlot", (parser, x) => x.RiderlessWeaponSlot = parser.ParseEnum<WeaponSlot>() },
                { "RiderlessHordeFlees", (parser, x) => x.RiderlessHordeFlees = parser.ParseBoolean() },
                { "DeathEntry", (parser, x) => x.DeathEntry = DeathEntry.Parse(parser) },
            };


        public string[] RiderSubObjects { get; private set; }
        public WeaponSlot RiderlessWeaponSlot { get; private set; }
        public bool RiderlessHordeFlees { get; private set; }
        public DeathEntry DeathEntry { get; private set; }
    }

    public sealed class DeathEntry
    {
        internal static DeathEntry Parse(IniParser parser)
        {
            return new DeathEntry
            {
                AnimationState = parser.ParseAttributeIdentifier("AnimState"),
                AnimationTime = parser.ParseAttributeInteger("AnimTime")
            };
        }

        public string AnimationState { get; private set; }
        public int AnimationTime { get; private set; }
    }
}
