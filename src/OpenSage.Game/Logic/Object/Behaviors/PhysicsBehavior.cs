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

        private float _mass;

        // TODO: Don't know if this belongs here.
        private Vector3 _velocity;

        private Vector3 _cumulativeForces;

        private Vector3 _unknownVector1;
        private Vector3 _unknownVector2;
        private Vector3 _unknownVector3;
        private Vector3 _unknownVector4;
        private int _unknownInt1;
        private uint _unknownInt2;
        private uint _unknownInt3;
        private uint _unknownInt4;
        private uint _unknownInt5;
        private uint _unknownFrame;
        private float _unknownFloat1;

        public float Mass
        {
            get => _mass;
            set => _mass = value;
        }

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
            if (_gameObject.Definition.KindOf.Get(ObjectKinds.Aircraft)
                || _gameObject.Definition.KindOf.Get(ObjectKinds.Drone)
                || _gameObject.Definition.KindOf.Get(ObjectKinds.GiantBird)
                || _gameObject.AIUpdate?.CurrentLocomotor?.LocomotorTemplate.Appearance == LocomotorAppearance.GiantBird)
            {
                if (_gameObject.ModelConditionFlags.Get(ModelConditionFlag.Dying) == false) return;
            }

            var cumulativeAcceleration = _cumulativeForces / Mass;
            _cumulativeForces = Vector3.Zero;

            var acceleration = _gravityAcceleration + cumulativeAcceleration;

            // Integrate velocity.
            var deltaTime = (float) context.Time.DeltaTime.TotalSeconds;
            _velocity += acceleration * deltaTime;

            // Integrate position.
            var newTranslation = context.GameObject.Translation + (_velocity * deltaTime);

            var terrainHeight = context.GameContext.Terrain.HeightMap.GetHeight(
                newTranslation.X,
                newTranslation.Y);

            if (newTranslation.Z < terrainHeight)
            {
                newTranslation.Z = terrainHeight;

                // TODO: Improve bouncing.
                _velocity.Z = Math.Abs(_velocity.Z) * 0.1f;

                if (_moduleData.KillWhenRestingOnGround && _velocity.Z < 0.1f)
                {
                    context.GameObject.Kill(DeathType.Normal, context.Time);
                }
            }

            context.GameObject.SetTranslation(newTranslation);
        }

        public void AddForce(in Vector3 force)
        {
            _cumulativeForces += force;
        }

        internal override void DrawInspector()
        {
            ImGui.InputFloat("Mass", ref _mass);
            ImGui.DragFloat3("Velocity", ref _velocity);
        }

        internal override void Load(StatePersister reader)
        {
            reader.PersistVersion(2);

            base.Load(reader);

            reader.PersistVector3("UnknownVector1", ref _unknownVector1);
            reader.PersistVector3("UnknownVector2", ref _unknownVector2);
            reader.PersistVector3("UnknownVector3", ref _unknownVector3);
            reader.PersistVector3("UnknownVector4", ref _unknownVector4);
            reader.PersistInt32("UnknownInt1", ref _unknownInt1);
            reader.PersistUInt32("UnknownInt2", ref _unknownInt2);
            reader.PersistUInt32("UnknownInt3", ref _unknownInt3);
            reader.PersistSingle("Mass", ref _mass);
            reader.PersistUInt32("UnknownInt4", ref _unknownInt4);
            reader.PersistUInt32("UnknownInt5", ref _unknownInt5);
            reader.PersistFrame("UnknownFrame", ref _unknownFrame);

            reader.SkipUnknownBytes(8);

            reader.PersistSingle("UnknownFloat1", ref _unknownFloat1);
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

        public float Mass { get; private set; } = 1.0f;
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
