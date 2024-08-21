using System.Numerics;
using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    public class DeployStyleAIUpdate : AIUpdate
    {
        private bool _isMovingOrDeployed; // seems to be 1 whenever the nuke cannon is moving or deployed, even if not firing
        private LogicFrame _packCompleteFrame; // 0 when not deployed or completely deployed - used for packing and unpacking
        private DeploymentStatus _deploymentStatus;

        private uint _targetObjectId;

        private Vector3 _targetPosition;

        private bool _unknownBool1;
        private bool _unknownBool2;
        private bool _unknownBool3;

        private readonly UnknownStateData _unknownStateData = new();

        internal DeployStyleAIUpdate(GameObject gameObject, AIUpdateModuleData moduleData) : base(gameObject, moduleData)
        {
        }

        internal override void Load(StatePersister reader)
        {
            var version = reader.PersistVersion(4);

            reader.BeginObject("Base");
            base.Load(reader);
            reader.EndObject();

            if (version >= 4)
            {
                reader.PersistEnum(ref _deploymentStatus);
                reader.PersistLogicFrame(ref _packCompleteFrame);
            }
            else
            {
                reader.PersistBoolean(ref _isMovingOrDeployed);
                reader.PersistLogicFrame(ref _packCompleteFrame);
                reader.PersistEnum(ref _deploymentStatus);

                reader.SkipUnknownBytes(4);
                reader.PersistObjectID(ref _targetObjectId);
                reader.PersistVector3(ref _targetPosition);

                reader.PersistBoolean(ref _unknownBool1); // repositioning to fire at ground?
                reader.PersistBoolean(ref _unknownBool2); // repositioning to fire at target?
                reader.PersistBoolean(ref _unknownBool3); // fire when ready?

                reader.SkipUnknownBytes(2);

                reader.PersistObject(_unknownStateData);
            }
        }

        private enum DeploymentStatus
        {
            NotDeployed,
            Unpacking,
            Deployed,
            Packing,
            PreparingToPack, // recentering turret, specifically in order to begin packing
        }
    }

    /// <summary>
    /// Allows use of the PACKING and UNPACKING condition states and allows the use of Deploy and
    /// Undeploy within UnitSpecificSounds section of the object.
    /// </summary>
    public sealed class DeployStyleAIUpdateModuleData : AIUpdateModuleData
    {
        internal new static DeployStyleAIUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private new static readonly IniParseTable<DeployStyleAIUpdateModuleData> FieldParseTable = AIUpdateModuleData.FieldParseTable
            .Concat(new IniParseTable<DeployStyleAIUpdateModuleData>
            {
                { "PackTime", (parser, x) => x.PackTime = parser.ParseInteger() },
                { "UnpackTime", (parser, x) => x.UnpackTime = parser.ParseInteger() },
                { "TurretsFunctionOnlyWhenDeployed", (parser, x) => x.TurretsFunctionOnlyWhenDeployed = parser.ParseBoolean() },
                { "TurretsMustCenterBeforePacking", (parser, x) => x.TurretsMustCenterBeforePacking = parser.ParseBoolean() },
                { "ManualDeployAnimations", (parser, x) => x.ManualDeployAnimations = parser.ParseBoolean() },
                { "MustDeployToAttack", (parser, x) => x.MustDeployToAttack = parser.ParseBoolean() },
                { "DeployedAttributeModifier", (parser, x) => x.DeployedAttributeModifier = parser.ParseAssetReference() }
            });

        public int PackTime { get; private set; }
        public int UnpackTime { get; private set; }
        public bool TurretsFunctionOnlyWhenDeployed { get; private set; }
        public bool TurretsMustCenterBeforePacking { get; private set; }

        [AddedIn(SageGame.CncGeneralsZeroHour)]
        public bool ManualDeployAnimations { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public bool MustDeployToAttack { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public string DeployedAttributeModifier { get; private set; }

        internal override BehaviorModule CreateModule(GameObject gameObject, GameContext context)
        {
            return new DeployStyleAIUpdate(gameObject, this);
        }
    }
}
