using System.IO;
using OpenSage.Data.Ini;
using OpenSage.FileFormats;

namespace OpenSage.Logic.Object
{
    public sealed class StructureBody : ActiveBody
    {
        internal StructureBody(GameObject gameObject, StructureBodyModuleData moduleData)
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

            var unknown = reader.ReadUInt32();
            if (unknown != 0)
            {
                throw new InvalidDataException();
            }
        }
    }

    /// <summary>
    /// Used by objects with STRUCTURE and IMMOBILE KindOfs defined.
    /// </summary>
    public sealed class StructureBodyModuleData : ActiveBodyModuleData
    {
        internal static new StructureBodyModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<StructureBodyModuleData> FieldParseTable = ActiveBodyModuleData.FieldParseTable
            .Concat(new IniParseTable<StructureBodyModuleData>());

        internal override BodyModule CreateBodyModule(GameObject gameObject)
        {
            return new StructureBody(gameObject, this);
        }
    }
}
