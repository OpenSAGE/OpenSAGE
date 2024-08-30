using System.Collections.Generic;
using FixedMath.NET;
using OpenSage.Data.Ini;
using OpenSage.FX;
using OpenSage.Logic.Object.Damage;
using OpenSage.Mathematics;

namespace OpenSage.Logic.Object
{
    public class ActiveBody : BodyModule
    {
        private readonly GameContext _context;
        private readonly ActiveBodyModuleData _moduleData;
        private readonly List<uint> _particleSystemIds = new();

        private float _currentHealth;
        private float _lastHealthBeforeDamage;
        private float _maxHealth;
        private BodyDamageType _damageType;
        private uint _unknownFrame1;
        private DamageType _lastDamageType;
        private DamageData _lastDamage;
        private LogicFrame _lastDamagedAt;
        private uint _unknownFrame3;
        private bool _unknownBool;
        private bool _indestructible;
        private BitArray<ArmorSetCondition> _armorSetConditions = new();

        public override Fix64 MaxHealth { get; internal set; }

        public DamageData LastDamage => _lastDamage;

        internal ActiveBody(GameObject gameObject, GameContext context, ActiveBodyModuleData moduleData) : base(gameObject)
        {
            _context = context;
            _moduleData = moduleData;

            MaxHealth = (Fix64) moduleData.MaxHealth;

            SetHealth((Fix64) (moduleData.InitialHealth ?? moduleData.MaxHealth));
        }

        private void SetHealth(Fix64 value)
        {
            var takingDamage = value < Health;

            Health = value;
            if (Health < Fix64.Zero)
            {
                Health = Fix64.Zero;
            }

            if (Health > MaxHealth)
            {
                Health = MaxHealth;
            }

            GameObject.UpdateDamageFlags(HealthPercentage, takingDamage);
            _lastHealthBeforeDamage = _currentHealth;
            _currentHealth = (float)Health;
        }

        public override void SetInitialHealth(float multiplier)
        {
            SetHealth((Fix64) ((_moduleData.InitialHealth ?? _moduleData.MaxHealth) * multiplier));
            _lastHealthBeforeDamage = _currentHealth;
        }

        public override void DoDamage(DamageType damageType, Fix64 amount, DeathType deathType, GameObject damageDealer)
        {
            if (Health <= Fix64.Zero)
            {
                return;
            }

            var armorSet = GameObject.CurrentArmorSet;

            // Actual amount of damage depends on armor.
            var armor = armorSet.Armor.Value;
            var damagePercent = armor?.GetDamagePercent(damageType) ?? new Percentage(1.0f);
            var actualDamage = amount * (Fix64) (float) damagePercent;

            var takingDamage = damageType is not DamageType.Healing;

            var newHealth = takingDamage ? Health - actualDamage: Health + actualDamage;

            SetHealth(newHealth);

            var damageRequest = new DamageDataRequest(damageDealer?.ID ?? 0, damageType, deathType, (float)amount, damageDealer?.Definition.Name ?? string.Empty);
            var damageResult = new DamageDataResult((float)actualDamage, _lastHealthBeforeDamage - _currentHealth);

            _lastDamage = new DamageData(damageRequest, damageResult);
            _lastDamagedAt = _context.GameLogic.CurrentFrame;

            // TODO: DamageFX
            if (armorSet.DamageFX?.Value != null) //e.g. AmericaJetRaptor's ArmorSet has no DamageFX (None)
            {
                var damageFXGroup = armorSet.DamageFX.Value.GetGroup(damageType);

                // TODO: MajorFX
                var damageFX = damageFXGroup.MinorFX?.Value;
                damageFX?.Execute(
                    new FXListExecutionContext(
                        GameObject.Rotation,
                        GameObject.Translation,
                        GameObject.GameContext));
            }

            if (Health <= Fix64.Zero)
            {
                GameObject.Die(deathType);
            }
        }

        public override void Heal(Fix64 amount, GameObject healer)
        {
            DoDamage(DamageType.Healing, amount, DeathType.None, healer);
        }

        public override void Heal(Fix64 amount)
        {
            var newHealth = Health + amount;
            SetHealth(newHealth);
        }

        internal override void Load(StatePersister reader)
        {
            reader.PersistVersion(1);

            reader.BeginObject("Base");
            base.Load(reader);
            reader.EndObject();

            reader.PersistSingle(ref _currentHealth);
            reader.PersistSingle(ref _lastHealthBeforeDamage);
            reader.PersistSingle(ref _maxHealth);

            var maxHealth2 = _maxHealth;
            reader.PersistSingle(ref maxHealth2);

            if (reader.SageGame >= SageGame.CncGeneralsZeroHour)
            {
                var maxHealth3 = _maxHealth;
                reader.PersistSingle(ref maxHealth3);
                if (maxHealth3 != maxHealth2)
                {
                    throw new InvalidStateException();
                }
            }

            reader.PersistEnum(ref _damageType);
            reader.PersistFrame(ref _unknownFrame1);

            var lastDamageType = (uint)_lastDamageType;
            reader.PersistUInt32(ref lastDamageType);
            _lastDamageType = (DamageType)lastDamageType; // -1 if no last damage

            reader.PersistObject(ref _lastDamage);
            reader.PersistLogicFrame(ref _lastDamagedAt);
            reader.PersistFrame(ref _unknownFrame3);

            reader.SkipUnknownBytes(2);

            reader.PersistBoolean(ref _unknownBool);
            reader.PersistBoolean(ref _indestructible);

            reader.PersistList(
                _particleSystemIds,
                static (StatePersister persister, ref uint item) =>
                {
                    persister.PersistUInt32Value(ref item);
                });

            reader.PersistBitArray(ref _armorSetConditions);
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

        internal override BehaviorModule CreateModule(GameObject gameObject, GameContext context)
        {
            return new ActiveBody(gameObject, context, this);
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
