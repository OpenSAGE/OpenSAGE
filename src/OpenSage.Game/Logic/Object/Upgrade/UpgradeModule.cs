using System.IO;
using ImGuiNET;
using OpenSage.Content;
using OpenSage.Data.Ini;
using OpenSage.FileFormats;

namespace OpenSage.Logic.Object
{
    public abstract class UpgradeModule : BehaviorModule
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

        private bool AnyUpgradeAvailable(LazyAssetReference<UpgradeTemplate>[] upgrades)
        {
            if (upgrades == null)
            {
                return false;
            }

            foreach (var trigger in upgrades)
            {
                if (_gameObject.UpgradeAvailable(trigger.Value))
                {
                    return true;
                }
            }
            return false;
        }

        private bool AllUpgradesAvailable(LazyAssetReference<UpgradeTemplate>[] upgrades)
        {
            if (upgrades == null)
            {
                return true;
            }

            foreach (var trigger in upgrades)
            {
                if (_gameObject.UpgradeAvailable(trigger.Value) == false)
                {
                    return false;
                }
            }
            return true;
        }

        internal override void Update(BehaviorUpdateContext context)
        {
            var triggered = _moduleData.RequiresAllTriggers ? AllUpgradesAvailable(_moduleData.TriggeredBy) : AnyUpgradeAvailable(_moduleData.TriggeredBy);
            var conflicts = _moduleData.RequiresAllConflictingTriggers ? AllUpgradesAvailable(_moduleData.ConflictsWith) : AnyUpgradeAvailable(_moduleData.ConflictsWith);
            if (conflicts)
            {
                triggered = false;
            }

            // what objects do use initial here?
            if (triggered != _triggered)
            {
                _triggered = triggered;
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
