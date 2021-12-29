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
            var version = reader.ReadVersion(5);

            reader.ReadAsciiString(ref CampaignName);
            reader.ReadAsciiString(ref MissionName);
            reader.ReadUInt32(ref _unknown);
            reader.ReadUInt32(ref _difficultyMaybe);

            if (version >= 5)
            {
                reader.ReadBoolean(ref _unknownBool1);

                reader.SkipUnknownBytes(4);
            }

            if (version == 1 && reader.SageGame >= SageGame.Bfme)
            {
                reader.ReadBoolean(ref _unknownBool2);
            }
        }
    }
}
