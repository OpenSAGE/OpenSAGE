using System.IO;
using OpenSage.Content;
using OpenSage.Data.Ini;
using OpenSage.FileFormats;

namespace OpenSage.Logic.Object
{
    public sealed class EjectPilotDie : DieModule
    {
        // TODO

        internal override void Load(BinaryReader reader)
        {
            var version = reader.ReadVersion();
            if (version != 1)
            {
                throw new InvalidDataException();
            }

            base.Load(reader);
        }
    }

    /// <summary>
    /// Allows use of SoundEject and VoiceEject within UnitSpecificSounds section of the object.
    /// </summary>
    public sealed class EjectPilotDieModuleData : DieModuleData
    {
        internal static EjectPilotDieModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<EjectPilotDieModuleData> FieldParseTable = DieModuleData.FieldParseTable
            .Concat(new IniParseTable<EjectPilotDieModuleData>
            {
                { "GroundCreationList", (parser, x) => x.GroundCreationList = parser.ParseObjectCreationListReference() },
                { "AirCreationList", (parser, x) => x.AirCreationList = parser.ParseObjectCreationListReference() }
            });

        public LazyAssetReference<ObjectCreationList> GroundCreationList { get; private set; }
        public LazyAssetReference<ObjectCreationList> AirCreationList { get; private set; }

        internal override BehaviorModule CreateModule(GameObject gameObject, GameContext context)
        {
            return new EjectPilotDie();
        }
    }
}
