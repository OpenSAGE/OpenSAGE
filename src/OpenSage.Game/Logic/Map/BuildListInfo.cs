using System;
using System.IO;
using System.Numerics;
using OpenSage.Data.Ini;
using OpenSage.FileFormats;

namespace OpenSage.Logic.Map;

public sealed class BuildListInfo : IPersistableObject
{
    public const uint MaxResourceGatherers = 10;
    public const uint UnlimitedRebuilds = 0xFFFFFFFF;

    private string _buildingName = "";
    public string BuildingName { get => _buildingName; private set => _buildingName = value; }

    private string _templateName;
    public string TemplateName { get => _templateName; private set => _templateName = value; }

    private Vector3 _location;
    public Vector3 Location { get => _location; private set => _location = value; }

    private Vector2 _rallyPointOffset;
    public Vector2 RallyPointOffset { get => _rallyPointOffset; private set => _rallyPointOffset = value; }

    private float _angle;
    public float Angle { get => _angle; private set => _angle = value; }

    private bool _isInitiallyBuilt;
    public bool IsInitiallyBuilt { get => _isInitiallyBuilt; private set => _isInitiallyBuilt = value; }

    private uint _numRebuilds;
    public uint NumRebuilds { get => _numRebuilds; private set => _numRebuilds = value; }

    private string _script = "";
    public string Script { get => _script; private set => _script = value; }

    private int _health = 100;
    public int Health { get => _health; private set => _health = value; }

    // Unused in Generals / ZH
    // No idea what it was supposed to be used for.
    private bool _whiner = true;
    public bool Whiner { get => _whiner; private set => _whiner = value; }

    private bool _unsellable;
    public bool Unsellable { get => _unsellable; private set => _unsellable = value; }

    private bool _repairable = true;
    public bool Repairable { get => _repairable; private set => _repairable = value; }

    private bool _automaticallyBuild = true;
    public bool AutomaticallyBuild { get => _automaticallyBuild; private set => _automaticallyBuild = value; }

    private uint _objectId;
    public uint ObjectId { get => _objectId; private set => _objectId = value; }

    private uint _objectTimestamp;
    public uint ObjectTimestamp { get => _objectTimestamp; private set => _objectTimestamp = value; }

    private bool _underConstruction;
    public bool UnderConstruction { get => _underConstruction; private set => _underConstruction = value; }

    #region AI data

    // Unused in Generals / ZH
    private uint[] _resourceGatherers = new uint[MaxResourceGatherers];
    public uint[] ResourceGatherers { get => _resourceGatherers; private set => _resourceGatherers = value; }

    private bool _isSupplyBuilding;
    public bool IsSupplyBuilding { get => _isSupplyBuilding; private set => _isSupplyBuilding = value; }

    private int _desiredGatherers;
    public int DesiredGatherers { get => _desiredGatherers; private set => _desiredGatherers = value; }

    private int _currentGatherers;
    public int CurrentGatherers { get => _currentGatherers; private set => _currentGatherers = value; }

    private bool _priorityBuild;
    public bool PriorityBuild { get => _priorityBuild; private set => _priorityBuild = value; }

    #endregion

    [AddedIn(SageGame.Cnc3)]
    public bool Unknown1 { get; private set; }

    public BuildListInfo Duplicate()
    {
        // Since this is a shallow copy, ResourceGatherers will be shared between the two instances.
        // That could be a problem if ResourceGatherers was ever used.
        return (BuildListInfo)MemberwiseClone();
    }

    public void Persist(StatePersister reader)
    {
        reader.PersistVersion(2);

        reader.PersistAsciiString(ref _buildingName);
        reader.PersistAsciiString(ref _templateName);
        reader.PersistVector3(ref _location);
        reader.PersistVector2(ref _rallyPointOffset);
        reader.PersistSingle(ref _angle);
        reader.PersistBoolean(ref _isInitiallyBuilt);
        reader.PersistUInt32(ref _numRebuilds);
        reader.PersistAsciiString(ref _script);
        reader.PersistInt32(ref _health);
        reader.PersistBoolean(ref _whiner);
        reader.PersistBoolean(ref _unsellable);
        reader.PersistBoolean(ref _repairable);
        reader.PersistBoolean(ref _automaticallyBuild);
        reader.PersistObjectID(ref _objectId);
        reader.PersistUInt32(ref _objectTimestamp);
        reader.PersistBoolean(ref _underConstruction);
        reader.PersistArray(
            _resourceGatherers,
            static (StatePersister persister, ref uint value) => persister.PersistUInt32(ref value)
        );
        reader.PersistBoolean(ref _isSupplyBuilding);
        reader.PersistInt32(ref _desiredGatherers);
        reader.PersistBoolean(ref _priorityBuild);
        reader.PersistInt32(ref _currentGatherers);
    }

    [Flags]
    internal enum IncludeFields
    {
        Default = 0,

        [AddedIn(SageGame.Cnc3)]
        Cnc3UnknownBoolean = 1 << 0,
    }

    //internal static BuildListInfo Parse(BinaryReader reader, ushort version, ushort versionThatHasUnknownBoolean, bool mapHasAssetList)
    internal static BuildListInfo Parse(BinaryReader reader, IncludeFields fields)
    {
        var buildingName = reader.ReadUInt16PrefixedAsciiString();
        var templateName = reader.ReadUInt16PrefixedAsciiString();
        var location = reader.ReadVector3();
        var angle = reader.ReadSingle();
        var isInitiallyBuilt = reader.ReadBooleanChecked();

        var result = new BuildListInfo
        {
            BuildingName = buildingName,
            TemplateName = templateName,
            Location = location,
            Angle = angle,
            IsInitiallyBuilt = isInitiallyBuilt
        };

        // C&C3+
        // BFME and C&C3 both used v1 for this chunk, but C&C3 has an extra boolean here.
        // If the map file has an AssetList chunk, we assume it's C&C3.
        if (fields.HasFlag(IncludeFields.Cnc3UnknownBoolean))
        {
            result.Unknown1 = reader.ReadBooleanChecked();
        }

        result.NumRebuilds = reader.ReadUInt32();
        // The following fields are present in SidesList version 3 and later
        // Generals has a version check for this, but since every map we've encountered to date has this data,
        // we're going to assume it's always present.
        result.Script = reader.ReadUInt16PrefixedAsciiString();
        result.Health = reader.ReadInt32();
        result.Whiner = reader.ReadBooleanChecked();
        result.Unsellable = reader.ReadBooleanChecked();
        result.Repairable = reader.ReadBooleanChecked();

        return result;
    }

    internal void WriteTo(BinaryWriter writer, IncludeFields fields)
    {
        writer.WriteUInt16PrefixedAsciiString(BuildingName);
        writer.WriteUInt16PrefixedAsciiString(TemplateName);
        writer.Write(Location);
        writer.Write(Angle);
        writer.Write(IsInitiallyBuilt);

        if (fields.HasFlag(IncludeFields.Cnc3UnknownBoolean))
        {
            writer.Write(Unknown1);
        }

        writer.Write(NumRebuilds);
        writer.WriteUInt16PrefixedAsciiString(Script);
        writer.Write(Health);
        writer.Write(Whiner);
        writer.Write(Unsellable);
        writer.Write(Repairable);
    }

    internal static BuildListInfo Parse(IniParser parser)
    {
        return parser.ParseNamedBlock(
            (x, name) => x.TemplateName = name,
            FieldParseTable
        );
    }

    private static readonly IniParseTable<BuildListInfo> FieldParseTable = new()
    {
        { "Name", (parser, x) => x.BuildingName = parser.ParseString() },
        { "Location", (parser, x) => x.Location = new Vector3(parser.ParseVector2(), 0.0f) },
        { "Rebuilds", (parser, x) => {
            // INI files can have Rebuilds set to -1, which is the same as 0xFFFFFFFF (UnlimitedRebuilds)
            // TODO: If this pattern is common, consider adding ParseIntegerAsUnsigned
            var rebuilds = parser.ParseInteger();
            x.NumRebuilds = unchecked((uint) rebuilds);
        } },
        { "Angle", (parser, x) => x.Angle = parser.ParseAngleFloat() },
        { "InitiallyBuilt", (parser, x) => x.IsInitiallyBuilt = parser.ParseBoolean() },
        { "RallyPointOffset", (parser, x) => x.RallyPointOffset = parser.ParseVector2() },
        { "AutomaticallyBuild", (parser, x) => x.AutomaticallyBuild = parser.ParseBoolean() },
    };

    // These three are used by the AI

    public void DecrementNumRebuilds()
    {
        if (NumRebuilds > 0 && NumRebuilds != UnlimitedRebuilds)
        {
            NumRebuilds--;
        }
    }

    public void IncrementNumRebuilds()
    {
        if (NumRebuilds != UnlimitedRebuilds)
        {
            NumRebuilds++;
        }
    }

    public bool IsBuildable => NumRebuilds > 0 || NumRebuilds == UnlimitedRebuilds;
}
