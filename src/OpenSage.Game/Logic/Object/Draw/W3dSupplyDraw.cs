using System.IO;
using OpenSage.Client;
using OpenSage.Data.Ini;
using OpenSage.FileFormats;

namespace OpenSage.Logic.Object
{
    public sealed class W3dSupplyDraw : W3dModelDraw
    {
        internal W3dSupplyDraw(W3dSupplyDrawModuleData data, Drawable drawable, GameContext context)
            : base(data, drawable, context)
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

    public sealed class W3dSupplyDrawModuleData : W3dModelDrawModuleData
    {
        internal static W3dSupplyDrawModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<W3dSupplyDrawModuleData> FieldParseTable = W3dModelDrawModuleData.FieldParseTable
            .Concat(new IniParseTable<W3dSupplyDrawModuleData>
            {
                { "SupplyBonePrefix", (parser, x) => x.SupplyBonePrefix = parser.ParseString() }
            });

        public string SupplyBonePrefix { get; private set; }

        internal override DrawModule CreateDrawModule(Drawable drawable, GameContext context)
        {
            return new W3dSupplyDraw(this, drawable, context);
        }
    }
}
