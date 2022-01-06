using System.Collections.Generic;
using ImGuiNET;
using OpenSage.Content;
using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    internal abstract class UpgradeModule : BehaviorModule, IUpgradeableModule
    {
        protected readonly GameObject _gameObject;
        private readonly UpgradeModuleData _moduleData;
        private UpgradeLogic _upgradeLogic;

        internal bool Triggered => _upgradeLogic.Triggered;

        internal UpgradeModule(GameObject gameObject, UpgradeModuleData moduleData)
        {
            _gameObject = gameObject;
            _moduleData = moduleData;
            _upgradeLogic = new UpgradeLogic(moduleData.UpgradeData, this);
        }

        internal bool CanUpgrade(UpgradeSet existingUpgrades) => _upgradeLogic.CanUpgrade(existingUpgrades);

        internal override void Update(BehaviorUpdateContext context)
        {
            _upgradeLogic.Update(context);
        }

        void IUpgradeableModule.OnTrigger(BehaviorUpdateContext context, bool triggered)
        {
            OnTrigger(context, triggered);
        }

        internal virtual void OnTrigger(BehaviorUpdateContext context, bool triggered) { }

        internal override void Load(StatePersister reader)
        {
            reader.PersistVersion(1);

            reader.BeginObject("Base");
            base.Load(reader);
            reader.EndObject();

            reader.PersistObject("UpgradeLogic", _upgradeLogic);
        }

        internal override void DrawInspector()
        {
            _upgradeLogic.DrawInspector();
        }
    }

    internal interface IUpgradeableModule
    {
        void OnTrigger(BehaviorUpdateContext context, bool triggered);
    }

    internal sealed class UpgradeLogic : IPersistableObject
    {
        private readonly UpgradeLogicData _data;
        private readonly IUpgradeableModule _upgradeableModule;
        private bool _triggered;

        public bool Triggered => _triggered;

        public UpgradeLogic(UpgradeLogicData data, IUpgradeableModule upgradeableModule)
        {
            _data = data;
            _upgradeableModule = upgradeableModule;

            if (data.StartsActive)
            {
                //DoUpgrade();
                _triggered = data.StartsActive;
            }
        }

        public void Update(BehaviorUpdateContext context)
        {
            // TODO: This is expensive to do every single update.
            var canUpgrade = CanUpgrade(context.GameObject.GetUpgradesCompleted());

            // what objects do use initial here?
            if (canUpgrade != _triggered)
            {
                DoUpgrade(context);
            }
        }

        private void DoUpgrade(BehaviorUpdateContext context)
        {
            if (_triggered)
            {
                _triggered = true;
                _upgradeableModule.OnTrigger(context, _triggered);
            }
        }

        public bool CanUpgrade(UpgradeSet existingUpgrades)
        {
            if (_triggered)
            {
                return false;
            }

            return CanUpgradeImpl(existingUpgrades);
        }

        private bool CanUpgradeImpl(UpgradeSet existingUpgrades)
        {
            // Does the object / player have the prerequisite upgrades that trigger this upgrade?
            var triggered = _data.RequiresAllTriggers
                ? existingUpgrades.SetEquals(_data.TriggeredByHashSet)
                : existingUpgrades.Overlaps(_data.TriggeredByHashSet);

            if (!triggered)
            {
                return false;
            }

            // Does the object / player have any upgrades that conflict with this upgrade?
            var conflicts = _data.RequiresAllConflictingTriggers
                ? existingUpgrades.SetEquals(_data.ConflictsWithHashSet)
                : existingUpgrades.Overlaps(_data.ConflictsWithHashSet);

            if (conflicts)
            {
                return false;
            }

            return true;
        }

        public void Persist(StatePersister reader)
        {
            reader.PersistVersion(1);

            reader.PersistBoolean("Triggered", ref _triggered);
        }

        internal void DrawInspector()
        {
            ImGui.Checkbox("Triggered", ref _triggered);
        }
    }

    public abstract class UpgradeModuleData : BehaviorModuleData
    {
        public override ModuleKinds ModuleKinds => ModuleKinds.Upgrade;

        public UpgradeLogicData UpgradeData { get; } = new();

        internal static readonly IniParseTableChild<UpgradeModuleData, UpgradeLogicData> FieldParseTable = new IniParseTableChild<UpgradeModuleData, UpgradeLogicData>(x => x.UpgradeData, UpgradeLogicData.FieldParseTable);
    }

    public sealed class UpgradeLogicData
    {
        internal static readonly IniParseTable<UpgradeLogicData> FieldParseTable = new IniParseTable<UpgradeLogicData>
        {
            { "TriggeredBy", (parser, x) => x.TriggeredBy = parser.ParseUpgradeReferenceArray() },
            { "ConflictsWith", (parser, x) => x.ConflictsWith = parser.ParseUpgradeReferenceArray() },
            { "RequiresAllTriggers", (parser, x) => x.RequiresAllTriggers = parser.ParseBoolean() },
            { "RequiresAllConflictingTriggers", (parser, x) => x.RequiresAllConflictingTriggers = parser.ParseBoolean() },
            { "StartsActive", (parser, x) => x.StartsActive = parser.ParseBoolean() },
            { "Description", (parser, x) => x.Description = parser.ParseLocalizedStringKey() },
            { "CustomAnimAndDuration", (parser, x) => x.CustomAnimAndDuration = AnimAndDuration.Parse(parser) },
            { "ActiveDuringConstruction", (parser, x) => x.ActiveDuringConstruction = parser.ParseBoolean() },
        };

        public LazyAssetReference<UpgradeTemplate>[] TriggeredBy { get; internal set; }
        public LazyAssetReference<UpgradeTemplate>[] ConflictsWith { get; internal set; }
        public bool RequiresAllTriggers { get; internal set; }

        [AddedIn(SageGame.Bfme)]
        public bool RequiresAllConflictingTriggers { get; internal set; }

        [AddedIn(SageGame.Bfme)]
        public bool StartsActive { get; internal set; }

        [AddedIn(SageGame.Bfme)]
        public string Description { get; internal set; }

        [AddedIn(SageGame.Bfme)]
        public AnimAndDuration CustomAnimAndDuration { get; internal set; }

        [AddedIn(SageGame.Bfme)]
        public bool ActiveDuringConstruction { get; internal set; }

        private HashSet<UpgradeTemplate> _triggeredByHashSet;
        internal HashSet<UpgradeTemplate> TriggeredByHashSet
        {
            get
            {
                if (_triggeredByHashSet == null)
                {
                    _triggeredByHashSet = new HashSet<UpgradeTemplate>();
                    if (TriggeredBy != null)
                    {
                        foreach (var upgrade in TriggeredBy)
                        {
                            _triggeredByHashSet.Add(upgrade.Value);
                        }
                    }
                }

                return _triggeredByHashSet;
            }
        }

        private HashSet<UpgradeTemplate> _conflictsWithHashSet;
        internal HashSet<UpgradeTemplate> ConflictsWithHashSet
        {
            get
            {
                if (_conflictsWithHashSet == null)
                {
                    _conflictsWithHashSet = new HashSet<UpgradeTemplate>();
                    if (ConflictsWith != null)
                    {
                        foreach (var upgrade in ConflictsWith)
                        {
                            _conflictsWithHashSet.Add(upgrade.Value);
                        }
                    }
                }

                return _triggeredByHashSet;
            }
        }
    }

    [AddedIn(SageGame.Bfme)]
    public sealed class AnimAndDuration
    {
        internal static AnimAndDuration Parse(IniParser parser)
        {
            var result = new AnimAndDuration
            {
                AnimState = parser.ParseAttributeEnum<ModelConditionFlag>("AnimState"),
                AnimTime = parser.ParseAttributeInteger("AnimTime")
            };

            parser.ParseAttributeOptional("TriggerTime", parser.ParseInteger, out var triggerTime);
            result.TriggerTime = triggerTime;
            return result;
        }

        public ModelConditionFlag AnimState { get; private set; }
        public int AnimTime { get; private set; }
        public int TriggerTime { get; private set; }
    }
}
