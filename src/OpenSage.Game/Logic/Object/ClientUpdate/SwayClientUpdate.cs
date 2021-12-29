using OpenSage.Client;
using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    public sealed class SwayClientUpdate : ClientUpdateModule
    {
        private float _unknownFloat1;
        private float _unknownFloat2;
        private float _unknownFloat3;
        private float _unknownFloat4;
        private float _unknownFloat5;
        private ushort _unknownShort;

        // TODO: Set this to false when ToppleUpdate starts toppling tree.
        private bool _isActive;

        internal SwayClientUpdate()
        {
            _isActive = true;
        }

        internal override void Load(SaveFileReader reader)
        {
            reader.ReadVersion(1);

            base.Load(reader);

            _unknownFloat1 = reader.ReadSingle();
            _unknownFloat2 = reader.ReadSingle();
            _unknownFloat3 = reader.ReadSingle();
            _unknownFloat4 = reader.ReadSingle();
            _unknownFloat5 = reader.ReadSingle();
            reader.ReadUInt16(ref _unknownShort);

            _isActive = reader.ReadBoolean();
        }
    }

    /// <summary>
    /// Allows the object to sway if enabled in GameData.INI or allowed by LOD/map specific settings.
    /// </summary>
    public sealed class SwayClientUpdateModuleData : ClientUpdateModuleData
    {
        internal static SwayClientUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<SwayClientUpdateModuleData> FieldParseTable = new IniParseTable<SwayClientUpdateModuleData>();

        internal override ClientUpdateModule CreateModule(Drawable drawable, GameContext context)
        {
            return new SwayClientUpdate();
        }
    }
}
