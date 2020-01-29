using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    public sealed class TransportAIUpdate : AIUpdate
    {
        internal TransportAIUpdate(GameObject gameObject, TransportAIUpdateModuleData moduleData)
            : base(gameObject, moduleData)
        {
        }
    }

    /// <summary>
    /// Used on TRANSPORT KindOfs that contain other objects.
    /// </summary>
    public sealed class TransportAIUpdateModuleData : AIUpdateModuleData
    {
        internal static new TransportAIUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<TransportAIUpdateModuleData> FieldParseTable = AIUpdateModuleData.FieldParseTable
            .Concat(new IniParseTable<TransportAIUpdateModuleData>());

        internal override AIUpdate CreateAIUpdate(GameObject gameObject)
        {
            return new TransportAIUpdate(gameObject, this);
        }
    }
}
