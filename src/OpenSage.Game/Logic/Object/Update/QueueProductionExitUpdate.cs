﻿using System.Numerics;
using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    public sealed class QueueProductionExitUpdateModuleData : UpdateModuleData
    {
        internal static QueueProductionExitUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<QueueProductionExitUpdateModuleData> FieldParseTable = new IniParseTable<QueueProductionExitUpdateModuleData>
        {
            { "UnitCreatePoint", (parser, x) => x.UnitCreatePoint = parser.ParseVector3() },
            { "NaturalRallyPoint", (parser, x) => x.NaturalRallyPoint = parser.ParseVector3() },
            { "ExitDelay", (parser, x) => x.ExitDelay = parser.ParseInteger() },
            { "InitialBurst", (parser, x) => x.InitialBurst = parser.ParseInteger() },
            { "PlacementViewAngle", (parser, x) => x.PlacementViewAngle = parser.ParseInteger() },
            { "NoExitPath", (parser, x) => x.NoExitPath = parser.ParseBoolean() },
            { "AllowAirborneCreation", (parser, x) => x.AllowAirborneCreation = parser.ParseBoolean() },
            { "UseReturnToFormation", (parser, x) => x.UseReturnToFormation = parser.ParseBoolean() }
        };

        public Vector3 UnitCreatePoint { get; private set; }

        /// <summary>
        /// <see cref="NaturalRallyPoint.X"/> must match <see cref="ObjectDefinition.GeometryMajorRadius"/>.
        /// </summary>
        public Vector3 NaturalRallyPoint { get; private set; }

        /// <summary>
        /// Used for Red Guards to make them come out one at a time.
        /// </summary>
        public int ExitDelay { get; private set; }

        public int InitialBurst { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public int PlacementViewAngle { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public bool NoExitPath { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public bool AllowAirborneCreation { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public bool UseReturnToFormation { get; private set; }
    }
}
