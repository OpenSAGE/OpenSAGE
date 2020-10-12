﻿using System;
using System.Numerics;
using OpenSage.Data.Ini;
using OpenSage.Graphics;
using OpenSage.Mathematics;

namespace OpenSage.Logic.Object
{
    public sealed class W3dTruckDraw : W3dModelDraw
    {
        private readonly W3dTruckDrawModuleData _data;

        private readonly (string name, bool affectedBySteering)[] _boneList;

        internal W3dTruckDraw(W3dTruckDrawModuleData data, GameObject gameObject, GameContext context)
            : base(data, gameObject, context)
        {
            _data = data;
            _boneList = new[] {
                (_data.LeftFrontTireBone, true),
                (_data.LeftFrontTireBone2, true),
                (_data.RightFrontTireBone, true),
                (_data.RightFrontTireBone2, true),

                (_data.MidRightFrontTireBone, false),
                (_data.MidLeftFrontTireBone, false),
                (_data.MidRightMidTireBone, false),
                (_data.MidRightMidTireBone2, false),
                (_data.MidLeftMidTireBone, false),
                (_data.MidLeftMidTireBone2, false),
                (_data.MidRightRearTireBone, false),
                (_data.MidLeftRearTireBone, false),

                (_data.LeftRearTireBone, false),
                (_data.LeftRearTireBone2, false),
                (_data.RightRearTireBone, false),
                (_data.RightRearTireBone2, false)
            };
        }

        internal override void Update(in TimeInterval gameTime)
        {
            base.Update(gameTime);

            // Rotating wheels
            var roll = _data.TireRotationMultiplier * GameObject.Speed * gameTime.TotalTime.Milliseconds;

            foreach (var (boneName, affectedBySteering) in _boneList)
            {
                var yaw = 0.0f;

                if (affectedBySteering)
                {
                    yaw = GameObject.SteeringWheelsYaw;
                }

                var boneInstance = FindBoneInstance(boneName);

                if (boneInstance == null)
                {
                    continue;
                }

                boneInstance.AnimatedOffset.Rotation = Quaternion.CreateFromYawPitchRoll(0, 0, yaw);
                boneInstance.AnimatedOffset.Rotation *= Quaternion.CreateFromYawPitchRoll(MathUtility.ToRadians(roll), 0, 0);
            }
        }

        private ModelBoneInstance FindBoneInstance(string name)
        {
            foreach (var bone in ActiveModelInstance.Model.BoneHierarchy.Bones)
            {
                if (bone.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                {
                    return ActiveModelInstance.ModelBoneInstances[bone.Index];
                }
            }

            return null;
        }
    }

    /// <summary>
    /// Hardcoded to call for the TreadDebrisRight and TreadDebrisLeft (unless overriden) particle 
    /// system definitions and allows use of TruckPowerslideSound and TruckLandingSound within the 
    /// UnitSpecificSounds section of the object.
    /// 
    /// This module also includes automatic logic for showing and hiding of HEADLIGHT bones in and 
    /// out of the NIGHT ModelConditionState.
    /// </summary>
    public class W3dTruckDrawModuleData : W3dModelDrawModuleData
    {
        internal static W3dTruckDrawModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        internal static new readonly IniParseTable<W3dTruckDrawModuleData> FieldParseTable = W3dModelDrawModuleData.FieldParseTable
            .Concat(new IniParseTable<W3dTruckDrawModuleData>
            {
                { "CabRotationMultiplier", (parser, x) => x.CabRotationMultiplier = parser.ParseFloat() },
                { "TrailerRotationMultiplier", (parser, x) => x.TrailerRotationMultiplier = parser.ParseFloat() },
                { "CabBone", (parser, x) => x.CabBone = parser.ParseBoneName() },
                { "TrailerBone", (parser, x) => x.TrailerBone = parser.ParseBoneName() },
                { "RotationDamping", (parser, x) => x.RotationDamping = parser.ParseFloat() },

                { "PowerslideRotationAddition", (parser, x) => x.PowerslideRotationAddition = parser.ParseFloat() },
                { "TireRotationMultiplier", (parser, x) => x.TireRotationMultiplier = parser.ParseFloat() },
                { "RightFrontTireBone", (parser, x) => x.RightFrontTireBone = parser.ParseBoneName() },
                { "RightFrontTireBone2", (parser, x) => x.RightFrontTireBone2 = parser.ParseBoneName() },
                { "LeftFrontTireBone", (parser, x) => x.LeftFrontTireBone = parser.ParseBoneName() },
                { "LeftFrontTireBone2", (parser, x) => x.LeftFrontTireBone2 = parser.ParseBoneName() },
                { "MidRightFrontTireBone", (parser, x) => x.MidRightFrontTireBone = parser.ParseBoneName() },
                { "MidLeftFrontTireBone", (parser, x) => x.MidLeftFrontTireBone = parser.ParseBoneName() },
                { "MidRightMidTireBone", (parser, x) => x.MidRightMidTireBone = parser.ParseBoneName() },
                { "MidRightMidTireBone2", (parser, x) => x.MidRightMidTireBone2 = parser.ParseBoneName() },
                { "MidLeftMidTireBone", (parser, x) => x.MidLeftMidTireBone = parser.ParseBoneName() },
                { "MidLeftMidTireBone2", (parser, x) => x.MidLeftMidTireBone2 = parser.ParseBoneName() },
                { "MidRightRearTireBone", (parser, x) => x.MidRightRearTireBone = parser.ParseBoneName() },
                { "MidLeftRearTireBone", (parser, x) => x.MidLeftRearTireBone = parser.ParseBoneName() },
                { "RightRearTireBone", (parser, x) => x.RightRearTireBone = parser.ParseBoneName() },
                { "RightRearTireBone2", (parser, x) => x.RightRearTireBone2 = parser.ParseBoneName() },
                { "LeftRearTireBone", (parser, x) => x.LeftRearTireBone = parser.ParseBoneName() },
                { "LeftRearTireBone2", (parser, x) => x.LeftRearTireBone2 = parser.ParseBoneName() },

                { "Dust", (parser, x) => x.Dust = parser.ParseAssetReference() },
                { "DirtSpray", (parser, x) => x.DirtSpray = parser.ParseAssetReference() },
                { "PowerslideSpray", (parser, x) => x.PowerslideSpray = parser.ParseAssetReference() },
                { "StaticModelLODMode", (parser, x) => x.StaticModelLODMode = parser.ParseBoolean() },
                { "WadingParticleSys", (parser, x) => x.WadingParticleSys = parser.ParseString() },
                { "DependencySharedModelFlags", (parser, x) => x.DependencySharedModelFlags = parser.ParseEnumBitArray<ModelConditionFlag>() },
                { "RandomTexture", (parser, x) => x.RandomTexture = RandomTexture.Parse(parser) }
            });

        // Settings for the attached "cab" model on the vehicle
        public float CabRotationMultiplier { get; private set; }
        public float TrailerRotationMultiplier { get; private set; }
        public string CabBone { get; private set; }
        public string TrailerBone { get; private set; }
        public float RotationDamping { get; private set; }

        // Wheel configuration
        public float PowerslideRotationAddition { get; private set; }
        public float TireRotationMultiplier { get; private set; }
        public string RightFrontTireBone { get; private set; }
        public string RightFrontTireBone2 { get; private set; }
        public string LeftFrontTireBone { get; private set; }
        public string LeftFrontTireBone2 { get; private set; }
        public string MidRightFrontTireBone { get; private set; }
        public string MidLeftFrontTireBone { get; private set; }
        public string MidRightMidTireBone { get; private set; }
        public string MidRightMidTireBone2 { get; private set; }
        public string MidLeftMidTireBone { get; private set; }
        public string MidLeftMidTireBone2 { get; private set; }
        public string MidRightRearTireBone { get; private set; }
        public string MidLeftRearTireBone { get; private set; }
        public string RightRearTireBone { get; private set; }
        public string RightRearTireBone2 { get; private set; }
        public string LeftRearTireBone { get; private set; }
        public string LeftRearTireBone2 { get; private set; }

        // Dust spray configuration
        public string Dust { get; private set; }
        public string DirtSpray { get; private set; }
        public string PowerslideSpray { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public bool StaticModelLODMode { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public string WadingParticleSys { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public BitArray<ModelConditionFlag> DependencySharedModelFlags { get; private set; }

        [AddedIn(SageGame.Bfme2Rotwk)]
        public RandomTexture RandomTexture { get; private set; }

        internal override DrawModule CreateDrawModule(GameObject gameObject, GameContext context)
        {
            return new W3dTruckDraw(this, gameObject, context);
        }
    }
}
