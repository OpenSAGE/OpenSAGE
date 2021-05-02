using System.IO;
using OpenSage.Data.Ini;
using OpenSage.FileFormats;

namespace OpenSage.Logic.Object
{
    public sealed class HordeUpdate : UpdateModule
    {
        internal override void Load(BinaryReader reader)
        {
            var version = reader.ReadVersion();
            if (version != 1)
            {
                throw new InvalidDataException();
            }

            base.Load(reader);

            // TODO
        }
    }

    /// <summary>
    /// Hardcoded to apply the following textures to objects that are affected by this module: 
    /// EXHorde, EXHorde_UP, EXHordeB, EXHordeB_UP.
    /// </summary>
    public sealed class HordeUpdateModuleData : UpdateModuleData
    {
        internal static HordeUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<HordeUpdateModuleData> FieldParseTable = new IniParseTable<HordeUpdateModuleData>
        {
            { "RubOffRadius", (parser, x) => x.RubOffRadius = parser.ParseInteger() },
            { "UpdateRate", (parser, x) => x.UpdateRate = parser.ParseInteger() },
            { "Radius", (parser, x) => x.Radius = parser.ParseInteger() },
            { "KindOf", (parser, x) => x.KindOf = parser.ParseEnum<ObjectKinds>() },
            { "AlliesOnly", (parser, x) => x.AlliesOnly = parser.ParseBoolean() },
            { "ExactMatch", (parser, x) => x.ExactMatch = parser.ParseBoolean() },
            { "Count", (parser, x) => x.Count = parser.ParseInteger() },
            { "Action", (parser, x) => x.Action = parser.ParseEnum<WeaponBonusType>() }
        };

        public int RubOffRadius { get; private set; }
        public int UpdateRate { get; private set; }
        public int Radius { get; private set; }
        public ObjectKinds KindOf { get; private set; }
        public bool AlliesOnly { get; private set; }
        public bool ExactMatch { get; private set; }
        public int Count { get; private set; }
        public WeaponBonusType Action { get; private set; }

        internal override BehaviorModule CreateModule(GameObject gameObject, GameContext context)
        {
            return new HordeUpdate();
        }
    }
}
