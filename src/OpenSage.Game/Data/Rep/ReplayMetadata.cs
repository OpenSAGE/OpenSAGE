using System;
using System.IO;
using OpenSage.Data.Utilities.Extensions;
using OpenSage.FileFormats;

namespace OpenSage.Data.Rep
{
    public sealed class ReplayMetadata
    {
        public int MapFileUnknownInt { get; private set; }
        public string MapFile { get; private set; }
        public int MapCrc { get; private set; }
        public int MapSize { get; private set; }

        // Might be seed.
        public int SD { get; private set; }

        public int C { get; private set; }

        public int SR { get; private set; }

        public int StartingCredits { get; private set; }

        public string O { get; private set; }

        public ReplaySlot[] Slots { get; private set; }

        internal static ReplayMetadata Parse(BinaryReader reader)
        {
            var raw = reader.ReadNullTerminatedAsciiString();
            var rawSplit = raw.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

            var result = new ReplayMetadata();

            foreach (var rawEntry in rawSplit)
            {
                var keyValue = rawEntry.Split('=');

                switch (keyValue[0])
                {
                    case "US":
                        break;

                    case "M":
                        result.MapFileUnknownInt = Convert.ToInt32(keyValue[1].Substring(0, 2));
                        result.MapFile = keyValue[1].Substring(2);
                        break;

                    case "MC":
                        result.MapCrc = Convert.ToInt32(keyValue[1], 16);
                        break;

                    case "MS":
                        result.MapSize = Convert.ToInt32(keyValue[1]);
                        break;

                    case "SD":
                        result.SD = Convert.ToInt32(keyValue[1]);
                        break;

                    case "C":
                        result.C = Convert.ToInt32(keyValue[1]);
                        break;

                    case "SR":
                        result.SR = Convert.ToInt32(keyValue[1]);
                        break;

                    case "SC":
                        result.StartingCredits = Convert.ToInt32(keyValue[1]);
                        break;

                    case "O":
                        result.O = keyValue[1];
                        break;

                    case "S":
                        var slots = keyValue[1].Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                        result.Slots = new ReplaySlot[slots.Length];
                        for (var i = 0; i < slots.Length; i++)
                        {
                            result.Slots[i] = ReplaySlot.Parse(slots[i]);
                        }
                        
                        break;

                    default:
                        throw new NotImplementedException($"Unexpected replay metadata key: '{keyValue[0]}'.");
                }
            }

            return result;
        }
    }

    public sealed class ReplaySlot
    {
        public ReplaySlotType SlotType { get; private set; }

        public string HumanName { get; private set; }
        public ReplaySlotDifficulty? ComputerDifficulty { get; private set; }

        public ReplaySlotColor Color { get; private set; }
        public int Faction { get; private set; }
        public int StartPosition { get; private set; }
        public int Team { get; private set; }

        // HDESKTOP-J8EU7T4,0,0,TT,-1,2,-1,-1,1:
        // CH,-1,-1,-1,-1:
        // CH,-1,-1,-1,-1:
        // CH,-1,-1,-1,-1:
        // X:
        // X:
        // X:
        // X:
        internal static ReplaySlot Parse(string raw)
        {
            var result = new ReplaySlot();

            ReplaySlotType getSlotType()
            {
                switch (raw[0])
                {
                    case 'H':
                        return ReplaySlotType.Human;
                    case 'C':
                        return ReplaySlotType.Computer;
                    case 'X':
                        return ReplaySlotType.Empty;
                    default:
                        throw new InvalidDataException();
                }
            }

            ReplaySlotDifficulty getSlotDifficulty()
            {
                switch (raw[1])
                {
                    case 'E':
                        return ReplaySlotDifficulty.Easy;
                    case 'M':
                        return ReplaySlotDifficulty.Medium;
                    case 'H':
                        return ReplaySlotDifficulty.Hard;
                    default:
                        throw new InvalidDataException();
                }
            }

            result.SlotType = getSlotType();

            var slotDetails = raw.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            switch (result.SlotType)
            {
                case ReplaySlotType.Human:
                    result.HumanName = slotDetails[0].Substring(1);
                    // TODO: 1, 2, 3, 4
                    result.Faction = Convert.ToInt32(slotDetails[5]);
                    result.StartPosition = Convert.ToInt32(slotDetails[6]);
                    result.Team = Convert.ToInt32(slotDetails[7]);
                    // TODO: 8
                    break;

                case ReplaySlotType.Computer:
                    result.ComputerDifficulty = getSlotDifficulty();
                    result.Color = (ReplaySlotColor) Convert.ToInt32(slotDetails[1]);
                    result.Faction = Convert.ToInt32(slotDetails[2]);
                    result.StartPosition = Convert.ToInt32(slotDetails[3]);
                    result.Team = Convert.ToInt32(slotDetails[4]);
                    break;

                case ReplaySlotType.Empty:
                    break;
            }

            return result;
        }
    }

    public enum ReplaySlotType
    {
        Human,
        Computer,
        Empty
    }

    public enum ReplaySlotDifficulty
    {
        Easy,
        Medium,
        Hard
    }

    // TODO: Probably only correct for Generals and Zero Hour
    public enum ReplaySlotColor
    {
        Random = -1,
        Gold = 0,
        Red,
        Blue,
        Green,
        Orange,
        Cyan,
        Purple,
        Pink
    }
}
