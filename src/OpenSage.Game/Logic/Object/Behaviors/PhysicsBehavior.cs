using System;
using System.Numerics;
using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    public sealed class PhysicsBehavior : BehaviorModule
    {
        private readonly GameObject _gameObject;
        private readonly PhysicsBehaviorModuleData _moduleData;

        // TODO: Don't know if this belongs here.
        private Vector3 _velocity;

        private Vector3 _cumulativeForces;

        public float Mass { get; set; }

        internal PhysicsBehavior(GameObject gameObject, PhysicsBehaviorModuleData moduleData)
        {
            _gameObject = gameObject;
            _moduleData = moduleData;

            Mass = moduleData.Mass;
        }

        internal override void Update(BehaviorUpdateContext context)
        {
            // Calculate force due to gravity.
            var gravity = context.GameContext.AssetLoadContext.AssetStore.GameData.Current.Gravity;
            var gravityAcceleration = Mass * new Vector3(0, 0, gravity);

            var cumulativeAcceleration = _cumulativeForces / Mass;
            _cumulativeForces = Vector3.Zero;

            var acceleration = gravityAcceleration + cumulativeAcceleration;

            // Integrate velocity.
            var deltaTime = (float) context.Time.DeltaTime.TotalSeconds;
            _velocity += acceleration * deltaTime;

            // Integrate position.
            var newTranslation = context.GameObject.Transform.Translation + (_velocity * deltaTime);

            var terrainHeight = context.GameContext.Terrain.HeightMap.GetHeight(
                newTranslation.X,
                newTranslation.Y);

            if (newTranslation.Z < terrainHeight)
            {
                newTranslation.Z = terrainHeight;

                // TODO: Improve bouncing.
                _velocity.Z = Math.Abs(_velocity.Z) * 0.9f;

                if (_moduleData.KillWhenRestingOnGround && _velocity.Z < 0.1f)
                {
                    context.GameObject.Kill(DeathType.Normal);
                }
            }

            context.GameObject.Transform.Translation = newTranslation;
        }

        public void AddForce(in Vector3 force)
        {
            _cumulativeForces += force;
        }
    }

    public sealed class PhysicsBehaviorModuleData : UpdateModuleData
    {
        internal static PhysicsBehaviorModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<PhysicsBehaviorModuleData> FieldParseTable = new IniParseTable<PhysicsBehaviorModuleData>
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

        public float Mass { get; private set; }
        public float AerodynamicFriction { get; private set; }
        public float ForwardFriction { get; private set; }
        public float CenterOfMassOffset { get; private set; }
        public bool AllowBouncing { get; private set; }
        public bool KillWhenRestingOnGround { get; private set; }
        public bool AllowCollideForce { get; private set; } = true;

        [AddedIn(SageGame.Bfme)]
        public float GravityMult { get; private set; }

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

        internal override BehaviorModule CreateModule(GameObject gameObject)
        {
            return new PhysicsBehavior(gameObject, this);
        }
    }
}
