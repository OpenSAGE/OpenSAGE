using System.Collections.Generic;
using System.Numerics;
using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;
using OpenSage.Mathematics;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme)]
    public sealed class HordeGarrisonContainModuleData : HordeTransportContainModuleData
    {
        internal static new HordeGarrisonContainModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        internal static new readonly IniParseTable<HordeGarrisonContainModuleData> FieldParseTable = HordeTransportContainModuleData.FieldParseTable
            .Concat(new IniParseTable<HordeGarrisonContainModuleData>
            {
                { "ContainMax", (parser, x) => x.ContainMax = parser.ParseInteger() },
                { "MaxHordeCapacity", (parser, x) => x.MaxHordeCapacity = parser.ParseInteger() },
                { "EntryPosition", (parser, x) => x.EntryPosition = parser.ParseVector3() },
                { "EntryOffset", (parser, x) => x.EntryOffset = parser.ParseVector3() },
                { "ExitOffset", (parser, x) => x.ExitOffset = parser.ParseVector3() },
                { "KillPassengersOnDeath", (parser, x) => x.KillPassengersOnDeath = parser.ParseBoolean() }
            });

        public int ContainMax { get; private set; }
        public int MaxHordeCapacity { get; private set; }
        public Vector3 EntryPosition { get; private set; }
        public Vector3 EntryOffset { get; private set; }
        public Vector3 ExitOffset { get; private set; }
        public bool KillPassengersOnDeath { get; private set; }
    }
}
