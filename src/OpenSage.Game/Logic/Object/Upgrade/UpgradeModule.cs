using System.Collections.Generic;
using System.IO;
using ImGuiNET;
using OpenSage.Content;
using OpenSage.Data.Ini;
using OpenSage.FileFormats;

namespace OpenSage.Logic.Object
{
    internal abstract class UpgradeModule : BehaviorModule
    {
        protected readonly GameObject _gameObject;
        private readonly UpgradeModuleData _moduleData;
        protected bool _triggered;

        internal bool Triggered => _triggered;

        internal UpgradeModule(GameObject gameObject, UpgradeModuleData moduleData)
        {
            _gameObject = gameObject;
            _moduleData = moduleData;
            _triggered = _moduleData.StartsActive;
        }

        public bool CanUpgrade(HashSet<string> existingUpgrades)
        {
            if (_triggered)
            {
                return false;
            }

            return CanUpgradeImpl(existingUpgrades);
        }

        private bool CanUpgradeImpl(HashSet<string> existingUpgrades)
        {
            // Does the object / player have the prerequisite upgrades that trigger this upgrade?
            var triggered = _moduleData.RequiresAllTriggers
                ? existingUpgrades.SetEquals(_moduleData.TriggeredByHashSet)
                : existingUpgrades.Overlaps(_moduleData.TriggeredByHashSet);

            if (!triggered)
            {
                return false;
            }

            // Does the object / player have any upgrades that conflict with this upgrade?
            var conflicts = _moduleData.RequiresAllConflictingTriggers
                ? existingUpgrades.SetEquals(_moduleData.ConflictsWithHashSet)
                : existingUpgrades.Overlaps(_moduleData.ConflictsWithHashSet);

            if (conflicts)
            {
                return false;
            }

            return true;
        }

        internal override void Update(BehaviorUpdateContext context)
        {
            // TODO: This is expensive to do every single update.
            var canUpgrade = CanUpgrade(_gameObject.GetUpgradesCompleted());

            // what objects do use initial here?
            if (canUpgrade != _triggered)
            {
                _triggered = canUpgrade;
                OnTrigger(context, _triggered);
            }
        }

        internal virtual void OnTrigger(BehaviorUpdateContext context, bool triggered) { }

        internal override void Load(BinaryReader reader)
        {
            var version = reader.ReadVersion();
            if (version != 1)
            {
                throw new InvalidDataException();
            }

            base.Load(reader);

            var unknownByte1 = reader.ReadByte();
            var unknownByte2 = reader.ReadByte();
        }

        internal override void DrawInspector()
        {
            ImGui.Checkbox("Triggered", ref _triggered);
        }
    }

    public abstract class UpgradeModuleData : BehaviorModuleData
    {
        public override ModuleKinds ModuleKinds => ModuleKinds.Upgrade;

        internal static readonly IniParseTable<UpgradeModuleData> FieldParseTable = new IniParseTable<UpgradeModuleData>
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

        public LazyAssetReference<UpgradeTemplate>[] TriggeredBy { get; private set; }
        public LazyAssetReference<UpgradeTemplate>[] ConflictsWith { get; private set; }
        public bool RequiresAllTriggers { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public bool RequiresAllConflictingTriggers { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public bool StartsActive { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public string Description { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public AnimAndDuration CustomAnimAndDuration { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public bool ActiveDuringConstruction { get; private set; }

        private HashSet<string> _triggeredByHashSet;
        internal HashSet<string> TriggeredByHashSet
        {
            get
            {
                if (_triggeredByHashSet == null)
                {
                    _triggeredByHashSet = new HashSet<string>();
                    if (TriggeredBy != null)
                    {
                        foreach (var upgrade in TriggeredBy)
                        {
                            _triggeredByHashSet.Add(upgrade.Value.Name);
                        }
                    }
                }

                return _triggeredByHashSet;
            }
        }

        private HashSet<string> _conflictsWithHashSet;
        internal HashSet<string> ConflictsWithHashSet
        {
            get
            {
                if (_conflictsWithHashSet == null)
                {
                    _conflictsWithHashSet = new HashSet<string>();
                    if (ConflictsWith != null)
                    {
                        foreach (var upgrade in ConflictsWith)
                        {
                            _conflictsWithHashSet.Add(upgrade.Value.Name);
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
