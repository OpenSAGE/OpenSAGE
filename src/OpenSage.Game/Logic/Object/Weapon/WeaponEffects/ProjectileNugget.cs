using System;
using System.Numerics;
using OpenSage.Content;
using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    public sealed class ProjectileNugget : WeaponEffectNugget
    {
        private readonly Weapon _weapon;
        private readonly ProjectileNuggetData _data;

        internal ProjectileNugget(Weapon weapon, ProjectileNuggetData data)
        {
            _weapon = weapon;
            _data = data;
        }

        internal override void Activate(TimeSpan currentTime)
        {
            // TODO: take care of weaponspeed and spawn projectile
        }

        internal override void Update(TimeSpan currentTime)
        {
            
        }
    }

    [AddedIn(SageGame.Bfme)]
    public sealed class ProjectileNuggetData : WeaponEffectNuggetData
    {
        internal static ProjectileNuggetData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<ProjectileNuggetData> FieldParseTable = WeaponEffectNuggetData.FieldParseTable
            .Concat(new IniParseTable<ProjectileNuggetData>
            {
                { "ProjectileTemplateName", (parser, x) => x.ProjectileTemplate = parser.ParseObjectReference() },
                { "WarheadTemplateName", (parser, x) => x.WarheadTemplate = parser.ParseWeaponTemplateReference() },
                
                { "AlwaysAttackHereOffset", (parser, x) => x.AlwaysAttackHereOffset = parser.ParseVector3() },
                { "UseAlwaysAttackOffset", (parser, x) => x.UseAlwaysAttackOffset = parser.ParseBoolean() },
                { "WeaponLaunchBoneSlotOverride", (parser, x) => x.WeaponLaunchBoneSlotOverride = parser.ParseEnum<WeaponSlot>() }
            });

        public LazyAssetReference<ObjectDefinition> ProjectileTemplate { get; internal set; }
        public LazyAssetReference<WeaponTemplate> WarheadTemplate { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public Vector3 AlwaysAttackHereOffset { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public bool UseAlwaysAttackOffset { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public WeaponSlot WeaponLaunchBoneSlotOverride { get; private set; } = WeaponSlot.NoWeapon;

        internal override WeaponEffectNugget CreateNugget(Weapon weapon)
        {
            return new ProjectileNugget(weapon, this);
        }
    }
}
