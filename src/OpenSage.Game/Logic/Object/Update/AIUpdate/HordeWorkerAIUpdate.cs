﻿using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme)]
    public sealed class HordeWorkerAIUpdateModuleData : AIUpdateModuleData
    {
        internal new static HordeWorkerAIUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private new static readonly IniParseTable<HordeWorkerAIUpdateModuleData> FieldParseTable = AIUpdateModuleData.FieldParseTable
            .Concat(new IniParseTable<HordeWorkerAIUpdateModuleData>
            {
                { "ComboLocomotorSet", (parser, x) => x.ComboLocomotorSet = parser.ParseAssetReference() },
                { "ComboLocoAttackDistance", (parser, x) => x.ComboLocoAttackDistance = parser.ParseInteger() }
            });
        
        public string ComboLocomotorSet { get; private set; }
        public int ComboLocoAttackDistance { get; private set; }
    }
}
