using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// Allows use of the PACKING and UNPACKING condition states and allows the use of Deploy and 
    /// Undeploy within UnitSpecificSounds section of the object.
    /// </summary>
    public sealed class DeployStyleAIUpdateModuleData : AIUpdateModuleData
    {
        internal static new DeployStyleAIUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<DeployStyleAIUpdateModuleData> FieldParseTable = AIUpdateModuleData.FieldParseTable
            .Concat(new IniParseTable<DeployStyleAIUpdateModuleData>
            {
                { "PackTime", (parser, x) => x.PackTime = parser.ParseInteger() },
                { "UnpackTime", (parser, x) => x.UnpackTime = parser.ParseInteger() },
                { "TurretsFunctionOnlyWhenDeployed", (parser, x) => x.TurretsFunctionOnlyWhenDeployed = parser.ParseBoolean() },
                { "TurretsMustCenterBeforePacking", (parser, x) => x.TurretsMustCenterBeforePacking = parser.ParseBoolean() },
                { "ManualDeployAnimations", (parser, x) => x.ManualDeployAnimations = parser.ParseBoolean() }
            });

        public int PackTime { get; private set; }
        public int UnpackTime { get; private set; }
        public bool TurretsFunctionOnlyWhenDeployed { get; private set; }
        public bool TurretsMustCenterBeforePacking { get; private set; }

        [AddedIn(SageGame.CncGeneralsZeroHour)]
        public bool ManualDeployAnimations { get; private set; }
    }
}
