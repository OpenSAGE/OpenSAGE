using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    public class AIUpdateModuleData : UpdateModuleData
    {
        internal static AIUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        internal static readonly IniParseTable<AIUpdateModuleData> FieldParseTable = new IniParseTable<AIUpdateModuleData>
        {
            { "Turret", (parser, x) => x.Turret = TurretAIData.Parse(parser) },
            { "AltTurret", (parser, x) => x.AltTurret = TurretAIData.Parse(parser) },
            { "TurretsLinked", (parser, x) => x.TurretsLinked = parser.ParseBoolean() },
            { "AutoAcquireEnemiesWhenIdle", (parser, x) => x.AutoAcquireEnemiesWhenIdle = parser.ParseEnumBitArray<AutoAcquireEnemiesType>() },
            { "MoodAttackCheckRate", (parser, x) => x.MoodAttackCheckRate = parser.ParseInteger() },
            { "ForbidPlayerCommands", (parser, x) => x.ForbidPlayerCommands = parser.ParseBoolean() },
            { "AILuaEventsList", (parser, x) => x.AILuaEventsList = parser.ParseString() },
            { "HoldGroundCloseRangeDistance", (parser, x) => x.HoldGroundCloseRangeDistance = parser.ParseInteger() },
            { "MinCowerTime", (parser, x) => x.MinCowerTime = parser.ParseInteger() },
            { "MaxCowerTime", (parser, x) => x.MaxCowerTime = parser.ParseInteger() },
            { "CanAttackWhileContained", (parser, x) => x.CanAttackWhileContained = parser.ParseBoolean() },

        };

        /// <summary>
        /// Allows the use of TurretMoveStart and TurretMoveLoop within the UnitSpecificSounds 
        /// section of the object.
        /// </summary>
        public TurretAIData Turret { get; private set; }

        public TurretAIData AltTurret { get; private set; }

        [AddedIn(SageGame.CncGeneralsZeroHour)]
        public bool TurretsLinked { get; private set; }

        public BitArray<AutoAcquireEnemiesType> AutoAcquireEnemiesWhenIdle { get; private set; }
        public int MoodAttackCheckRate { get; private set; }

        [AddedIn(SageGame.CncGeneralsZeroHour)]
        public bool ForbidPlayerCommands { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public string AILuaEventsList { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public int HoldGroundCloseRangeDistance { get; private set; }
        [AddedIn(SageGame.Bfme)]
        public int MinCowerTime { get; private set; }
        [AddedIn(SageGame.Bfme)]
        public int MaxCowerTime { get; private set; }
        [AddedIn(SageGame.Bfme)]
        public bool CanAttackWhileContained { get; private set; }
    }

    public enum AutoAcquireEnemiesType
    {
        [IniEnum("YES")]
        Yes,

        [IniEnum("NO")]
        No,

        /// <summary>
        /// Attack buildings in addition to units.
        /// </summary>
        [IniEnum("ATTACK_BUILDINGS")]
        AttackBuildings,

        /// <summary>
        /// Don't counter-attack.
        /// </summary>
        [IniEnum("NotWhileAttacking")]
        NotWhileAttacking,

        [IniEnum("Stealthed")]
        Stealthed,
    }
}
