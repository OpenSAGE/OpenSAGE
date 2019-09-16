using System;
using System.Collections.Generic;
using System.Linq;
using OpenSage.Content;
using OpenSage.Data.Ini.Parser;
using OpenSage.Graphics;
using OpenSage.Logic.Object;
using OpenSage.Mathematics;

namespace OpenSage.Data.Ini
{
    public sealed class BridgeTemplate : IHasName
    {
        internal static BridgeTemplate Parse(IniParser parser)
        {
            return parser.ParseNamedBlock(
                (x, name) => x.Name = name,
                FieldParseTable);
        }

        private static readonly IniParseTable<BridgeTemplate> FieldParseTable = new IniParseTable<BridgeTemplate>
        {
            { "BridgeScale", (parser, x) => x.BridgeScale = parser.ParseFloat() },
            { "RadarColor", (parser, x) => x.RadarColor = parser.ParseColorRgb() },
            { "BridgeModelName", (parser, x) => x.BridgeModelName = parser.ParseModelReference() },
            { "Texture", (parser, x) => x.Texture = parser.ParseFileName() },
            { "BridgeModelNameDamaged", (parser, x) => x.BridgeModelNameDamaged = parser.ParseFileName() },
            { "TextureDamaged", (parser, x) => x.TextureDamaged = parser.ParseFileName() },
            { "BridgeModelNameReallyDamaged", (parser, x) => x.BridgeModelNameReallyDamaged = parser.ParseFileName() },
            { "TextureReallyDamaged", (parser, x) => x.TextureReallyDamaged = parser.ParseFileName() },
            { "BridgeModelNameBroken", (parser, x) => x.BridgeModelNameBroken = parser.ParseFileName() },
            { "TextureBroken", (parser, x) => x.TextureBroken = parser.ParseFileName() },
            { "TowerObjectNameFromLeft", (parser, x) => x.TowerObjectNameFromLeft = parser.ParseObjectReference() },
            { "TowerObjectNameFromRight", (parser, x) => x.TowerObjectNameFromRight = parser.ParseObjectReference() },
            { "TowerObjectNameToLeft", (parser, x) => x.TowerObjectNameToLeft = parser.ParseObjectReference() },
            { "TowerObjectNameToRight", (parser, x) => x.TowerObjectNameToRight = parser.ParseObjectReference() },
            { "ScaffoldObjectName", (parser, x) => x.ScaffoldObjectName = parser.ParseAssetReference() },
            { "ScaffoldSupportObjectName", (parser, x) => x.ScaffoldSupportObjectName = parser.ParseAssetReference() },

            { "TransitionEffectsHeight", (parser, x) => x.TransitionEffectsHeight = parser.ParseFloat() },
            { "NumFXPerType", (parser, x) => x.NumFXPerType = parser.ParseInteger() },
            { "DamagedToSound", (parser, x) => x.DamagedToSound = parser.ParseAssetReference() },
            { "RepairedToSound", (parser, x) => x.RepairedToSound = parser.ParseAssetReference() },

            { "TransitionToOCL", (parser, x) => x.ParseTransition(parser, t => t.ObjectCreationList = parser.ParseAttributeIdentifier("OCL")) },
            { "TransitionToFX", (parser, x) => x.ParseTransition(parser, t => t.FX = parser.ParseAttributeIdentifier("FX")) },
        };

        public string Name { get; private set; }

        public float BridgeScale { get; private set; }
        public ColorRgb RadarColor { get; private set; }
        public LazyAssetReference<Model> BridgeModelName { get; private set; }
        public string Texture { get; private set; }
        public string BridgeModelNameDamaged { get; private set; }
        public string TextureDamaged { get; private set; }
        public string BridgeModelNameReallyDamaged { get; private set; }
        public string TextureReallyDamaged { get; private set; }
        public string BridgeModelNameBroken { get; private set; }
        public string TextureBroken { get; private set; }
        public LazyAssetReference<ObjectDefinition> TowerObjectNameFromLeft { get; private set; }
        public LazyAssetReference<ObjectDefinition> TowerObjectNameFromRight { get; private set; }
        public LazyAssetReference<ObjectDefinition> TowerObjectNameToLeft { get; private set; }
        public LazyAssetReference<ObjectDefinition> TowerObjectNameToRight { get; private set; }
        public string ScaffoldObjectName { get; private set; }
        public string ScaffoldSupportObjectName { get; private set; }

        public float TransitionEffectsHeight { get; private set; }
        public int NumFXPerType { get; private set; }
        public string DamagedToSound { get; private set; }
        public string RepairedToSound { get; private set; }

        public List<BridgeTransition> Transitions { get; } = new List<BridgeTransition>();

        private void ParseTransition(IniParser parser, Action<BridgeTransition> callback)
        {
            var transitionType = parser.ParseAttributeEnum<BridgeTransitionType>("Transition");
            var toState = parser.ParseAttributeEnum<BodyDamageType>("ToState");
            var effectNum = parser.ParseAttributeInteger("EffectNum");

            var transition = Transitions.FirstOrDefault(x => 
                x.Transition == transitionType
                && x.ToState == toState
                && x.EffectNum == effectNum);

            if (transition == null)
            {
                Transitions.Add(transition = new BridgeTransition(transitionType, toState, effectNum));
            }

            callback(transition);
        }
    }

    public sealed class BridgeTransition
    {
        public BridgeTransitionType Transition { get; private set; }
        public BodyDamageType ToState { get; private set; }
        public int EffectNum { get; private set; }

        public BridgeTransition(
            BridgeTransitionType transitionType, 
            BodyDamageType toState,
            int effectNum)
        {
            Transition = transitionType;
            ToState = toState;
            EffectNum = effectNum;
        }

        public string ObjectCreationList { get; internal set; }
        public string FX { get; internal set; }
    }

    public enum BridgeTransitionType
    {
        [IniEnum("Damage")]
        Damage,

        [IniEnum("Repair")]
        Repair
    }
}
