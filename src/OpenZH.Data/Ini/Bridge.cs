using System;
using System.Collections.Generic;
using System.Linq;
using OpenZH.Data.Ini.Parser;

namespace OpenZH.Data.Ini
{
    public sealed class Bridge
    {
        internal static Bridge Parse(IniParser parser)
        {
            return parser.ParseTopLevelNamedBlock(
                (x, name) => x.Name = name,
                FieldParseTable);
        }

        private static readonly IniParseTable<Bridge> FieldParseTable = new IniParseTable<Bridge>
        {
            { "BridgeScale", (parser, x) => x.BridgeScale = parser.ParseFloat() },
            { "RadarColor", (parser, x) => x.RadarColor = IniColorRgb.Parse(parser) },
            { "BridgeModelName", (parser, x) => x.BridgeModelName = parser.ParseAsciiString() },
            { "Texture", (parser, x) => x.Texture = parser.ParseAsciiString() },
            { "BridgeModelNameDamaged", (parser, x) => x.BridgeModelNameDamaged = parser.ParseAsciiString() },
            { "TextureDamaged", (parser, x) => x.TextureDamaged = parser.ParseAsciiString() },
            { "BridgeModelNameReallyDamaged", (parser, x) => x.BridgeModelNameReallyDamaged = parser.ParseAsciiString() },
            { "TextureReallyDamaged", (parser, x) => x.TextureReallyDamaged = parser.ParseAsciiString() },
            { "BridgeModelNameBroken", (parser, x) => x.BridgeModelNameBroken = parser.ParseAsciiString() },
            { "TextureBroken", (parser, x) => x.TextureBroken = parser.ParseAsciiString() },
            { "TowerObjectNameFromLeft", (parser, x) => x.TowerObjectNameFromLeft = parser.ParseAsciiString() },
            { "TowerObjectNameFromRight", (parser, x) => x.TowerObjectNameFromRight = parser.ParseAsciiString() },
            { "TowerObjectNameToLeft", (parser, x) => x.TowerObjectNameToLeft = parser.ParseAsciiString() },
            { "TowerObjectNameToRight", (parser, x) => x.TowerObjectNameToRight = parser.ParseAsciiString() },
            { "ScaffoldObjectName", (parser, x) => x.ScaffoldObjectName = parser.ParseAsciiString() },
            { "ScaffoldSupportObjectName", (parser, x) => x.ScaffoldSupportObjectName = parser.ParseAsciiString() },

            { "TransitionEffectsHeight", (parser, x) => x.TransitionEffectsHeight = parser.ParseFloat() },
            { "NumFXPerType", (parser, x) => x.NumFXPerType = parser.ParseInteger() },
            { "DamagedToSound", (parser, x) => x.DamagedToSound = parser.ParseAsciiString() },
            { "RepairedToSound", (parser, x) => x.RepairedToSound = parser.ParseAsciiString() },

            { "TransitionToOCL", (parser, x) => x.ParseTransition(parser, t => t.ObjectCreationList = parser.ParseAttributeString("OCL")) },
            { "TransitionToFX", (parser, x) => x.ParseTransition(parser, t => t.FX = parser.ParseAttributeString("FX")) },
        };

        public string Name { get; private set; }

        public float BridgeScale { get; private set; }
        public IniColorRgb RadarColor { get; private set; }
        public string BridgeModelName { get; private set; }
        public string Texture { get; private set; }
        public string BridgeModelNameDamaged { get; private set; }
        public string TextureDamaged { get; private set; }
        public string BridgeModelNameReallyDamaged { get; private set; }
        public string TextureReallyDamaged { get; private set; }
        public string BridgeModelNameBroken { get; private set; }
        public string TextureBroken { get; private set; }
        public string TowerObjectNameFromLeft { get; private set; }
        public string TowerObjectNameFromRight { get; private set; }
        public string TowerObjectNameToLeft { get; private set; }
        public string TowerObjectNameToRight { get; private set; }
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
