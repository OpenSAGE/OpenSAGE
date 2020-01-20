using System.Numerics;
using OpenSage.Content;
using OpenSage.Data.Ini;
using OpenSage.Graphics.ParticleSystems;
using OpenSage.Mathematics;

namespace OpenSage.FX
{
    public sealed class ParticleSystemFXNugget : FXNugget
    {
        internal static ParticleSystemFXNugget Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<ParticleSystemFXNugget> FieldParseTable = FXNuggetFieldParseTable.Concat(new IniParseTable<ParticleSystemFXNugget>
        {
            { "AttachToObject", (parser, x) => x.AttachToObject = parser.ParseBoolean() },
            { "AttachToBone", (parser, x) => x.AttachToBone = parser.ParseBoneName() },
            { "Count", (parser, x) => x.Count = parser.ParseInteger() },
            { "CreateAtGroundHeight", (parser, x) => x.CreateAtGroundHeight = parser.ParseBoolean() },
            { "Height", (parser, x) => x.Height = parser.ParseRandomVariable() },
            { "InitialDelay", (parser, x) => x.InitialDelay = parser.ParseRandomVariable() },
            { "Name", (parser, x) => x.Template = parser.ParseFXParticleSystemTemplateReference() },
            { "Offset", (parser, x) => x.Offset = parser.ParseVector3() },
            { "OrientToObject", (parser, x) => x.OrientToObject = parser.ParseBoolean() },
            { "Radius", (parser, x) => x.Radius = parser.ParseRandomVariable() },
            { "Ricochet", (parser, x) => x.Ricochet = parser.ParseBoolean() },
            { "RotateY", (parser, x) => x.RotateY = parser.ParseInteger() },
            { "UseCallersRadius", (parser, x) => x.UseCallersRadius = parser.ParseBoolean() },
            { "CreateBoneOverride", (parser, x) => x.CreateBoneOverride = parser.ParseBoneName() },
            { "TargetBoneOverride", (parser, x) => x.TargetBoneOverride = parser.ParseBoneName() },
            { "UseTargetOffset", (parser, x) => x.UseTargetOffset = parser.ParseBoolean() },
            { "TargetOffset", (parser, x) => x.TargetOffset = parser.ParseVector3() },
            { "TargetCoeff", (parser, x) => x.TargetCoeff = parser.ParseInteger() },
            { "SystemLife", (parser, x) => x.SystemLife = parser.ParseInteger() },
            { "SetTargetMatrix", (parser, x) => x.SetTargetMatrix = parser.ParseBoolean() },
            { "OnlyIfOnWater", (parser, x) => x.OnlyIfOnWater = parser.ParseBoolean() },
        });

        public bool AttachToObject { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public string AttachToBone { get; private set; }

        public int Count { get; private set; } = 1;
        public bool CreateAtGroundHeight { get; private set; }
        public RandomVariable Height { get; private set; }
        public RandomVariable InitialDelay { get; private set; }
        public LazyAssetReference<FXParticleSystemTemplate> Template { get; private set; }
        public Vector3 Offset { get; private set; }
        public bool OrientToObject { get; private set; }
        public RandomVariable Radius { get; private set; }
        public bool Ricochet { get; private set; }
        public int RotateY { get; private set; }
        public bool UseCallersRadius { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public string CreateBoneOverride { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public string TargetBoneOverride { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public bool UseTargetOffset { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public Vector3 TargetOffset { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public int TargetCoeff { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public int SystemLife { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public bool SetTargetMatrix { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public bool OnlyIfOnWater { get; private set; }

        internal override void Execute(FXListExecutionContext context)
        {
            var worldMatrix = OrientToObject
                ? Matrix4x4.CreateFromQuaternion(context.Rotation)
                : Matrix4x4.Identity;

            var position = context.Position;

            if (CreateAtGroundHeight)
            {
                position.Z = context.GameContext.Terrain.HeightMap.GetHeight(position.X, position.Y);
            }

            worldMatrix.Translation = position;

            var particleSystem = context.GameContext.ParticleSystems.Create(
                Template.Value,
                worldMatrix);

            particleSystem.Activate();
        }
    }
}
