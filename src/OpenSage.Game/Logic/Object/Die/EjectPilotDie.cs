using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// Allows use of SoundEject and VoiceEject within UnitSpecificSounds section of the object.
    /// </summary>
    public sealed class EjectPilotDie : ObjectBehavior
    {
        internal static EjectPilotDie Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<EjectPilotDie> FieldParseTable = new IniParseTable<EjectPilotDie>
        {
            { "DeathTypes", (parser, x) => x.DeathTypes = parser.ParseEnumBitArray<DeathType>() },
            { "ExemptStatus", (parser, x) => x.ExemptStatus = parser.ParseEnum<ObjectStatus>() },
            { "VeterancyLevels", (parser, x) => x.VeterancyLevels = parser.ParseEnumBitArray<VeterancyLevel>() },
            { "GroundCreationList", (parser, x) => x.GroundCreationList = parser.ParseAssetReference() },
            { "AirCreationList", (parser, x) => x.AirCreationList = parser.ParseAssetReference() }
        };

        public BitArray<DeathType> DeathTypes { get; private set; }
        public ObjectStatus ExemptStatus { get; private set; }
        public BitArray<VeterancyLevel> VeterancyLevels { get; private set; }
        public string GroundCreationList { get; private set; }
        public string AirCreationList { get; private set; }
    }
}
