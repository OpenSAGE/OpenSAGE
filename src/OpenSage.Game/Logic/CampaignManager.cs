﻿namespace OpenSage.Logic
{
    public sealed class CampaignManager : IPersistableObject
    {
        public string CampaignName;
        public string MissionName;

        private uint _unknown;
        private uint _difficultyMaybe;

        private bool _unknownBool1;
        private bool _unknownBool2;

        public void Persist(StatePersister reader)
        {
            var version = reader.PersistVersion(5);

            reader.PersistAsciiString(ref CampaignName);
            reader.PersistAsciiString(ref MissionName);
            reader.PersistUInt32(ref _unknown);
            reader.PersistUInt32(ref _difficultyMaybe);

            if (version >= 5)
            {
                reader.PersistBoolean(ref _unknownBool1);

                reader.SkipUnknownBytes(4);
            }

            if (version == 1 && reader.SageGame >= SageGame.Bfme)
            {
                reader.PersistBoolean(ref _unknownBool2);
            }
        }
    }
}
