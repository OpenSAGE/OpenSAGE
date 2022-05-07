using OpenSage.Client;
using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    public sealed class W3dScienceModelDraw : W3dModelDraw
    {
        internal W3dScienceModelDraw(W3dScienceModelDrawModuleData data, Drawable drawable, GameContext context)
            : base(data, drawable, context)
        {
        }

        internal override void Load(StatePersister reader)
        {
            reader.PersistVersion(1);

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
