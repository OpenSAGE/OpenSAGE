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

        public int ContainMax { get; internal set; }
        public int MaxHordeCapacity { get; internal set; }
        public Vector3 EntryPosition { get; internal set; }
        public Vector3 EntryOffset { get; internal set; }
        public Vector3 ExitOffset { get; internal set; }
        public bool KillPassengersOnDeath { get; internal set; }
    }
}
