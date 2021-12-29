using OpenSage.Client;
using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    public sealed class W3dOverlordTankDraw : W3dTankDraw
    {
        internal W3dOverlordTankDraw(W3dOverlordTankDrawModuleData data, Drawable drawable, GameContext context)
            : base(data, drawable, context)
        {
        }

        internal override void Load(StatePersister reader)
        {
            reader.ReadVersion(1);

            base.Load(reader);
        }
    }

    /// <summary>
    /// Special-case draw module which is interdependent with the W3DDependencyModelDraw module.
    /// Allows other objects to be attached to this object through use of AttachToBoneInContainer 
    /// logic.
    /// </summary>
    public sealed class W3dOverlordTankDrawModuleData : W3dTankDrawModuleData
    {
        internal static new W3dOverlordTankDrawModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<W3dOverlordTankDrawModuleData> FieldParseTable = W3dTankDrawModuleData.FieldParseTable
            .Concat(new IniParseTable<W3dOverlordTankDrawModuleData>());

        internal override DrawModule CreateDrawModule(Drawable drawable, GameContext context)
        {
            return new W3dOverlordTankDraw(this, drawable, context);
        }
    }
}
