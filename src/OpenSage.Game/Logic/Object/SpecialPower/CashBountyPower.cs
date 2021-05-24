using System.IO;
using OpenSage.Data.Ini;
using OpenSage.FileFormats;
using OpenSage.Mathematics;

namespace OpenSage.Logic.Object
{
    public sealed class CashBountyPower : SpecialPowerModule
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

    public sealed class CashBountyPowerModuleData : SpecialPowerModuleData
    {
        internal static new CashBountyPowerModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<CashBountyPowerModuleData> FieldParseTable = SpecialPowerModuleData.FieldParseTable
            .Concat(new IniParseTable<CashBountyPowerModuleData>
            {
                { "Bounty", (parser, x) => x.Bounty = parser.ParsePercentage() },
            });

        public Percentage Bounty { get; private set; }

        internal override BehaviorModule CreateModule(GameObject gameObject, GameContext context)
        {
            return new CashBountyPower();
        }
    }
}
