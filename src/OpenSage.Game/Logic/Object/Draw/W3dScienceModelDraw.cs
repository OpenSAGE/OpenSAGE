using System.IO;
using OpenSage.Client;
using OpenSage.Data.Ini;
using OpenSage.FileFormats;

namespace OpenSage.Logic.Object
{
    public sealed class W3dScienceModelDraw : W3dModelDraw
    {
        internal W3dScienceModelDraw(W3dScienceModelDrawModuleData data, Drawable drawable, GameContext context)
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

    public sealed class W3dScienceModelDrawModuleData : W3dModelDrawModuleData
    {
        internal static W3dScienceModelDrawModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<W3dScienceModelDrawModuleData> FieldParseTable = W3dModelDrawModuleData.FieldParseTable
            .Concat(new IniParseTable<W3dScienceModelDrawModuleData>
            {
                { "RequiredScience", (parser, x) => x.RequiredScience = parser.ParseAssetReference() }
            });

        public string RequiredScience { get; private set; }

        internal override DrawModule CreateDrawModule(Drawable drawable, GameContext context)
        {
            return new W3dScienceModelDraw(this, drawable, context);
        }
    }
}
