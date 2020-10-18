using System;
using System.Collections.Generic;
using OpenSage.Content;
using OpenSage.Data.Ini;
using OpenSage.FX;
using OpenSage.Mathematics;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme)]
    public class PassiveAreaEffectBehavior : UpdateModule
    {
        private readonly GameObject _gameObject;
        private readonly PassiveAreaEffectBehaviorModuleData _moduleData;
        private TimeSpan _nextPing;

        public PassiveAreaEffectBehavior(GameObject gameObject, PassiveAreaEffectBehaviorModuleData moduleData)
        {
            _moduleData = moduleData;
            _gameObject = gameObject;

        }

        internal override void Update(BehaviorUpdateContext context)
        {
            if (context.Time.TotalTime < _nextPing)
            {
                return;
            }
            _nextPing = context.Time.TotalTime + TimeSpan.FromMilliseconds(_moduleData.PingDelay);

            var nearbyObjects = context.GameContext.Quadtree.FindNearby(_gameObject, _gameObject.Transform, _moduleData.EffectRadius);

            foreach (var nearbyObject in nearbyObjects)
            {
                if (!_moduleData.AllowFilter.Matches(nearbyObject))
                {
                    continue;
                }

                // TODO: HealPercentPerSecond, UpgradeRequired, NonStackable, HealFX, AntiCategories

                foreach (var modifier in _moduleData.Modifiers)
                {
                    nearbyObject.AddAttributeModifier(modifier.Value.Name, new AttributeModifier(modifier.Value));
                }
            }
        }
    }


    public sealed class PassiveAreaEffectBehaviorModuleData : UpdateModuleData
    {
        internal static PassiveAreaEffectBehaviorModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<PassiveAreaEffectBehaviorModuleData> FieldParseTable = new IniParseTable<PassiveAreaEffectBehaviorModuleData>
        {
            { "EffectRadius", (parser, x) => x.EffectRadius = parser.ParseLong() },
            { "PingDelay", (parser, x) => x.PingDelay = parser.ParseInteger() },
            { "HealPercentPerSecond", (parser, x) => x.HealPercentPerSecond = parser.ParsePercentage() },
            { "AllowFilter", (parser, x) => x.AllowFilter = ObjectFilter.Parse(parser) },
            { "ModifierName", (parser, x) => x.Modifiers.Add(parser.ParseModifierListReference()) },
            { "UpgradeRequired", (parser, x) => x.UpgradeRequired = parser.ParseUpgradeReference() },
            { "NonStackable", (parser, x) => x.NonStackable = parser.ParseBoolean() },
            { "HealFX", (parser, x) => x.HealFX = parser.ParseFXListReference() },
            { "AntiCategories", (parser, x) => x.AntiCategories = parser.ParseEnumBitArray<ModifierCategory>() }
        };

        public long EffectRadius { get; private set; }
        public int PingDelay { get; private set; }
        public Percentage HealPercentPerSecond { get; private set; }
        public ObjectFilter AllowFilter { get; private set; }
        public List<LazyAssetReference<ModifierList>> Modifiers { get; } = new List<LazyAssetReference<ModifierList>>();

        [AddedIn(SageGame.Bfme2)]
        public LazyAssetReference<UpgradeTemplate> UpgradeRequired { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public bool NonStackable { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public LazyAssetReference<FXList> HealFX { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public BitArray<ModifierCategory> AntiCategories { get; private set; }

        internal override BehaviorModule CreateModule(GameObject gameObject, GameContext context)
        {
            return new PassiveAreaEffectBehavior(gameObject, this);
        }
    }
}
