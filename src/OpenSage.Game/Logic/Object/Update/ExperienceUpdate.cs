using System.Collections.Generic;
using System.Linq;
using OpenSage.FX;

namespace OpenSage.Logic.Object;

[AddedIn(SageGame.Bfme)]
public class ExperienceUpdate : UpdateModule
{
    private List<ExperienceLevel> _experienceLevels;
    private bool _initial;
    private ExperienceLevel _currentLevel;
    private ExperienceLevel _nextLevel;
    private BannerCarrierUpdate _bannerCarrierUpdate;

    public bool ObjectGainsExperience { get; private set; }

    internal ExperienceUpdate(GameObject gameObject, GameEngine gameEngine) : base(gameObject, gameEngine)
    {
        _initial = true;
    }

    private void Initialize(BehaviorUpdateContext context)
    {
        // not sure why the required experience for rank 1 is 1 instead of 0
        if (GameObject.ExperienceTracker.CurrentExperience == 0)
        {
            GameObject.ExperienceTracker.SetExperienceAndLevel(1);
        }

        _experienceLevels = FindRelevantExperienceLevels(context);
        if (_experienceLevels != null && _experienceLevels.Count > 0)
        {
            _nextLevel = _experienceLevels.First();
            GameObject.ExperienceRequiredForNextLevel = _nextLevel.RequiredExperience;
            ObjectGainsExperience = true;

            while ((int)GameObject.Rank >= _nextLevel.Rank)
            {
                levelUp();
            }
        }

        _bannerCarrierUpdate = GameObject.FindBehavior<BannerCarrierUpdate>();
        _initial = false;
    }

    internal override void Update(BehaviorUpdateContext context)
    {
        if (_initial && _experienceLevels == null)
        {
            Initialize(context);
        }

        if (_experienceLevels == null || _experienceLevels.Count == 0
            || GameObject.ExperienceTracker.CurrentExperience < _nextLevel.RequiredExperience)
        {
            return;
        }

        if (_nextLevel.LevelUpFX != null)
        {
            _nextLevel.LevelUpFX.Value?.Execute(new FXListExecutionContext(
                context.GameObject.Rotation,
                context.GameObject.Translation,
                context.GameEngine));
        }

        GameObject.ExperienceTracker.SetVeterancyLevel((VeterancyLevel)_nextLevel.Rank, false);

        levelUp();
    }

    private void levelUp()
    {
        RemoveUpgradesAndAttributeModifiersOfCurrentLevel();
        ApplyUpgradesAndAttributeModifiersForNextLevel();

        if (_nextLevel.InformUpdateModule)
        {
            _bannerCarrierUpdate?.NotifyLevelChanged();
        }

        if (_nextLevel.SelectionDecal != null)
        {
            GameObject.SelectionDecal = _nextLevel.SelectionDecal;
        }

        // TODO:
        // ExperienceAwardOwnGuysDie -> what is this?
        // EmotionType

        _experienceLevels.RemoveAt(0);
        if (_experienceLevels.Count > 0)
        {
            _currentLevel = _nextLevel;
            _nextLevel = _experienceLevels.First();
            GameObject.ExperienceRequiredForNextLevel = _nextLevel.RequiredExperience;
        }
        else
        {
            GameObject.ExperienceRequiredForNextLevel = int.MaxValue;
        }
    }

    private void RemoveUpgradesAndAttributeModifiersOfCurrentLevel()
    {
        if (_currentLevel == null)
        {
            return;
        }

        if (_currentLevel.Upgrades != null)
        {
            foreach (var upgrade in _currentLevel.Upgrades)
            {
                upgrade.Value.RemoveUpgrade(GameObject);
            }
        }

        if (_currentLevel.AttributeModifiers != null)
        {
            foreach (var modifierList in _currentLevel.AttributeModifiers)
            {
                GameObject.RemoveAttributeModifier(modifierList.Value.Name);
            }
        }
    }

    private void ApplyUpgradesAndAttributeModifiersForNextLevel()
    {
        if (_nextLevel.Upgrades != null)
        {
            foreach (var upgrade in _nextLevel.Upgrades)
            {
                upgrade.Value.GrantUpgrade(GameObject);
            }
        }

        if (_nextLevel.AttributeModifiers != null)
        {
            foreach (var modifierList in _nextLevel.AttributeModifiers)
            {
                var attributeModifier = new AttributeModifier(modifierList.Value);
                GameObject.AddAttributeModifier(modifierList.Value.Name, attributeModifier);
            }
        }
    }

    private List<ExperienceLevel> FindRelevantExperienceLevels(BehaviorUpdateContext context)
    {
        var experienceLevels = context.GameEngine.AssetLoadContext.AssetStore.ExperienceLevels;
        var levels = experienceLevels.Where(x => x.TargetNames != null && x.TargetNames.Contains(GameObject.Definition.Name)).ToList();
        levels.Sort((x, y) => x.Rank.CompareTo(y.Rank));
        return levels.Count > 0 ? levels : null;
    }
}
