using System.Collections.Generic;
using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    public sealed class NeutronMissileSlowDeathBehavior : ObjectBehavior
    {
        internal static NeutronMissileSlowDeathBehavior Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<NeutronMissileSlowDeathBehavior> FieldParseTable = new IniParseTable<NeutronMissileSlowDeathBehavior>
        {
            { "DestructionDelay", (parser, x) => x.DestructionDelay = parser.ParseInteger() },
            { "ScorchMarkSize", (parser, x) => x.ScorchMarkSize = parser.ParseInteger() },
            { "FXList", (parser, x) => x.FXList = parser.ParseAssetReference() },

            { "Blast1Enabled", (parser, x) => x.Blast1Enabled = parser.ParseBoolean() },
            { "Blast1Delay", (parser, x) => x.Blast1Delay = parser.ParseInteger() },
            { "Blast1ScorchDelay", (parser, x) => x.Blast1ScorchDelay = parser.ParseInteger() },
            { "Blast1InnerRadius", (parser, x) => x.Blast1InnerRadius = parser.ParseFloat() },
            { "Blast1OuterRadius", (parser, x) => x.Blast1OuterRadius = parser.ParseFloat() },
            { "Blast1MaxDamage", (parser, x) => x.Blast1MaxDamage = parser.ParseFloat() },
            { "Blast1MinDamage", (parser, x) => x.Blast1MinDamage = parser.ParseFloat() },
            { "Blast1ToppleSpeed", (parser, x) => x.Blast1ToppleSpeed = parser.ParseFloat() },
            { "Blast1PushForce", (parser, x) => x.Blast1PushForce = parser.ParseFloat() },

            { "Blast2Enabled", (parser, x) => x.Blast2Enabled = parser.ParseBoolean() },
            { "Blast2Delay", (parser, x) => x.Blast2Delay = parser.ParseInteger() },
            { "Blast2ScorchDelay", (parser, x) => x.Blast2ScorchDelay = parser.ParseInteger() },
            { "Blast2InnerRadius", (parser, x) => x.Blast2InnerRadius = parser.ParseFloat() },
            { "Blast2OuterRadius", (parser, x) => x.Blast2OuterRadius = parser.ParseFloat() },
            { "Blast2MaxDamage", (parser, x) => x.Blast2MaxDamage = parser.ParseFloat() },
            { "Blast2MinDamage", (parser, x) => x.Blast2MinDamage = parser.ParseFloat() },
            { "Blast2ToppleSpeed", (parser, x) => x.Blast2ToppleSpeed = parser.ParseFloat() },
            { "Blast2PushForce", (parser, x) => x.Blast2PushForce = parser.ParseFloat() },

            { "Blast3Enabled", (parser, x) => x.Blast3Enabled = parser.ParseBoolean() },
            { "Blast3Delay", (parser, x) => x.Blast3Delay = parser.ParseInteger() },
            { "Blast3ScorchDelay", (parser, x) => x.Blast3ScorchDelay = parser.ParseInteger() },
            { "Blast3InnerRadius", (parser, x) => x.Blast3InnerRadius = parser.ParseFloat() },
            { "Blast3OuterRadius", (parser, x) => x.Blast3OuterRadius = parser.ParseFloat() },
            { "Blast3MaxDamage", (parser, x) => x.Blast3MaxDamage = parser.ParseFloat() },
            { "Blast3MinDamage", (parser, x) => x.Blast3MinDamage = parser.ParseFloat() },
            { "Blast3ToppleSpeed", (parser, x) => x.Blast3ToppleSpeed = parser.ParseFloat() },
            { "Blast3PushForce", (parser, x) => x.Blast3PushForce = parser.ParseFloat() },

            { "Blast4Enabled", (parser, x) => x.Blast4Enabled = parser.ParseBoolean() },
            { "Blast4Delay", (parser, x) => x.Blast4Delay = parser.ParseInteger() },
            { "Blast4ScorchDelay", (parser, x) => x.Blast4ScorchDelay = parser.ParseInteger() },
            { "Blast4InnerRadius", (parser, x) => x.Blast4InnerRadius = parser.ParseFloat() },
            { "Blast4OuterRadius", (parser, x) => x.Blast4OuterRadius = parser.ParseFloat() },
            { "Blast4MaxDamage", (parser, x) => x.Blast4MaxDamage = parser.ParseFloat() },
            { "Blast4MinDamage", (parser, x) => x.Blast4MinDamage = parser.ParseFloat() },
            { "Blast4ToppleSpeed", (parser, x) => x.Blast4ToppleSpeed = parser.ParseFloat() },
            { "Blast4PushForce", (parser, x) => x.Blast4PushForce = parser.ParseFloat() },

            { "Blast5Enabled", (parser, x) => x.Blast5Enabled = parser.ParseBoolean() },
            { "Blast5Delay", (parser, x) => x.Blast5Delay = parser.ParseInteger() },
            { "Blast5ScorchDelay", (parser, x) => x.Blast5ScorchDelay = parser.ParseInteger() },
            { "Blast5InnerRadius", (parser, x) => x.Blast5InnerRadius = parser.ParseFloat() },
            { "Blast5OuterRadius", (parser, x) => x.Blast5OuterRadius = parser.ParseFloat() },
            { "Blast5MaxDamage", (parser, x) => x.Blast5MaxDamage = parser.ParseFloat() },
            { "Blast5MinDamage", (parser, x) => x.Blast5MinDamage = parser.ParseFloat() },
            { "Blast5ToppleSpeed", (parser, x) => x.Blast5ToppleSpeed = parser.ParseFloat() },
            { "Blast5PushForce", (parser, x) => x.Blast5PushForce = parser.ParseFloat() },

            { "Blast6Enabled", (parser, x) => x.Blast6Enabled = parser.ParseBoolean() },
            { "Blast6Delay", (parser, x) => x.Blast6Delay = parser.ParseInteger() },
            { "Blast6ScorchDelay", (parser, x) => x.Blast6ScorchDelay = parser.ParseInteger() },
            { "Blast6InnerRadius", (parser, x) => x.Blast6InnerRadius = parser.ParseFloat() },
            { "Blast6OuterRadius", (parser, x) => x.Blast6OuterRadius = parser.ParseFloat() },
            { "Blast6MaxDamage", (parser, x) => x.Blast6MaxDamage = parser.ParseFloat() },
            { "Blast6MinDamage", (parser, x) => x.Blast6MinDamage = parser.ParseFloat() },
            { "Blast6ToppleSpeed", (parser, x) => x.Blast6ToppleSpeed = parser.ParseFloat() },
            { "Blast6PushForce", (parser, x) => x.Blast6PushForce = parser.ParseFloat() },

            { "Blast7Enabled", (parser, x) => x.Blast7Enabled = parser.ParseBoolean() },
            { "Blast7Delay", (parser, x) => x.Blast7Delay = parser.ParseInteger() },
            { "Blast7ScorchDelay", (parser, x) => x.Blast7ScorchDelay = parser.ParseInteger() },
            { "Blast7InnerRadius", (parser, x) => x.Blast7InnerRadius = parser.ParseFloat() },
            { "Blast7OuterRadius", (parser, x) => x.Blast7OuterRadius = parser.ParseFloat() },
            { "Blast7MaxDamage", (parser, x) => x.Blast7MaxDamage = parser.ParseFloat() },
            { "Blast7MinDamage", (parser, x) => x.Blast7MinDamage = parser.ParseFloat() },
            { "Blast7ToppleSpeed", (parser, x) => x.Blast7ToppleSpeed = parser.ParseFloat() },
            { "Blast7PushForce", (parser, x) => x.Blast7PushForce = parser.ParseFloat() },

            { "Blast8Enabled", (parser, x) => x.Blast8Enabled = parser.ParseBoolean() },
            { "Blast8Delay", (parser, x) => x.Blast8Delay = parser.ParseInteger() },
            { "Blast8ScorchDelay", (parser, x) => x.Blast8ScorchDelay = parser.ParseInteger() },
            { "Blast8InnerRadius", (parser, x) => x.Blast8InnerRadius = parser.ParseFloat() },
            { "Blast8OuterRadius", (parser, x) => x.Blast8OuterRadius = parser.ParseFloat() },
            { "Blast8MaxDamage", (parser, x) => x.Blast8MaxDamage = parser.ParseFloat() },
            { "Blast8MinDamage", (parser, x) => x.Blast8MinDamage = parser.ParseFloat() },
            { "Blast8ToppleSpeed", (parser, x) => x.Blast8ToppleSpeed = parser.ParseFloat() },
            { "Blast8PushForce", (parser, x) => x.Blast8PushForce = parser.ParseFloat() },

            { "Blast9Enabled", (parser, x) => x.Blast9Enabled = parser.ParseBoolean() },
            { "Blast9Delay", (parser, x) => x.Blast9Delay = parser.ParseInteger() },
            { "Blast9ScorchDelay", (parser, x) => x.Blast9ScorchDelay = parser.ParseInteger() },
            { "Blast9InnerRadius", (parser, x) => x.Blast9InnerRadius = parser.ParseFloat() },
            { "Blast9OuterRadius", (parser, x) => x.Blast9OuterRadius = parser.ParseFloat() },
            { "Blast9MaxDamage", (parser, x) => x.Blast9MaxDamage = parser.ParseFloat() },
            { "Blast9MinDamage", (parser, x) => x.Blast9MinDamage = parser.ParseFloat() },
            { "Blast9ToppleSpeed", (parser, x) => x.Blast9ToppleSpeed = parser.ParseFloat() },
            { "Blast9PushForce", (parser, x) => x.Blast9PushForce = parser.ParseFloat() },

            { "OCL", (parser, x) => x.OCLs[parser.ParseEnum<SlowDeathStage>()] = parser.ParseAssetReference() },
        };

        public int DestructionDelay { get; private set; }
        public int ScorchMarkSize { get; private set; }
        public string FXList { get; private set; }

        public bool Blast1Enabled { get; private set; }
        public int Blast1Delay { get; private set; }
        public int Blast1ScorchDelay { get; private set; }
        public float Blast1InnerRadius { get; private set; }
        public float Blast1OuterRadius { get; private set; }
        public float Blast1MaxDamage { get; private set; }
        public float Blast1MinDamage { get; private set; }
        public float Blast1ToppleSpeed { get; private set; }
        public float Blast1PushForce { get; private set; }

        public bool Blast2Enabled { get; private set; }
        public int Blast2Delay { get; private set; }
        public int Blast2ScorchDelay { get; private set; }
        public float Blast2InnerRadius { get; private set; }
        public float Blast2OuterRadius { get; private set; }
        public float Blast2MaxDamage { get; private set; }
        public float Blast2MinDamage { get; private set; }
        public float Blast2ToppleSpeed { get; private set; }
        public float Blast2PushForce { get; private set; }

        public bool Blast3Enabled { get; private set; }
        public int Blast3Delay { get; private set; }
        public int Blast3ScorchDelay { get; private set; }
        public float Blast3InnerRadius { get; private set; }
        public float Blast3OuterRadius { get; private set; }
        public float Blast3MaxDamage { get; private set; }
        public float Blast3MinDamage { get; private set; }
        public float Blast3ToppleSpeed { get; private set; }
        public float Blast3PushForce { get; private set; }

        public bool Blast4Enabled { get; private set; }
        public int Blast4Delay { get; private set; }
        public int Blast4ScorchDelay { get; private set; }
        public float Blast4InnerRadius { get; private set; }
        public float Blast4OuterRadius { get; private set; }
        public float Blast4MaxDamage { get; private set; }
        public float Blast4MinDamage { get; private set; }
        public float Blast4ToppleSpeed { get; private set; }
        public float Blast4PushForce { get; private set; }

        public bool Blast5Enabled { get; private set; }
        public int Blast5Delay { get; private set; }
        public int Blast5ScorchDelay { get; private set; }
        public float Blast5InnerRadius { get; private set; }
        public float Blast5OuterRadius { get; private set; }
        public float Blast5MaxDamage { get; private set; }
        public float Blast5MinDamage { get; private set; }
        public float Blast5ToppleSpeed { get; private set; }
        public float Blast5PushForce { get; private set; }

        public bool Blast6Enabled { get; private set; }
        public int Blast6Delay { get; private set; }
        public int Blast6ScorchDelay { get; private set; }
        public float Blast6InnerRadius { get; private set; }
        public float Blast6OuterRadius { get; private set; }
        public float Blast6MaxDamage { get; private set; }
        public float Blast6MinDamage { get; private set; }
        public float Blast6ToppleSpeed { get; private set; }
        public float Blast6PushForce { get; private set; }

        public bool Blast7Enabled { get; private set; }
        public int Blast7Delay { get; private set; }
        public int Blast7ScorchDelay { get; private set; }
        public float Blast7InnerRadius { get; private set; }
        public float Blast7OuterRadius { get; private set; }
        public float Blast7MaxDamage { get; private set; }
        public float Blast7MinDamage { get; private set; }
        public float Blast7ToppleSpeed { get; private set; }
        public float Blast7PushForce { get; private set; }

        public bool Blast8Enabled { get; private set; }
        public int Blast8Delay { get; private set; }
        public int Blast8ScorchDelay { get; private set; }
        public float Blast8InnerRadius { get; private set; }
        public float Blast8OuterRadius { get; private set; }
        public float Blast8MaxDamage { get; private set; }
        public float Blast8MinDamage { get; private set; }
        public float Blast8ToppleSpeed { get; private set; }
        public float Blast8PushForce { get; private set; }

        public bool Blast9Enabled { get; private set; }
        public int Blast9Delay { get; private set; }
        public int Blast9ScorchDelay { get; private set; }
        public float Blast9InnerRadius { get; private set; }
        public float Blast9OuterRadius { get; private set; }
        public float Blast9MaxDamage { get; private set; }
        public float Blast9MinDamage { get; private set; }
        public float Blast9ToppleSpeed { get; private set; }
        public float Blast9PushForce { get; private set; }

        public Dictionary<SlowDeathStage, string> OCLs { get; } = new Dictionary<SlowDeathStage, string>();
    }
}
