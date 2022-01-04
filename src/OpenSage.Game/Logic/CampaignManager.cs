namespace OpenSage.Logic
{
    public sealed class CampaignManager
    {
        public string CampaignName;
        public string MissionName;

        private uint _unknown;
        private uint _difficultyMaybe;

        private bool _unknownBool1;
        private bool _unknownBool2;

        internal void Load(StatePersister reader)
        {
            var version = reader.PersistVersion(5);

            reader.PersistAsciiString("CampaignName", ref CampaignName);
            reader.PersistAsciiString("MissionName", ref MissionName);
            reader.PersistUInt32("Unknown", ref _unknown);
            reader.PersistUInt32("DifficultyMaybe", ref _difficultyMaybe);

            if (version >= 5)
            {
                reader.PersistBoolean("UnknownBool1", ref _unknownBool1);

                reader.SkipUnknownBytes(4);
            }

            if (version == 1 && reader.SageGame >= SageGame.Bfme)
            {
                reader.PersistBoolean("UnknownBool2", ref _unknownBool2);
            }
        }
    }
}
