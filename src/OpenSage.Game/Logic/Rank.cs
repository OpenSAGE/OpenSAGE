
using OpenSage.Content;

namespace OpenSage.Logic
{
    public class Rank
    {
        private readonly Player _player;
        private RankTemplate _currentRank;
        private readonly ScopedAssetCollection<RankTemplate> _rankTemplates;
        public int CurrentRank => _currentRank.InternalId;

        public Rank(Player player, ScopedAssetCollection<RankTemplate> rankTemplates)
        {
            _player = player;
            _rankTemplates = rankTemplates;

            SetRank(1);
        }

        public void SetRank(int id)
        {
            _currentRank = _rankTemplates.GetByInternalId(id);
            if (_currentRank.SciencesGranted != null)
            {
                foreach (var science in _currentRank.SciencesGranted)
                {
                    _player.PurchaseScience(science.Value);
                    _player.SciencePurchasePoints += (uint) _currentRank.SciencePurchasePointsGranted;
                }
            }
        }

        public void Update()
        {
            if (_rankTemplates == null || _rankTemplates.Count == 0) return;

            if (_currentRank == null)
            {
                var firstRank = _rankTemplates.GetByIndex(0);
                SetRank(firstRank.InternalId);
            }

            if(_rankTemplates.Count == _currentRank.InternalId)
            {
                return;
            }

            var nextRankId = _currentRank.InternalId + 1;
            var nextRank = _rankTemplates.GetByInternalId(nextRankId);
            if (_player.SkillPointsAvailable >= nextRank.SkillPointsNeeded)
            {
                SetRank(nextRankId);
            }
        }
    }
}
