﻿#nullable enable

using OpenSage.Content;
using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object;

public sealed class CreateCrateDie : DieModule
{
    private readonly CreateCrateDieModuleData _moduleData;

    internal CreateCrateDie(GameObject gameObject, IGameEngine gameEngine, CreateCrateDieModuleData moduleData)
        : base(gameObject, gameEngine, moduleData)
    {
        _moduleData = moduleData;
    }

    protected override void Die(in DamageInfoInput damageInput)
    {
        var crateData = _moduleData.CrateData?.Value;

        var killer = GameEngine.GameLogic.GetObjectById(damageInput.SourceID);

        if (crateData != null && KillerCanSpawnCrate(killer, crateData))
        {
            if (GameEngine.GameLogic.Random.NextSingle(0, 1) < crateData.CreationChance)
            {
                // actually create the crate
                float totalProbability = 0;
                var selection = GameEngine.GameLogic.Random.NextSingle(0, 1);
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

    private bool KillerCanSpawnCrate(GameObject? killer, CrateData crateData)
    {
        if (killer is null)
        {
            return crateData.KillerScience is null; // if we don't need to match killer science, everything else passes by default
        }

        return (!crateData.KilledByType.HasValue || killer.Definition.KindOf.Get(crateData.KilledByType.Value)) && // killer type ok
               killer.Team != GameObject.Team && // we can't generate our own salvage
               (crateData.VeterancyLevel is not { } v || v != GameObject.ExperienceTracker.VeterancyLevel) && // killer meets veterancy requirements
               (crateData.KillerScience is null || killer.Owner.HasScience(crateData.KillerScience.Value)); // killer owner meets science requirements
    }

    private void SpawnCrate(CrateObject crate, CrateData crateData)
    {
        if (crate.Object is not null)
        {
            var newCrate = GameEngine.GameLogic.CreateObject(crate.Object.Value, GameObject.Owner);
            newCrate.SetTransformMatrix(GameObject.TransformMatrix);

            if (crateData.OwnedByMaker)
            {
                newCrate.Team = GameObject.Team;
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

    public LazyAssetReference<CrateData>? CrateData { get; private set; }

    internal override CreateCrateDie CreateModule(GameObject gameObject, IGameEngine gameEngine)
    {
        return new CreateCrateDie(gameObject, gameEngine, this);
    }
}
