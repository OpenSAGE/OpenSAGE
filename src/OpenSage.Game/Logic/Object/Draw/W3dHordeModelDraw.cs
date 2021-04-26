using System;
using System.Collections.Generic;
using OpenSage.Client;
using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme)]

    public class W3dHordeModelDraw : W3dScriptedModelDraw
    {
        internal W3dHordeModelDraw(
            W3dHordeModelDrawModuleData data,
            Drawable drawable,
            GameContext context) : base(data, drawable, context)
        {
        }

        protected override bool SetActiveAnimationState(AnimationState animationState, Random random)
        {
            return base.SetActiveAnimationState(animationState, random);
        }
    }

    public class W3dHordeModelDrawModuleData : W3dScriptedModelDrawModuleData
    {
        internal static new W3dHordeModelDrawModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        internal static new readonly IniParseTable<W3dHordeModelDrawModuleData> FieldParseTable = W3dScriptedModelDrawModuleData.FieldParseTable
            .Concat(new IniParseTable<W3dHordeModelDrawModuleData>
            {
                { "LodOptions", (parser, x) => x.LodOptions.Add(LodOption.Parse(parser)) }
            });

        public List<LodOption> LodOptions { get; private set; } = new List<LodOption>();

        internal override DrawModule CreateDrawModule(Drawable drawable, GameContext context)
        {
            return new W3dHordeModelDraw(this, drawable, context);
        }
    }

    public sealed class LodOption
    {
        internal static LodOption Parse(IniParser parser)
        {
            var lod = parser.ParseEnum<ModelLevelOfDetail>();
            var result = parser.ParseBlock(FieldParseTable);
            result.Lod = lod;
            return result;
        }

        internal static readonly IniParseTable<LodOption> FieldParseTable = new IniParseTable<LodOption>
            {
                { "AllowMultipleModels", (parser, x) => x.AllowMultipleModels = parser.ParseBoolean() },
                { "MaxRandomTextures", (parser, x) => x.MaxRandomTextures = parser.ParseInteger() },
                { "MaxRandomAnimations", (parser, x) => x.MaxRandomAnimations = parser.ParseInteger() },
                { "MaxAnimFrameDelta", (parser, x) => x.MaxAnimFrameDelta = parser.ParseInteger() },
            };

        public ModelLevelOfDetail Lod { get; private set; }

        public bool AllowMultipleModels { get; private set; }
        public int MaxRandomTextures { get; private set; }
        public int MaxRandomAnimations { get; private set; }
        public int MaxAnimFrameDelta { get; private set; }
    }
}
