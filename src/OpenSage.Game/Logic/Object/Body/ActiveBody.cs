using System.Collections.Generic;
using System.IO;
using OpenSage.Data.Ini;
using OpenSage.FileFormats;
using OpenSage.FX;
using OpenSage.Mathematics;
using OpenSage.Mathematics.FixedMath;

namespace OpenSage.Logic.Object
{
    public class ActiveBody : BodyModule
    {
        private readonly ActiveBodyModuleData _moduleData;

        protected readonly GameObject GameObject;

        public override Fix64 MaxHealth { get; internal set; }

        internal ActiveBody(GameObject gameObject, ActiveBodyModuleData moduleData)
        {
            GameObject = gameObject;
            _moduleData = moduleData;

            MaxHealth = (Fix64) moduleData.MaxHealth;

            SetHealth((Fix64) (moduleData.InitialHealth ?? moduleData.MaxHealth));
        }

        private void SetHealth(Fix64 value)
        {
            Health = value;
            GameObject.UpdateDamageFlags(HealthPercentage);
        }

        public override void SetInitialHealth(float multiplier)
        {
            SetHealth((Fix64) ((_moduleData.InitialHealth ?? _moduleData.MaxHealth) * multiplier));
        }

        public override void DoDamage(DamageType damageType, Fix64 amount, DeathType deathType, TimeInterval time)
        {
            if (Health <= Fix64.Zero)
            {
                return;
            }

            var armorSet = GameObject.CurrentArmorSet;

            // Actual amount of damage depends on armor.
            var armor = armorSet.Armor.Value;
            var damagePercent = armor?.GetDamagePercent(damageType) ?? new Percentage(1.0f);
            var actualDamage = amount * (Fix64) ((float) damagePercent);
            SetHealth(Health - actualDamage);

            // TODO: DamageFX
            var damageFXGroup = armorSet.DamageFX.Value.GetGroup(damageType);

            // TODO: MajorFX
            var damageFX = damageFXGroup.MinorFX?.Value;
            damageFX?.Execute(
                new FXListExecutionContext(
                    GameObject.Rotation,
                    GameObject.Translation,
                    GameObject.GameContext));

            if (Health <= Fix64.Zero)
            {
                GameObject.Die(deathType, time);
            }
        }

        internal override void Load(BinaryReader reader)
        {
            var version = reader.ReadVersion();
            if (version != 1)
            {
                throw new InvalidDataException();
            }

            base.Load(reader);

            var unknownFloat = reader.ReadSingle();
            if (unknownFloat != 1.0f)
            {
                throw new InvalidDataException();
            }

            var currentHealth1 = reader.ReadSingle(); // These two values
            var currentHealth2 = reader.ReadSingle(); // are almost but not quite the same.

            var maxHealth1 = reader.ReadSingle();
            var maxHealth2 = reader.ReadSingle();
            if (maxHealth1 != maxHealth2)
            {
                throw new InvalidDataException();
            }

            var unknown = reader.ReadBytes(61);
        }
    }

    public class ActiveBodyModuleData : BodyModuleData
    {
        internal static ActiveBodyModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        internal static readonly IniParseTable<ActiveBodyModuleData> FieldParseTable = new IniParseTable<ActiveBodyModuleData>
        {
            { "MaxHealth", (parser, x) => x.MaxHealth = parser.ParseFloat() },
            { "InitialHealth", (parser, x) => x.InitialHealth = parser.ParseFloat() },
            { "MaxHealthDamaged", (parser, x) => x.MaxHealthDamaged = parser.ParseFloat() },
            { "MaxHealthReallyDamaged", (parser, x) => x.MaxHealthReallyDamaged = parser.ParseFloat() },
            { "RecoveryTime", (parser, x) => x.RecoveryTime = parser.ParseFloat() },

            { "SubdualDamageCap", (parser, x) => x.SubdualDamageCap = parser.ParseInteger() },
            { "SubdualDamageHealRate", (parser, x) => x.SubdualDamageHealRate = parser.ParseInteger() },
            { "SubdualDamageHealAmount", (parser, x) => x.SubdualDamageHealAmount = parser.ParseInteger() },
            { "GrabObject", (parser, x) => x.GrabObject = parser.ParseAssetReference() },
            { "GrabOffset", (parser, x) => x.GrabOffset = parser.ParsePoint() },
            { "DamageCreationList", (parser, x) => x.DamageCreationLists.Add(DamageCreationList.Parse(parser)) },
            { "GrabFX", (parser, x) => x.GrabFX = parser.ParseAssetReference() },
            { "GrabDamage", (parser, x) => x.GrabDamage = parser.ParseInteger() },
            { "CheerRadius", (parser, x) => x.CheerRadius = parser.ParseInteger() },
            { "DodgePercent", (parser, x) => x.DodgePercent = parser.ParsePercentage() },
            { "UseDefaultDamageSettings", (parser, x) => x.UseDefaultDamageSettings = parser.ParseBoolean() },
            { "EnteringDamagedTransitionTime", (parser, x) => x.EnteringDamagedTransitionTime = parser.ParseInteger() },
            { "HealingBuffFx", (parser, x) => x.HealingBuffFx = parser.ParseAssetReference() },
            { "BurningDeathBehavior", (parser, x) => x.BurningDeathBehavior = parser.ParseBoolean() },
            { "BurningDeathFX", (parser, x) => x.BurningDeathFX = parser.ParseAssetReference() },
            { "DamagedAttributeModifier", (parser, x) => x.DamagedAttributeModifier = parser.ParseAssetReference() },
            { "ReallyDamagedAttributeModifier", (parser, x) => x.ReallyDamagedAttributeModifier = parser.ParseAssetReference() }
        };

        public float MaxHealth { get; private set; }
        public float? InitialHealth { get; private set; }
       
        [AddedIn(SageGame.CncGeneralsZeroHour)]
        public int SubdualDamageCap { get; private set; }

        [AddedIn(SageGame.CncGeneralsZeroHour)]
        public int SubdualDamageHealRate { get; private set; }

        [AddedIn(SageGame.CncGeneralsZeroHour)]
        public int SubdualDamageHealAmount { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public float MaxHealthDamaged { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public float RecoveryTime { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public string GrabObject { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public Point2D GrabOffset { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public List<DamageCreationList> DamageCreationLists { get; private set; } = new List<DamageCreationList>();

        [AddedIn(SageGame.Bfme)]
        public float MaxHealthReallyDamaged { get; private set; }
        
        [AddedIn(SageGame.Bfme)]
        public string GrabFX { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public int GrabDamage { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public int CheerRadius { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public Percentage DodgePercent { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public bool UseDefaultDamageSettings { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public int EnteringDamagedTransitionTime { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public string HealingBuffFx { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public bool BurningDeathBehavior { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public string BurningDeathFX { get; private set; }

        [AddedIn(SageGame.Bfme2Rotwk)]
        public string DamagedAttributeModifier { get; private set; }

        [AddedIn(SageGame.Bfme2Rotwk)]
        public string ReallyDamagedAttributeModifier { get; private set; }

        internal override BodyModule CreateBodyModule(GameObject gameObject)
        {
            return new ActiveBody(gameObject, this);
        }
    }

    [AddedIn(SageGame.Bfme)]
    public sealed class DamageCreationList
    {
        internal static DamageCreationList Parse(IniParser parser)
        {
            return new DamageCreationList()
            {
                Object = parser.ParseAssetReference(),
                ObjectKind = parser.ParseEnum<ObjectKinds>(),
                Unknown = parser.ParseString()
            };
        }

        public string Object { get; private set; }
        public ObjectKinds ObjectKind { get; private set; }
        public string Unknown { get; private set; }
    }
}
