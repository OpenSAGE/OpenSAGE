using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    public sealed class TransportAIUpdate : AIUpdate
    {
        internal TransportAIUpdate(GameObject gameObject, TransportAIUpdateModuleData moduleData)
            : base(gameObject, moduleData)
        {
        }

        internal override void Load(StatePersister reader)
        {
            reader.PersistVersion(1);

            base.Load(reader);
        }
    }

    /// <summary>
    /// Used on TRANSPORT KindOfs that contain other objects.
    /// </summary>
    public sealed class TransportAIUpdateModuleData : AIUpdateModuleData
    {
        internal new static TransportAIUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private new static readonly IniParseTable<TransportAIUpdateModuleData> FieldParseTable = AIUpdateModuleData.FieldParseTable
            .Concat(new IniParseTable<TransportAIUpdateModuleData>());

        internal override AIUpdate CreateAIUpdate(GameObject gameObject)
        {
            return new TransportAIUpdate(gameObject, this);
        }
    }
}
