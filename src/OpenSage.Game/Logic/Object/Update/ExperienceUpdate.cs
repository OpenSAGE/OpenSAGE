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
        private ExperienceLevel _nextLevel;
        private int _experienceForPreviousLevels;
        private BannerCarrierUpdate _bannerCarrierUpdate;

        internal ExperienceUpdate(GameObject gameObject)
        {
            _gameObject = gameObject;
            _initial = true;
            _experienceForPreviousLevels = 0;
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
                || _gameObject.ExperienceValue < _experienceForPreviousLevels + _nextLevel.RequiredExperience)
            {
                return;
            }

            _experienceForPreviousLevels += _nextLevel.RequiredExperience;
            _gameObject.GainExperience(_nextLevel.ExperienceAward);
            _gameObject.Rank = _nextLevel.Rank;

            if (_nextLevel.Upgrades != null)
            {
                foreach (var upgradeReference in _nextLevel.Upgrades)
                {
                    _gameObject.Upgrades.Add(context.GameContext.AssetLoadContext.AssetStore.Upgrades.GetByName(upgradeReference));
                }
            }

            if (_nextLevel.LevelUpFX != null)
            {
                var levelUpFx = context.GameContext.AssetLoadContext.AssetStore.FXLists.GetByName(_nextLevel.LevelUpFX);

                levelUpFx?.Execute(new FXListExecutionContext(
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
            // AttributeModifiers
            // EmotionType

            _experienceLevels.RemoveAt(0);
            if (_experienceLevels.Count > 0)
            {
                _nextLevel = _experienceLevels.First();
            }
        }

        private List<ExperienceLevel> FindRelevantExperienceLevels(BehaviorUpdateContext context)
        {
            var experienceLevels = context.GameContext.AssetLoadContext.AssetStore.ExperienceLevels;
            var levels = experienceLevels.Where(x => x.TargetNames.Contains(_gameObject.Definition.Name)).ToList();
            levels.Sort((x, y) => x.Rank.CompareTo(y.Rank));
            return levels.Count > 0 ? levels : null;
        }
    }
}
