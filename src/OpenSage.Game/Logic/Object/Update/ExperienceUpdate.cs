using System.Collections.Generic;
using System.Linq;
using OpenSage.FX;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme)]
    public class ExperienceUpdate : UpdateModule
    {
        private GameObject _gameObject;

        private List<ExperienceLevel> _experienceLevels;
        private bool _initial;
        private ExperienceLevel _currentLevel;
        private ExperienceLevel _nextLevel;
        private BannerCarrierUpdate _bannerCarrierUpdate;

        public bool ObjectGainsExperience { get; private set; }

        internal ExperienceUpdate(GameObject gameObject)
        {
            _gameObject = gameObject;
            _initial = true;
        }

        private void Initialize(BehaviorUpdateContext context)
        {
            // not sure why the required experience for rank 1 is 1 instead of 0
            if (_gameObject.ExperienceValue == 0)
            {
                _gameObject.ExperienceValue = 1;
            }

            _experienceLevels = FindRelevantExperienceLevels(context);
            if (_experienceLevels != null && _experienceLevels.Count > 0)
            {
                _nextLevel = _experienceLevels.First();
                _gameObject.ExperienceRequiredForNextLevel = _nextLevel.RequiredExperience;
                ObjectGainsExperience = true;
            }

            _bannerCarrierUpdate = _gameObject.FindBehavior<BannerCarrierUpdate>();
            _initial = false;
        }

        internal override void Update(BehaviorUpdateContext context)
        {
            if (_initial && _experienceLevels == null)
            {
                Initialize(context);
            }

            if (_experienceLevels == null
                || _experienceLevels.Count == 0
                || _gameObject.ExperienceValue < _nextLevel.RequiredExperience)
            {
                return;
            }

            _gameObject.ExperienceValue -= _nextLevel.RequiredExperience;
            _gameObject.Rank = _nextLevel.Rank;

            //remove stuff from current level
            if (_currentLevel != null)
            {
                if (_currentLevel.Upgrades != null)
                {
                    foreach (var upgrade in _currentLevel.Upgrades)
                    {
                        _gameObject.Upgrades.Remove(upgrade.Value);
                    }
                }

                if (_currentLevel.AttributeModifiers != null)
                {
                    foreach (var modifierList in _currentLevel.AttributeModifiers)
                    {
                        _gameObject.RemoveAttributeModifier(modifierList.Value.Name);
                    }
                }
            }


            if (_nextLevel.Upgrades != null)
            {
                foreach (var upgrade in _nextLevel.Upgrades)
                {
                    _gameObject.Upgrades.Add(upgrade.Value);
                }
            }

            if (_nextLevel.AttributeModifiers != null)
            {
                foreach (var modifierList in _nextLevel.AttributeModifiers)
                {
                    var attributeModifier = new AttributeModifier(modifierList.Value);
                    _gameObject.AddAttributeModifier(modifierList.Value.Name, attributeModifier);
                }
            }

            if (_nextLevel.LevelUpFX != null)
            {
                _nextLevel.LevelUpFX.Value?.Execute(new FXListExecutionContext(
                    context.GameObject.Rotation,
                    context.GameObject.Translation,
                    context.GameContext));
            }

            if (_nextLevel.InformUpdateModule)
            {
                _bannerCarrierUpdate?.NotifyLevelChanged();
            }

            if (_nextLevel.SelectionDecal != null)
            {
                _gameObject.SelectionDecal = _nextLevel.SelectionDecal;
            }

            // TODO:
            // ExperienceAwardOwnGuysDie -> what is this?
            // EmotionType

            _experienceLevels.RemoveAt(0);
            if (_experienceLevels.Count > 0)
            {
                _currentLevel = _nextLevel;
                _nextLevel = _experienceLevels.First();
                _gameObject.ExperienceRequiredForNextLevel = _nextLevel.RequiredExperience;
            }
            else
            {
                _gameObject.ExperienceRequiredForNextLevel = int.MaxValue;
            }
        }

        private List<ExperienceLevel> FindRelevantExperienceLevels(BehaviorUpdateContext context)
        {
            var experienceLevels = context.GameContext.AssetLoadContext.AssetStore.ExperienceLevels;
            var levels = experienceLevels.Where(x => x.TargetNames != null && x.TargetNames.Contains(_gameObject.Definition.Name)).ToList();
            levels.Sort((x, y) => x.Rank.CompareTo(y.Rank));
            return levels.Count > 0 ? levels : null;
        }
    }
}
