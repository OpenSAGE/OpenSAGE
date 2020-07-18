using System.Linq;
using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    public abstract class UpgradeModule : BehaviorModule
    {
        internal bool _triggered;
        internal bool _initial = true;
        internal UpgradeModuleData _moduleData;

        internal UpgradeModule(UpgradeModuleData moduleData)
        {
            _moduleData = moduleData;
            _triggered = _moduleData.StartsActive;
        }

        internal override void Update(BehaviorUpdateContext context)
        {
            bool triggered = false;

            foreach (var trigger in _moduleData.TriggeredBy)
            {
                var upgrade = context.GameObject.Upgrades.FirstOrDefault(template => template.Name == trigger);

                if (upgrade != null)
                {
                    triggered = true;
                    if (_moduleData.RequiresAllTriggers == false)
                    {
                        break;
                    }
                    else
                    {
                        //TODO:
                    }
                }
                else if (_moduleData.RequiresAllTriggers == true)
                {
                    break;
                }
            }

            foreach(var conflict in _moduleData.ConflictsWith)
            {
                var upgrade = context.GameObject.Upgrades.FirstOrDefault(template => template.Name == conflict);

                if (upgrade != null)
                {
                    if (_moduleData.RequiresAllConflictingTriggers == false)
                    {
                        triggered = false;
                        break;
                    }
                    else
                    {
                        //TODO:
                    }
                }
                else if(_moduleData.RequiresAllConflictingTriggers == true)
                {
                    break;
                }
            }

            if (triggered != _triggered || _initial)
            {
                _initial = false;
                _triggered = triggered;
                OnTrigger(context, _triggered);
            }
        }

        internal virtual void OnTrigger(BehaviorUpdateContext context, bool triggered)
        {
            if(triggered)
            {

            }
        }

    }

    public abstract class UpgradeModuleData : BehaviorModuleData
    {
        internal static readonly IniParseTable<UpgradeModuleData> FieldParseTable = new IniParseTable<UpgradeModuleData>
        {
            { "TriggeredBy", (parser, x) => x.TriggeredBy = parser.ParseAssetReferenceArray() },
            { "ConflictsWith", (parser, x) => x.ConflictsWith = parser.ParseAssetReferenceArray() },
            { "RequiresAllTriggers", (parser, x) => x.RequiresAllTriggers = parser.ParseBoolean() },
            { "RequiresAllConflictingTriggers", (parser, x) => x.RequiresAllConflictingTriggers = parser.ParseBoolean() },
            { "StartsActive", (parser, x) => x.StartsActive = parser.ParseBoolean() },
            { "Description", (parser, x) => x.Description = parser.ParseLocalizedStringKey() },
            { "CustomAnimAndDuration", (parser, x) => x.CustomAnimAndDuration = AnimAndDuration.Parse(parser) },
            { "ActiveDuringConstruction", (parser, x) => x.ActiveDuringConstruction = parser.ParseBoolean() },
        };

        public string[] TriggeredBy { get; private set; }
        public string[] ConflictsWith { get; private set; }
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
