using System.IO;
using OpenSage.Data.Ini;
using OpenSage.FileFormats;

namespace OpenSage.Logic.Object
{
    public sealed class BoneFXDamage : DamageModule
    {
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
    /// Enables use of BoneFXUpdate module on this object where an additional dynamic FX logic can 
    /// be used.
    /// </summary>
    public sealed class BoneFXDamageModuleData : DamageModuleData
    {
        internal static BoneFXDamageModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<BoneFXDamageModuleData> FieldParseTable = new IniParseTable<BoneFXDamageModuleData>();

        internal override BehaviorModule CreateModule(GameObject gameObject, GameContext context)
        {
            return new BoneFXDamage();
        }
    }
}
