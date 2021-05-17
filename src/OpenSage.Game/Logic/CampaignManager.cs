using OpenSage.Data.Sav;

namespace OpenSage.Logic
{
    public sealed class CampaignManager
    {
        private string _campaignName;
        private string _missionName;
        private uint _unknown;
        private uint _difficultyMaybe;

        internal void Load(SaveFileReader reader)
        {
            var version = reader.ReadVersion(5);

            _campaignName = reader.ReadAsciiString();
            _missionName = reader.ReadAsciiString();
            _unknown = reader.ReadUInt32();
            _difficultyMaybe = reader.ReadUInt32();

            if (version >= 5)
            {
                reader.__Skip(5);
            }
        }
    }
}
