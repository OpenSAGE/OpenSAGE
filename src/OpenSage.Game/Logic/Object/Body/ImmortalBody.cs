using System.IO;
using OpenSage.Data.Ini;
using OpenSage.FileFormats;

namespace OpenSage.Logic.Object
{
    public sealed class ImmortalBody : ActiveBody
    {
        internal ImmortalBody(GameObject gameObject, ImmortalBodyModuleData moduleData)
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
        }
    }

    /// <summary>
    /// Prevents the object from dying or taking damage.
    /// </summary>
    public sealed class ImmortalBodyModuleData : ActiveBodyModuleData
    {
        internal static new ImmortalBodyModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<ImmortalBodyModuleData> FieldParseTable = ActiveBodyModuleData.FieldParseTable
            .Concat(new IniParseTable<ImmortalBodyModuleData>());

        internal override BodyModule CreateBodyModule(GameObject gameObject)
        {
            return new ImmortalBody(gameObject, this);
        }
    }
}
