using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// Allows use of SoundEject and VoiceEject within UnitSpecificSounds section of the object.
    /// </summary>
    public sealed class EjectPilotDieModuleData : DieModuleData
    {
        internal static EjectPilotDieModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<EjectPilotDieModuleData> FieldParseTable = DieModuleData.FieldParseTable
            .Concat(new IniParseTable<EjectPilotDieModuleData>
            {
                { "GroundCreationList", (parser, x) => x.GroundCreationList = parser.ParseAssetReference() },
                { "AirCreationList", (parser, x) => x.AirCreationList = parser.ParseAssetReference() }
            });

        public string GroundCreationList { get; private set; }
        public string AirCreationList { get; private set; }
    }
}
