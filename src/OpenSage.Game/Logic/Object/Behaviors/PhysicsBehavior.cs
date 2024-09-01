using System;
using System.Numerics;
using ImGuiNET;
using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    public class PhysicsBehavior : UpdateModule
    {
        private readonly GameObject _gameObject;
        private readonly PhysicsBehaviorModuleData _moduleData;
        private readonly Vector3 _gravityAcceleration;

        private Vector3 _unknownVector1;
        private Vector3 _acceleration;
        private Vector3 _lastAcceleration;
        private Vector3 _velocity;
        private int _unknownInt1;
        private uint _unknownInt2;
        private uint _unknownInt3;
        private float _mass;
        private uint _unknownInt4;
        private uint _unknownInt5;
        private LogicFrame _unknownFrame;
        private float _velocityMagnitude;
        private byte _unknownByte1;
        private byte _unknownByte2;

        public float Mass
        {
            get => _mass;
            set => _mass = value;
        }

        internal Vector3 UnknownVector => _unknownVector1;
        internal Vector3 Acceleration => _acceleration;
        internal Vector3 LastAcceleration => _lastAcceleration;
        internal Vector3 Velocity => _velocity;
        internal int UnknownInt1 => _unknownInt1;
        internal uint UnknownInt2 => _unknownInt2;
        internal uint UnknownInt3 => _unknownInt3;
        internal uint UnknownInt4 => _unknownInt4;
        internal uint UnknownInt5 => _unknownInt5;
        internal LogicFrame UnknownFrame => _unknownFrame;
        internal float VelocityMagnitude => _velocityMagnitude;
        internal byte UnknownByte1 => _unknownByte1;
        internal byte UnknownByte2 => _unknownByte2;

        internal PhysicsBehavior(GameObject gameObject, GameContext context, PhysicsBehaviorModuleData moduleData)
        {
            _gameObject = gameObject;
            _moduleData = moduleData;

            Mass = moduleData.Mass;

            var gravity = context.AssetLoadContext.AssetStore.GameData.Current.Gravity * moduleData.GravityMult;
            _gravityAcceleration = new Vector3(0, 0, gravity);
        }

        internal override void Update(BehaviorUpdateContext context)
        {
            // TODO: This probably isn't right. Aircraft locomotors should probably apply forces using this behavior
            // instead of modifying translations directly.
            if (_gameObject.Definition.KindOf.Get(ObjectKinds.Aircraft)
                || _gameObject.Definition.KindOf.Get(ObjectKinds.Drone)
                || _gameObject.Definition.KindOf.Get(ObjectKinds.GiantBird)
                || _gameObject.AIUpdate?.CurrentLocomotor?.LocomotorTemplate.Appearance == LocomotorAppearance.GiantBird)
            {
                if (_gameObject.ModelConditionFlags.Get(ModelConditionFlag.Dying) == false) return;
            }

            var acceleration = _gravityAcceleration + _acceleration;

            _lastAcceleration = _acceleration;

            _acceleration = Vector3.Zero;

            // Integrate velocity.
            _velocity += acceleration;

            // Integrate position.
            var newTranslation = _gameObject.Translation + _velocity;

            var terrainHeight = _gameObject.GameContext.Game.TerrainLogic.HeightMap.GetHeight(
                newTranslation.X,
                newTranslation.Y);

            if (newTranslation.Z < terrainHeight)
            {
                newTranslation.Z = terrainHeight;

                // TODO: Improve bouncing.
                _velocity.Z = Math.Abs(_velocity.Z) * 0.1f;

                if (_moduleData.KillWhenRestingOnGround && _velocity.Z < 0.1f)
                {
                    _gameObject.Kill(DeathType.Normal);
                }
            }

            _gameObject.SetTranslation(newTranslation);
        }

        public void AddForce(in Vector3 force)
        {
            _acceleration += force / _mass;
        }

        internal override void DrawInspector()
        {
            ImGui.InputFloat("Mass", ref _mass);
            ImGui.DragFloat3("Acceleration", ref _acceleration);
            ImGui.DragFloat3("Velocity", ref _velocity);
        }

        internal override void Load(StatePersister reader)
        {
            reader.PersistVersion(2);

            reader.BeginObject("Base");
            base.Load(reader);
            reader.EndObject();

            reader.PersistVector3(ref _unknownVector1);
            reader.PersistVector3(ref _acceleration);
            reader.PersistVector3(ref _lastAcceleration);
            reader.PersistVector3(ref _velocity);
            reader.PersistInt32(ref _unknownInt1);
            reader.PersistUInt32(ref _unknownInt2);
            reader.PersistUInt32(ref _unknownInt3);
            reader.PersistSingle(ref _mass);
            reader.PersistUInt32(ref _unknownInt4);
            reader.PersistUInt32(ref _unknownInt5);
            reader.PersistLogicFrame(ref _unknownFrame); // When object starts moving, this is set to current frame + 10

            reader.SkipUnknownBytes(6);
            reader.PersistByte(ref _unknownByte1); // 128 for supply drop zone crate parachute
            reader.PersistByte(ref _unknownByte2); // 63 for supply drop zone crate parachute

            reader.PersistSingle(ref _velocityMagnitude);
        }
    }

    public class PhysicsBehaviorModuleData : UpdateModuleData
    {
        internal static PhysicsBehaviorModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        internal static readonly IniParseTable<PhysicsBehaviorModuleData> FieldParseTable = new IniParseTable<PhysicsBehaviorModuleData>
        {
            { "Mass", (parser, x) => x.Mass = parser.ParseFloat() },
            { "AerodynamicFriction", (parser, x) => x.AerodynamicFriction = parser.ParseFloat() },
            { "ForwardFriction", (parser, x) => x.ForwardFriction = parser.ParseFloat() },
            { "CenterOfMassOffset", (parser, x) => x.CenterOfMassOffset = parser.ParseFloat() },
            { "AllowBouncing", (parser, x) => x.AllowBouncing = parser.ParseBoolean() },
            { "KillWhenRestingOnGround", (parser, x) => x.KillWhenRestingOnGround = parser.ParseBoolean() },
            { "AllowCollideForce", (parser, x) => x.AllowCollideForce = parser.ParseBoolean() },
            { "GravityMult", (parser, x) => x.GravityMult = parser.ParseFloat() },
            { "ShockStandingTime", (parser, x) => x.ShockStandingTime = parser.ParseInteger() },
            { "ShockStunnedTimeLow", (parser, x) => x.ShockStunnedTimeLow = parser.ParseInteger() },
            { "ShockStunnedTimeHigh", (parser, x) => x.ShockStunnedTimeHigh = parser.ParseInteger() },
            { "OrientToFlightPath", (parser, x) => x.OrientToFlightPath = parser.ParseBoolean() },
            { "FirstHeight", (parser, x) => x.FirstHeight = parser.ParseInteger() },
            { "SecondHeight", (parser, x) => x.SecondHeight = parser.ParseInteger() }
        };

        public float Mass { get; internal set; } = 1.0f;
        public float AerodynamicFriction { get; private set; }
        public float ForwardFriction { get; private set; }
        public float CenterOfMassOffset { get; private set; }
        public bool AllowBouncing { get; private set; }
        public bool KillWhenRestingOnGround { get; private set; }
        public bool AllowCollideForce { get; private set; } = true;

        [AddedIn(SageGame.Bfme)]
        public float GravityMult { get; private set; } = 1.0f;

        [AddedIn(SageGame.Bfme)]
        public int ShockStandingTime { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public int ShockStunnedTimeLow { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public int ShockStunnedTimeHigh { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public bool OrientToFlightPath { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public int FirstHeight { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public int SecondHeight { get; private set; }

        internal override BehaviorModule CreateModule(GameObject gameObject, GameContext context)
        {
            return new PhysicsBehavior(gameObject, context, this);
        }
    }
}
