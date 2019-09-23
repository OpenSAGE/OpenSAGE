using System.Collections.Generic;
using OpenSage.Data.Ini;
using OpenSage.Mathematics;

namespace OpenSage.Logic.Object
{
    public sealed class TurretAIData
    {
        internal static TurretAIData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<TurretAIData> FieldParseTable = new IniParseTable<TurretAIData>
        {
            { "InitiallyDisabled", (parser, x) => x.InitiallyDisabled = parser.ParseBoolean() },
            { "TurretTurnRate", (parser, x) => x.TurretTurnRate = parser.ParseInteger() },
            { "TurretPitchRate", (parser, x) => x.TurretPitchRate = parser.ParseInteger() },
            { "AllowsPitch", (parser, x) => x.AllowsPitch = parser.ParseBoolean() },
            { "FiresWhileTurning", (parser, x) => x.FiresWhileTurning = parser.ParseBoolean() },
            { "NaturalTurretPitch", (parser, x) => x.NaturalTurretPitch = parser.ParseInteger() },
            { "NaturalTurretAngle", (parser, x) => x.NaturalTurretAngle = parser.ParseInteger() },
            { "GroundUnitPitch", (parser, x) => x.GroundUnitPitch = parser.ParseInteger() },
            { "MinPhysicalPitch", (parser, x) => x.MinPhysicalPitch = parser.ParseInteger() },
            { "FirePitch", (parser, x) => x.FirePitch = parser.ParseInteger() },
            { "MinIdleScanAngle", (parser, x) => x.MinIdleScanAngle = parser.ParseInteger() },
            { "MaxIdleScanAngle", (parser, x) => x.MaxIdleScanAngle = parser.ParseInteger() },
            { "MinIdleScanInterval", (parser, x) => x.MinIdleScanInterval = parser.ParseInteger() },
            { "MaxIdleScanInterval", (parser, x) => x.MaxIdleScanInterval = parser.ParseInteger() },
            { "RecenterTime", (parser, x) => x.RecenterTime = parser.ParseInteger() },
            { "ControlledWeaponSlots", (parser, x) => x.ControlledWeaponSlots = parser.ParseEnumBitArray<WeaponSlot>() },

            { "TurretFireAngleSweep", (parser, x) => x.TurretFireAngleSweeps.Add(parser.ParseEnum<WeaponSlot>(), parser.ParseInteger()) },
            { "TurretSweepSpeedModifier", (parser, x) => x.TurretSweepSpeedModifiers.Add(parser.ParseEnum<WeaponSlot>(), parser.ParseFloat()) },

            { "TurretMaxDeflectionCW", (parser, x) => x.TurretMaxDeflectionCW = parser.ParseInteger() },
            { "TurretMaxDeflectionACW", (parser, x) => x.TurretMaxDeflectionACW = parser.ParseInteger() },
        };

        public bool InitiallyDisabled { get; private set; }

        /// <summary>
        /// Turn rate, in degrees per second.
        /// </summary>
        public int TurretTurnRate { get; private set; }

        public int TurretPitchRate { get; private set; }

        public bool AllowsPitch { get; private set; }

        public bool FiresWhileTurning { get; private set; }

        public int NaturalTurretPitch { get; private set; }

        public int NaturalTurretAngle { get; private set; }

        public int GroundUnitPitch { get; private set; }

        public int MinPhysicalPitch { get; private set; }

        public int FirePitch { get; private set; }

        /// <summary>
        /// Minimum offset, in degrees, from <see cref="NaturalTurretAngle"/>.
        /// </summary>
        public int MinIdleScanAngle { get; private set; }

        /// <summary>
        /// Maximum offset, in degrees, from <see cref="NaturalTurretAngle"/>.
        /// </summary>
        public int MaxIdleScanAngle { get; private set; }

        public int MinIdleScanInterval { get; private set; }

        public int MaxIdleScanInterval { get; private set; }

        /// <summary>
        /// Time to wait when idling before recentering.
        /// </summary>
        public int RecenterTime { get; private set; }

        public BitArray<WeaponSlot> ControlledWeaponSlots { get; private set; }

        public Dictionary<WeaponSlot, int> TurretFireAngleSweeps { get; } = new Dictionary<WeaponSlot, int>();
        public Dictionary<WeaponSlot, float> TurretSweepSpeedModifiers { get; } = new Dictionary<WeaponSlot, float>();

        [AddedIn(SageGame.Bfme2Rotwk)]
        public int TurretMaxDeflectionCW { get; private set; }

        [AddedIn(SageGame.Bfme2Rotwk)]
        public int TurretMaxDeflectionACW { get; private set; }
    }
}
