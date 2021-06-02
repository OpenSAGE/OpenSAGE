using OpenSage.Data.Sav;

namespace OpenSage.Logic
{
    public sealed class CampaignManager
    {
        public string CampaignName { get; private set; }
        public string MissionName { get; private set; }

        private uint _unknown;
        private uint _difficultyMaybe;

        internal void Load(SaveFileReader reader)
        {
            var version = reader.ReadVersion(5);

            CampaignName = reader.ReadAsciiString();
            MissionName = reader.ReadAsciiString();
            _unknown = reader.ReadUInt32();
            _difficultyMaybe = reader.ReadUInt32();

            if (version >= 5)
            {
                reader.__Skip(5);
            }
        }
    }
}
