using System;
using System.Collections.Generic;
using OpenSage.Data.Ini;
using OpenSage.Mathematics;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme)]
    public class W3dScriptedModelDraw : W3dModelDraw
    {
        private readonly GameContext _context;
        public AnimationState ActiveAnimationState => _activeAnimationState;

        internal W3dScriptedModelDraw(
            W3dScriptedModelDrawModuleData data,
            GameObject gameObject,
            GameContext context) : base(data, gameObject, context)
        {
            _context = context;
        }

        protected override bool SetActiveAnimationState(AnimationState animationState, Random random)
        {
            if (!base.SetActiveAnimationState(animationState, random))
            {
                return false;
            }

            if (animationState != null && animationState.Script != null)
            {
                _context.Scene3D.Game.Lua.ExecuteDrawModuleLuaCode(this, animationState.Script);
            }
            _activeAnimationState = animationState;
            return true;
        }
    }


    public class W3dScriptedModelDrawModuleData : W3dModelDrawModuleData
    {
        internal static W3dScriptedModelDrawModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        internal static new readonly IniParseTable<W3dScriptedModelDrawModuleData> FieldParseTable = W3dModelDrawModuleData.FieldParseTable
            .Concat(new IniParseTable<W3dScriptedModelDrawModuleData>
            {
                { "StaticModelLODMode", (parser, x) => x.StaticModelLODMode = parser.ParseBoolean() },
                { "ShowShadowWhileContained", (parser, x) => x.ShowShadowWhileContained = parser.ParseBoolean() },
                { "RandomTexture", (parser, x) => x.RandomTextures.Add(RandomTexture.Parse(parser)) },
                { "WallBoundsMesh", (parser, x) => x.WallBoundsMesh = parser.ParseAssetReference() },
                { "UseStandardModelNames", (parser, x) => x.UseStandardModelNames = parser.ParseBoolean() },
                { "RampMesh1", (parser, x) => x.RampMesh1 = parser.ParseAssetReference() },
                { "RampMesh2", (parser, x) => x.RampMesh2 = parser.ParseAssetReference() },
                { "MultiPlayerOnly", (parser, x) => x.MultiPlayerOnly = parser.ParseBoolean() },
                { "RaisedWallMesh", (parser, x) => x.RaisedWallMesh = parser.ParseAssetReference() },
                { "AlphaCameraFadeOuterRadius", (parser, x) => x.AlphaCameraFadeOuterRadius = parser.ParseInteger() },
                { "AlphaCameraFadeInnerRadius", (parser, x) => x.AlphaCameraFadeInnerRadius = parser.ParseInteger() },
                { "AlphaCameraAtInnerRadius", (parser, x) => x.AlphaCameraAtInnerRadius = parser.ParsePercentage() },
                { "UseDefaultAnimation", (parser, x) => x.UseDefaultAnimation = parser.ParseBoolean() },
                { "DependencySharedModelFlags", (parser, x) => x.DependencySharedModelFlags = parser.ParseEnumBitArray<ModelConditionFlag>() },
                { "AffectedByStealth", (parser, x) => x.AffectedByStealth = parser.ParseBoolean() },
                { "StaticSortLevelWhileFading", (parser, x) => x.StaticSortLevelWhileFading = parser.ParseInteger() },
                { "GlowEnabled", (parser, x) => x.GlowEnabled = parser.ParseBoolean() },
                { "GlowEmissive", (parser, x) => x.GlowEmissive = parser.ParseBoolean() },
                { "HighDetailOnly", (parser, x) => x.HighDetailOnly = parser.ParseBoolean() },
                { "WadingParticleSys", (parser, x) => x.WadingParticleSys = parser.ParseAssetReference() },
                { "NoRotate", (parser, x) => x.NoRotate = parser.ParseBoolean() },
                { "UseProducerTexture", (parser, x) => x.UseProducerTexture = parser.ParseBoolean() },
                { "ShadowForceDisable", (parser, x) => x.ShadowForceDisable = parser.ParseBoolean() },
                { "RandomTextureFixedRandomIndex", (parser, x) => x.RandomTextureFixedRandomIndex = parser.ParseBoolean() }
            });

        public bool StaticModelLODMode { get; private set; }
        public bool ShowShadowWhileContained { get; private set; }
        public List<RandomTexture> RandomTextures { get; private set; } = new List<RandomTexture>();
        public string WallBoundsMesh { get; private set; }
        public bool UseStandardModelNames { get; private set; }
        public string RampMesh1 { get; private set; }
        public string RampMesh2 { get; private set; }
        public bool MultiPlayerOnly { get; private set; }
        public string RaisedWallMesh { get; private set; }
        public int AlphaCameraFadeOuterRadius { get; private set; }
        public int AlphaCameraFadeInnerRadius { get; private set; }
        public Percentage AlphaCameraAtInnerRadius { get; private set; }
        public bool UseDefaultAnimation { get; private set; }
        public BitArray<ModelConditionFlag> DependencySharedModelFlags { get; private set; }
        public bool AffectedByStealth { get; private set; }
        public int StaticSortLevelWhileFading { get; private set; }
        public bool GlowEnabled { get; private set; }
        public bool GlowEmissive { get; private set; }
        public bool HighDetailOnly { get; private set; }
        public string WadingParticleSys { get; private set; }
        public bool NoRotate { get; private set; }
        public bool UseProducerTexture { get; private set; }
        public bool ShadowForceDisable { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public bool RandomTextureFixedRandomIndex { get; private set; }

        internal override DrawModule CreateDrawModule(GameObject gameObject, GameContext context)
        {
            return new W3dScriptedModelDraw(this, gameObject, context);
        }
    }

    public sealed class RandomTexture
    {
        internal static RandomTexture Parse(IniParser parser)
        {
            var result = new RandomTexture
            {
                First = parser.ParseAssetReference(),
                Unknown = parser.ParseInteger()
            };
            if (result.Unknown != 0)
            {
                throw new Exception();
            }

            var second = parser.GetNextTokenOptional();
            if (second.HasValue)
            {
                result.Second = parser.ScanAssetReference(second.Value);
            }
            return result;
        }

        public string First { get; private set; }
        public int Unknown { get; private set; }
        public string Second { get; private set; }}
}
