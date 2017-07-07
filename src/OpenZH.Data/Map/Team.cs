using System.IO;
using OpenZH.Data.Utilities.Extensions;

namespace OpenZH.Data.Map
{
    public sealed class Team
    {
        public string Name { get; private set; }
        public uint MaxInstances { get; private set; }
        public string Owner { get; private set; }
        public string Home { get; private set; }
        public bool IsSingleton { get; private set; }
        public bool ExecuteAssociatedActions { get; private set; }
        public uint ProductionPriority { get; private set; }
        public uint ProductionPrioritySuccessIncrease { get; private set; }
        public uint ProductionPriorityFailureDecrease { get; private set; }
        public uint InitialIdleFrames { get; private set; }
        public UnitDescription[] Units { get; } = new UnitDescription[7];
        public string Description { get; private set; }

        public static Team Parse(BinaryReader reader, MapParseContext context)
        {
            var numProperties = reader.ReadUInt16();

            var result = new Team();

            for (var i = 0; i < numProperties; i++)
            {
                var propertyType = reader.ReadUInt32();
                var propertyName = context.AssetNames[propertyType >> 8];

                switch (propertyName)
                {
                    case "teamName":
                        result.Name = reader.ReadUInt16PrefixedAsciiString();
                        break;

                    case "teamOwner":
                        result.Owner = reader.ReadUInt16PrefixedAsciiString();
                        break;

                    case "teamIsSingleton":
                        result.IsSingleton = reader.ReadBoolean();
                        break;

                    case "teamUnitType1":
                    case "teamUnitType2":
                    case "teamUnitType3":
                    case "teamUnitType4":
                    case "teamUnitType5":
                    case "teamUnitType6":
                    case "teamUnitType7":
                        GetUnitDescription(result, propertyName).UnitType = reader.ReadUInt16PrefixedAsciiString();
                        break;

                    case "teamUnitMaxCount1":
                    case "teamUnitMaxCount2":
                    case "teamUnitMaxCount3":
                    case "teamUnitMaxCount4":
                    case "teamUnitMaxCount5":
                    case "teamUnitMaxCount6":
                    case "teamUnitMaxCount7":
                        GetUnitDescription(result, propertyName).MaxCount = reader.ReadUInt32();
                        break;

                    case "teamUnitMinCount1":
                    case "teamUnitMinCount2":
                    case "teamUnitMinCount3":
                    case "teamUnitMinCount4":
                    case "teamUnitMinCount5":
                    case "teamUnitMinCount6":
                    case "teamUnitMinCount7":
                        GetUnitDescription(result, propertyName).MinCount = reader.ReadUInt32();
                        break;

                    case "teamProductionPriority":
                        result.ProductionPriority = reader.ReadUInt32();
                        break;

                    case "teamDescription":
                        result.Description = reader.ReadUInt16PrefixedAsciiString();
                        break;

                    case "teamMaxInstances":
                        result.MaxInstances = reader.ReadUInt32();
                        break;

                    case "teamProductionPrioritySuccessIncrease":
                        result.ProductionPrioritySuccessIncrease = reader.ReadUInt32();
                        break;

                    case "teamProductionPriorityFailureDecrease":
                        result.ProductionPriorityFailureDecrease = reader.ReadUInt32();
                        break;

                    case "teamInitialIdleFrames":
                        result.InitialIdleFrames = reader.ReadUInt32();
                        break;

                    case "teamExecutesActionsOnCreate":
                        result.ExecuteAssociatedActions = reader.ReadBoolean();
                        break;

                    case "teamHome":
                        result.Home = reader.ReadUInt16PrefixedAsciiString();
                        break;

                    default:
                        throw new InvalidDataException($"Unexpected property name: {propertyName}");
                }
            }
            
            return result;
        }

        private static int GetUnitIndex(string propertyName)
        {
            return int.Parse(propertyName.Substring(propertyName.Length - 1)) - 1;
        }

        private static UnitDescription GetUnitDescription(Team team, string propertyName)
        {
            var index = GetUnitIndex(propertyName);
            return team.Units[index] ?? (team.Units[index] = new UnitDescription());
        }

        public sealed class UnitDescription
        {
            public string UnitType { get; set; }
            public uint MinCount { get; set; }
            public uint MaxCount { get; set; }
        }
    }
}
