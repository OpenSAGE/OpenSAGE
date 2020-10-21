﻿using System.IO;
using OpenSage.Data.Ini;
using OpenSage.FileFormats;

namespace OpenSage.Logic.Object
{
    internal sealed class ExperienceScalarUpgrade : UpgradeModule
    {
        private readonly ExperienceScalarUpgradeModuleData _moduleData;

        internal ExperienceScalarUpgrade(GameObject gameObject, ExperienceScalarUpgradeModuleData moduleData)
            : base(gameObject, moduleData)
        {
            _moduleData = moduleData;
        }

        internal override void OnTrigger(BehaviorUpdateContext context, bool triggered)
        {
            if (triggered)
            {
                _gameObject.ExperienceMultiplier += _moduleData.AddXPScalar;
            }
        }

        internal override void Load(BinaryReader reader)
        {
            var version = reader.ReadVersion();
            if (version != 1)
            {
                throw new InvalidDataException();
            }

            base.Load(reader);
        }
    }

    public sealed class ExperienceScalarUpgradeModuleData : UpgradeModuleData
    {
        internal static ExperienceScalarUpgradeModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<ExperienceScalarUpgradeModuleData> FieldParseTable = UpgradeModuleData.FieldParseTable
            .Concat(new IniParseTable<ExperienceScalarUpgradeModuleData>
            {
                { "AddXPScalar", (parser, x) => x.AddXPScalar = parser.ParseFloat() }
            });

        public float AddXPScalar { get; private set; }

        internal override BehaviorModule CreateModule(GameObject gameObject, GameContext context)
        {
            return new ExperienceScalarUpgrade(gameObject, this);
        }
    }
}
