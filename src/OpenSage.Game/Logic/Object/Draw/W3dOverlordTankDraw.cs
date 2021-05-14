using System.IO;
using OpenSage.Client;
using OpenSage.Data.Ini;
using OpenSage.FileFormats;

namespace OpenSage.Logic.Object
{
    public sealed class W3dOverlordTankDraw : W3dTankDraw
    {
        internal W3dOverlordTankDraw(W3dOverlordTankDrawModuleData data, Drawable drawable, GameContext context)
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
