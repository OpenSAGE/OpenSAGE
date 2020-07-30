﻿using OpenSage.Content;
using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    public sealed class CreateCrateDie : DieModule
    {
        // TODO
    }

    public sealed class CreateCrateDieModuleData : DieModuleData
    {
        internal static CreateCrateDieModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<CreateCrateDieModuleData> FieldParseTable = DieModuleData.FieldParseTable
            .Concat(new IniParseTable<CreateCrateDieModuleData>
            {
                { "CrateData", (parser, x) => x.CrateData = parser.ParseCrateReference() }
            });

        public LazyAssetReference<CrateData> CrateData { get; private set; }

        internal override BehaviorModule CreateModule(GameObject gameObject, GameContext context)
        {
            return new CreateCrateDie();
        }
    }
}
