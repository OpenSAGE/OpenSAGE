﻿using System;
using System.IO;
using System.Numerics;
using ImGuiNET;
using OpenSage.Data.Ini;
using OpenSage.Diagnostics.Util;
using OpenSage.FileFormats;

namespace OpenSage.Logic.Object
{
    public sealed class PhysicsBehavior : UpdateModule
    {
        private readonly GameObject _gameObject;
        private readonly PhysicsBehaviorModuleData _moduleData;
        private readonly Vector3 _gravityAcceleration;

        private float _mass;

        // TODO: Don't know if this belongs here.
        private Vector3 _velocity;

        private Vector3 _cumulativeForces;

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
                || _gameObject.Definition.KindOf.Get(ObjectKinds.GiantBird))
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

        internal override void Load(BinaryReader reader)
        {
            var version = reader.ReadVersion();
            if (version != 2)
            {
                throw new InvalidDataException();
            }

            base.Load(reader);

            var unknown1 = reader.ReadBytes(52);

            var unknown2 = reader.ReadUInt32();

            var unknown3 = reader.ReadUInt32();

            var unknown4 = reader.ReadSingle();

            var unknown5 = reader.ReadBytes(20);

            var unknown6 = reader.ReadSingle();
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
