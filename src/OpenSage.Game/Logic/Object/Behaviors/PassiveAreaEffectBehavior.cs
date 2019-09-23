﻿using System.Collections.Generic;
using OpenSage.Data.Ini;
using OpenSage.Mathematics;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme)]
    public sealed class PassiveAreaEffectBehaviorModuleData : UpdateModuleData
    {
        internal static PassiveAreaEffectBehaviorModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<PassiveAreaEffectBehaviorModuleData> FieldParseTable = new IniParseTable<PassiveAreaEffectBehaviorModuleData>
        {
            { "EffectRadius", (parser, x) => x.EffectRadius = parser.ParseLong() },
            { "PingDelay", (parser, x) => x.PingDelay = parser.ParseInteger() },
            { "HealPercentPerSecond", (parser, x) => x.HealPercentPerSecond = parser.ParsePercentage() },
            { "AllowFilter", (parser, x) => x.AllowFilter = ObjectFilter.Parse(parser) },
            { "ModifierName", (parser, x) => x.ModifierNames.Add(parser.ParseAssetReference()) },
            { "UpgradeRequired", (parser, x) => x.UpgradeRequired = parser.ParseAssetReference() },
            { "NonStackable", (parser, x) => x.NonStackable = parser.ParseBoolean() },
            { "HealFX", (parser, x) => x.HealFX = parser.ParseAssetReference() },
            { "AntiCategories", (parser, x) => x.AntiCategories = parser.ParseEnumBitArray<ModifierCategory>() }
        };

        public long EffectRadius { get; private set; }
        public int PingDelay { get; private set; }
        public Percentage HealPercentPerSecond { get; private set; }
        public ObjectFilter AllowFilter { get; private set; }
        public List<string> ModifierNames { get; } = new List<string>();

        [AddedIn(SageGame.Bfme2)]
        public string UpgradeRequired { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public bool NonStackable { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public string HealFX { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public BitArray<ModifierCategory> AntiCategories { get; private set; }
    }
}
