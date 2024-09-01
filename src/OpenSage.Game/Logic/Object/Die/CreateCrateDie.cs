#nullable enable

using OpenSage.Content;
using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    public sealed class CreateCrateDie : DieModule
    {
        private readonly GameObject _gameObject;
        private readonly GameContext _context;
        private readonly CreateCrateDieModuleData _moduleData;

        internal CreateCrateDie(GameObject gameObject, GameContext context, CreateCrateDieModuleData moduleData) : base(moduleData)
        {
            _gameObject = gameObject;
            _context = context;
            _moduleData = moduleData;
        }

        private protected override void Die(BehaviorUpdateContext context, DeathType deathType)
        {
            var crateData = _moduleData.CrateData.Value;

            if (_gameObject.TryGetLastDamage(out var lastDamageData))
            {
                var killer = _context.GameLogic.GetObjectById(lastDamageData.Request.DamageDealer);

                if (KillerCanSpawnCrate(killer, crateData))
                {
                    if (_context.Random.NextSingle() < crateData.CreationChance)
                    {
                        // actually create the crate
                        float totalProbability = 0;
                        var selection = _context.Random.NextSingle();
                        foreach (var crate in crateData.CrateObjects)
                        {
                            totalProbability += crate.Probability;
                            if (totalProbability > selection)
                            {
                                SpawnCrate(crate, crateData);
                                break;
                            }
                        }
                    }
                }
            }

            base.Die(context, deathType);
        }

        private bool KillerCanSpawnCrate(GameObject? killer, CrateData crateData)
        {
            if (killer is null)
            {
                return crateData.KillerScience is null; // if we don't need to match killer science, everything else passes by default
            }

            return (!crateData.KilledByType.HasValue || killer.Definition.KindOf.Get(crateData.KilledByType.Value)) && // killer type ok
                   killer.Team != _gameObject.Team && // we can't generate our own salvage
                   (crateData.VeterancyLevel is not { } v || v != _gameObject.VeterancyHelper.VeterancyLevel) && // killer meets veterancy requirements
                   (crateData.KillerScience is null || killer.Owner.HasScience(crateData.KillerScience.Value)); // killer owner meets science requirements
        }

        private void SpawnCrate(CrateObject crate, CrateData crateData)
        {
            if (crate.Object is not null)
            {
                var newCrate = _context.GameLogic.CreateObject(crate.Object.Value, _gameObject.Owner);
                newCrate.SetTransformMatrix(_gameObject.TransformMatrix);

                if (crateData.OwnedByMaker)
                {
                    newCrate.Team = _gameObject.Team;
                }
            }
        }

        internal override void Load(StatePersister reader)
        {
            reader.PersistVersion(1);

            reader.BeginObject("Base");
            base.Load(reader);
            reader.EndObject();
        }
    }

    public sealed class CreateCrateDieModuleData : DieModuleData
    {
        internal static CreateCrateDieModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<CreateCrateDieModuleData> FieldParseTable = DieModuleData.FieldParseTable
            .Concat(new IniParseTable<CreateCrateDieModuleData>
            {
                { "CrateData", (parser, x) => x.CrateData = parser.ParseCrateReference() }
            });

        public LazyAssetReference<CrateData> CrateData { get; private set; }

        internal override CreateCrateDie CreateModule(GameObject gameObject, GameContext context)
        {
            return new CreateCrateDie(gameObject, context, this);
        }
    }
}
