#nullable enable

using OpenSage.Client;
using OpenSage.Content;
using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    public sealed class W3dScienceModelDraw : W3dModelDraw
    {
        private readonly W3dScienceModelDrawModuleData _data;
        private readonly Drawable _drawable;
        private readonly GameContext _context;

        internal W3dScienceModelDraw(W3dScienceModelDrawModuleData data, Drawable drawable, GameContext context)
            : base(data, drawable, context)
        {
            _data = data;
            _drawable = drawable;
            _context = context;
        }

        internal override void Load(StatePersister reader)
        {
            reader.PersistVersion(1);

            reader.BeginObject("Base");
            base.Load(reader);
            reader.EndObject();
        }

        internal override void Update(in TimeInterval gameTime)
        {
            if (_data.RequiredScience is null || _context.Game.PlayerManager.LocalPlayer.HasScience(_data.RequiredScience.Value))
            {
                base.Update(in gameTime);
            }
        }
    }

    public sealed class W3dScienceModelDrawModuleData : W3dModelDrawModuleData
    {
        internal static W3dScienceModelDrawModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<W3dScienceModelDrawModuleData> FieldParseTable = W3dModelDrawModuleData.FieldParseTable
            .Concat(new IniParseTable<W3dScienceModelDrawModuleData>
            {
                { "RequiredScience", (parser, x) => x.RequiredScience = parser.ParseScienceReference() }
            });

        public LazyAssetReference<Science>? RequiredScience { get; private set; }

        internal override DrawModule CreateDrawModule(Drawable drawable, GameContext context)
        {
            return new W3dScienceModelDraw(this, drawable, context);
        }
    }
}
