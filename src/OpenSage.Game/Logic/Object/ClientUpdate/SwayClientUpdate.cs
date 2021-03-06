﻿using System.IO;
using OpenSage.Client;
using OpenSage.Data.Ini;
using OpenSage.FileFormats;

namespace OpenSage.Logic.Object
{
    public sealed class SwayClientUpdate : ClientUpdateModule
    {
        // TODO: Set this to false when ToppleUpdate starts toppling tree.
        private bool _isActive;

        internal SwayClientUpdate()
        {
            _isActive = true;
        }

        internal override void Load(BinaryReader reader)
        {
            var version = reader.ReadVersion();
            if (version != 1)
            {
                throw new InvalidDataException();
            }

            base.Load(reader);

            var unknownFloat1 = reader.ReadSingle();
            var unknownFloat2 = reader.ReadSingle();
            var unknownFloat3 = reader.ReadSingle();
            var unknownFloat4 = reader.ReadSingle();
            var unknownFloat5 = reader.ReadSingle();
            var unknownShort1 = reader.ReadUInt16();

            _isActive = reader.ReadBooleanChecked();
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
