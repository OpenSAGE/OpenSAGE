using System.IO;
using OpenSage.Data.Ini;
using OpenSage.FileFormats;

namespace OpenSage.Logic.Object
{
    public sealed class TransportAIUpdate : AIUpdate
    {
        internal TransportAIUpdate(GameObject gameObject, TransportAIUpdateModuleData moduleData)
            : base(gameObject, moduleData)
        {
        }

        internal override void Load(BinaryReader reader)
        {
            var version = reader.ReadVersion();
            if (version != 1)
            {
                throw new InvalidDataException();
            }

            base.Load(reader);

            var unknown = reader.ReadBytes(33);
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
