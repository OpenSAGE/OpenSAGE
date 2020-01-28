using System.Linq;
using System.Numerics;
using OpenSage.Content;
using OpenSage.Data.Ini;
using OpenSage.Mathematics;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// Creates an object and sends it towards the target with a warhead.
    /// </summary>
    [AddedIn(SageGame.Bfme)]
    public sealed class ProjectileNugget : WeaponEffectNugget
    {
        internal static ProjectileNugget Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<ProjectileNugget> FieldParseTable = WeaponEffectNugget.FieldParseTable
            .Concat(new IniParseTable<ProjectileNugget>
            {
                { "ProjectileTemplateName", (parser, x) => x.ProjectileTemplate = parser.ParseObjectReference() },
                { "WarheadTemplateName", (parser, x) => x.WarheadTemplate = parser.ParseWeaponTemplateReference() },
                
                { "AlwaysAttackHereOffset", (parser, x) => x.AlwaysAttackHereOffset = parser.ParseVector3() },
                { "UseAlwaysAttackOffset", (parser, x) => x.UseAlwaysAttackOffset = parser.ParseBoolean() },
                { "WeaponLaunchBoneSlotOverride", (parser, x) => x.WeaponLaunchBoneSlotOverride = parser.ParseEnum<WeaponSlot>() }
            });

        internal bool IsConvertedFromLegacyData;

        internal WeaponTemplate ParentWeaponTemplate;

        public LazyAssetReference<ObjectDefinition> ProjectileTemplate { get; internal set; }
        public LazyAssetReference<WeaponTemplate> WarheadTemplate { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public Vector3 AlwaysAttackHereOffset { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public bool UseAlwaysAttackOffset { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public WeaponSlot WeaponLaunchBoneSlotOverride { get; private set; } = WeaponSlot.NoWeapon;

        internal override void Execute(WeaponEffectExecutionContext context)
        {
            var projectileTemplate = ProjectileTemplate.Value;

            WeaponTemplate warheadTemplate;
            if (IsConvertedFromLegacyData)
            {
                warheadTemplate = new WeaponTemplate();
                warheadTemplate.ProjectileCollidesWith = ParentWeaponTemplate.ProjectileCollidesWith;
                warheadTemplate.Nuggets.AddRange(ParentWeaponTemplate.Nuggets.OfType<DamageNugget>());
            }
            else
            {
                warheadTemplate = WarheadTemplate.Value;
            }

            var projectileObject = context.GameContext.GameObjects.Add(
                projectileTemplate,
                context.Weapon.ParentGameObject.Owner);

            var launchBoneTransform = context.Weapon.ParentGameObject.GetWeaponLaunchBoneTransform(
                context.Weapon.Slot, context.Weapon.WeaponIndex);

            projectileObject.Transform.CopyFrom(launchBoneTransform.Value);

            projectileObject.SetWeapon(warheadTemplate);

            projectileObject.CurrentWeapon.SetTarget(context.Weapon.CurrentTarget);

            projectileObject.Speed = ParentWeaponTemplate.WeaponSpeed;

            if (IsConvertedFromLegacyData)
            {
                var bezierProjectileBehavior = projectileObject.FindBehavior<BezierProjectileBehavior>();
                if (bezierProjectileBehavior != null)
                {
                    bezierProjectileBehavior.DetonationFX = ParentWeaponTemplate.ProjectileDetonationFX?.Value;
                }

                var missileAIUpdate = projectileObject.FindBehavior<MissileAIUpdate>();
                if (missileAIUpdate != null)
                {
                    missileAIUpdate.DetonationFX = ParentWeaponTemplate.ProjectileDetonationFX?.Value;
                }
            }
        }
    }
}
