using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// Allows the use of the DOOR_1_WAITING_OPEN, DOOR_1_CLOSING, DOOR_1_OPENING model condition states.
    /// </summary>
    public sealed class MissileLauncherBuildingUpdateModuleData : UpdateModuleData
    {
        internal static MissileLauncherBuildingUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<MissileLauncherBuildingUpdateModuleData> FieldParseTable = new IniParseTable<MissileLauncherBuildingUpdateModuleData>
        {
            { "SpecialPowerTemplate", (parser, x) => x.SpecialPowerTemplate = parser.ParseAssetReference() },

            { "DoorOpenTime", (parser, x) => x.DoorOpenTime = parser.ParseInteger() },
            { "DoorWaitOpenTime", (parser, x) => x.DoorWaitOpenTime = parser.ParseInteger() },
            { "DoorCloseTime", (parser, x) => x.DoorCloseTime = parser.ParseInteger() },

            { "DoorOpeningFX", (parser, x) => x.DoorOpeningFX = parser.ParseAssetReference() },
            { "DoorWaitingToCloseFX", (parser, x) => x.DoorWaitingToCloseFX = parser.ParseAssetReference() },

            { "DoorOpenIdleAudio", (parser, x) => x.DoorOpenIdleAudio = parser.ParseAssetReference() }
        };

        public string SpecialPowerTemplate { get; private set; }

        public int DoorOpenTime { get; private set; }
        public int DoorWaitOpenTime { get; private set; }
        public int DoorCloseTime { get; private set; }

        public string DoorOpeningFX { get; private set; }
        public string DoorWaitingToCloseFX { get; private set; }

        public string DoorOpenIdleAudio { get; private set; }
    }
}
