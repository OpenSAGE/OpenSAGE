using System.IO;
using OpenSage.Data.Ini;
using OpenSage.FileFormats;

namespace OpenSage.Logic.Object
{
    public sealed class ConvertToCarBombCrateCollide : CrateCollide
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
    /// Triggers use of CARBOMB WeaponSet Condition of the hijacked object and turns it to a 
    /// suicide unit unless given with a different weapon.
    /// </summary>
    public sealed class ConvertToCarBombCrateCollideModuleData : CrateCollideModuleData
    {
        internal static ConvertToCarBombCrateCollideModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<ConvertToCarBombCrateCollideModuleData> FieldParseTable = CrateCollideModuleData.FieldParseTable
            .Concat(new IniParseTable<ConvertToCarBombCrateCollideModuleData>());

        internal override BehaviorModule CreateModule(GameObject gameObject, GameContext context)
        {
            return new ConvertToCarBombCrateCollide();
        }
    }
}
